using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class BossNavigation : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float nodeSize = 0.5f;
    [SerializeField] private int gridHalfExtent = 16;

    [Header("Boss Collision")]
    [SerializeField] private BoxCollider2D bossCollider;

    [Header("Navigation Padding (World Directions)")]
    [SerializeField] private float topPadding = 0.05f;
    [SerializeField] private float bottomPadding = 0.05f;
    [SerializeField] private float leftPadding = 0.05f;
    [SerializeField] private float rightPadding = 0.05f;

    [SerializeField, HideInInspector]
    private float collisionPadding = 0.05f;

    [SerializeField, HideInInspector]
    private bool hasMigratedCollisionPadding;

    [Header("Repathing")]
    [SerializeField] private float repathInterval = 0.4f;
    [SerializeField] private float playerMoveRepathThreshold = 0.75f;
    [SerializeField] private float directPathCheckInterval = 0.15f;
    [SerializeField] private float waypointReachDistance = 0.2f;
    [SerializeField] private float stuckVelocityThreshold = 0.05f;
    [SerializeField] private float stuckRepathDelay = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool drawPathGizmos;

    [Header("Path Quality")]
    [SerializeField] private bool smoothCalculatedPath = true;

    private Rigidbody2D rb;

    private List<Vector2> currentPath = new List<Vector2>();
    private int currentWaypointIndex;

    private float nextRepathTime;
    private float nextDirectPathCheckTime;
    private float stuckStartTime = -1f;

    private Vector2 lastRepathTargetPosition;
    private bool hasDirectPath = true;

    private float waypointReachDistanceSqr;
    private float playerMoveRepathThresholdSqr;
    private float stuckVelocityThresholdSqr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bossCollider == null)
        {
            bossCollider = GetComponent<BoxCollider2D>();
        }

        if (bossCollider == null)
        {
            Debug.LogError(
                "BossNavigation requires a BoxCollider2D component.",
                this
            );

            enabled = false;
            return;
        }

        MigrateLegacyCollisionPaddingIfNeeded();
        ValidateSettings();
        RefreshCachedValues();
        ResetNavigation();
    }

    private void OnValidate()
    {
        if (bossCollider == null)
        {
            bossCollider = GetComponent<BoxCollider2D>();
        }

        MigrateLegacyCollisionPaddingIfNeeded();
        ValidateSettings();
        RefreshCachedValues();
    }

    public bool TryGetMoveDirection(
        Vector2 targetPosition,
        out Vector2 moveDirection)
    {
        moveDirection = Vector2.zero;

        if (rb == null)
        {
            return false;
        }

        Vector2 currentPosition = rb.position;
        Vector2 directDifference =
            targetPosition - currentPosition;

        if (directDifference.sqrMagnitude <=
            waypointReachDistanceSqr)
        {
            ClearCurrentPath();
            return false;
        }

        if (obstacleMask.value == 0)
        {
            ClearCurrentPath();
            moveDirection = directDifference.normalized;
            return true;
        }

        bool targetIsWalkable =
            IsWalkable(targetPosition);

        if (!targetIsWalkable)
        {
            hasDirectPath = false;
        }
        else if (Time.time >= nextDirectPathCheckTime)
        {
            hasDirectPath = HasDirectPath(
                currentPosition,
                targetPosition
            );

            nextDirectPathCheckTime =
                Time.time + directPathCheckInterval;
        }

        if (hasDirectPath)
        {
            ClearCurrentPath();
            moveDirection = directDifference.normalized;
            return true;
        }

        bool targetMovedEnough =
            (targetPosition - lastRepathTargetPosition).sqrMagnitude
            >= playerMoveRepathThresholdSqr;

        bool hasNoPath =
            currentPath.Count == 0 ||
            currentWaypointIndex >= currentPath.Count;

        bool periodicRepath =
            Time.time >= nextRepathTime;

        bool shouldForceStuckRepath = false;

        if (!hasNoPath)
        {
            Vector2 currentWaypoint =
                currentPath[currentWaypointIndex];

            float waypointDistanceSqr =
                (currentWaypoint - currentPosition).sqrMagnitude;

            if (waypointDistanceSqr >
                    waypointReachDistanceSqr * 4f &&
                rb.linearVelocity.sqrMagnitude <=
                    stuckVelocityThresholdSqr)
            {
                if (stuckStartTime < 0f)
                {
                    stuckStartTime = Time.time;
                }
                else if (Time.time - stuckStartTime >=
                         stuckRepathDelay)
                {
                    shouldForceStuckRepath = true;
                }
            }
            else
            {
                stuckStartTime = -1f;
            }
        }

        bool shouldRepath =
            targetMovedEnough ||
            hasNoPath ||
            shouldForceStuckRepath ||
            periodicRepath;

        if (shouldRepath)
        {
            List<Vector2> newlyCalculatedPath = FindPathAStar(
                currentPosition,
                targetPosition
            );

            if (newlyCalculatedPath.Count > 0)
            {
                currentPath = newlyCalculatedPath;
                currentWaypointIndex = 0;
            }
            else
            {
                ClearCurrentPath();
            }

            nextRepathTime = Time.time + repathInterval;
            lastRepathTargetPosition = targetPosition;
            stuckStartTime = -1f;
        }

        if (currentPath.Count == 0)
        {
            return false;
        }

        while (currentWaypointIndex < currentPath.Count &&
               (currentPath[currentWaypointIndex] -
                currentPosition).sqrMagnitude
               <= waypointReachDistanceSqr)
        {
            currentWaypointIndex++;
        }

        if (currentWaypointIndex >= currentPath.Count)
        {
            return false;
        }

        moveDirection =
            (currentPath[currentWaypointIndex] -
             currentPosition).normalized;

        return true;
    }

    public void ResetNavigation()
    {
        ClearCurrentPath();

        nextRepathTime = 0f;
        nextDirectPathCheckTime = 0f;
        stuckStartTime = -1f;
        hasDirectPath = true;

        lastRepathTargetPosition = rb != null
            ? rb.position
            : transform.position;
    }

    public bool TrySnapToNearestWalkablePosition()
    {
        if (rb == null)
        {
            return false;
        }

        Vector2 currentPosition = rb.position;

        if (IsWalkable(currentPosition))
        {
            return true;
        }

        float stepDistance = Mathf.Max(nodeSize, 0.1f);
        float colliderDiameter = GetNavigationBoxSize().magnitude;

        int maxRingCount = Mathf.Max(
            4,
            Mathf.CeilToInt(colliderDiameter / stepDistance) + 4
        );

        for (int ring = 1; ring <= maxRingCount; ring++)
        {
            float radius = ring * stepDistance;
            int sampleCount = Mathf.Max(8, ring * 8);

            for (int sampleIndex = 0;
                 sampleIndex < sampleCount;
                 sampleIndex++)
            {
                float angle =
                    sampleIndex /
                    (float)sampleCount *
                    Mathf.PI * 2f;

                Vector2 candidatePosition =
                    currentPosition +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) * radius;

                if (!IsWalkable(candidatePosition))
                {
                    continue;
                }

                rb.position = candidatePosition;

                Vector3 worldPosition = transform.position;
                worldPosition.x = candidatePosition.x;
                worldPosition.y = candidatePosition.y;
                transform.position = worldPosition;

                ResetNavigation();
                return true;
            }
        }

        return false;
    }

    private bool HasDirectPath(Vector2 from, Vector2 to)
    {
        if (!IsWalkable(to))
        {
            return false;
        }

        Vector2 direction = to - from;
        float distance = direction.magnitude;

        if (distance <= waypointReachDistance)
        {
            return true;
        }

        Vector2 colliderCenter =
            from + GetColliderCenterOffset();

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            colliderCenter,
            GetNavigationBoxSize(),
            0f,
            direction / distance,
            distance,
            obstacleMask
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;

            if (hitCollider == null)
            {
                continue;
            }

            if (bossCollider != null &&
                hitCollider == bossCollider)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private bool IsWalkable(Vector2 bodyPosition)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            bodyPosition + GetColliderCenterOffset(),
            GetNavigationBoxSize(),
            0f
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
            {
                continue;
            }

            if (bossCollider != null &&
                hit == bossCollider)
            {
                continue;
            }

            if (((1 << hit.gameObject.layer) &
                 obstacleMask.value) == 0)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private Vector2 GetColliderCenterOffset()
    {
        if (bossCollider == null || rb == null)
        {
            return Vector2.zero;
        }

        Vector2 colliderOffset =
            (Vector2)bossCollider.bounds.center - rb.position;

        float horizontalOffset =
            (rightPadding - leftPadding) * 0.5f;

        float verticalOffset =
            (topPadding - bottomPadding) * 0.5f;

        return colliderOffset +
            new Vector2(horizontalOffset, verticalOffset);
    }

    private Vector2 GetNavigationBoxSize()
    {
        if (bossCollider == null)
        {
            return Vector2.one;
        }

        Vector2 colliderSize = bossCollider.bounds.size;

        return colliderSize + new Vector2(
            leftPadding + rightPadding,
            topPadding + bottomPadding
        );
    }

    private List<Vector2> FindPathAStar(
        Vector2 startPosition,
        Vector2 targetPosition)
    {
        List<Vector2> result = new List<Vector2>();

        int size = gridHalfExtent * 2 + 1;

        Vector2 gridCenter =
            (startPosition + targetPosition) * 0.5f;

        PathNode[,] grid = new PathNode[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 worldPosition =
                    GridToWorld(x, y, gridCenter);

                bool walkable = IsWalkable(worldPosition);

                grid[x, y] = new PathNode(
                    x,
                    y,
                    worldPosition,
                    walkable
                );
            }
        }

        Vector2Int startIndex =
            WorldToGrid(startPosition, gridCenter);

        Vector2Int targetIndex =
            WorldToGrid(targetPosition, gridCenter);

        if (!IsInBounds(startIndex, size) ||
            !IsInBounds(targetIndex, size))
        {
            return result;
        }

        grid[startIndex.x, startIndex.y].walkable = true;

        if (grid[targetIndex.x, targetIndex.y].walkable)
        {
            result = FindPathToTarget(
                grid,
                startIndex,
                targetIndex,
                startPosition
            );

            if (result.Count > 0)
            {
                return result;
            }
        }

        return FindPathToClosestReachableTarget(
            grid,
            startIndex,
            targetPosition,
            startPosition
        );
    }

    private List<Vector2> FindPathToTarget(
        PathNode[,] grid,
        Vector2Int startIndex,
        Vector2Int targetIndex,
        Vector2 startPosition)
    {
        int size = grid.GetLength(0);

        ResetPathData(grid, size);

        PathNode startNode =
            grid[startIndex.x, startIndex.y];

        PathNode targetNode =
            grid[targetIndex.x, targetIndex.y];

        if (startNode == targetNode)
        {
            return new List<Vector2>();
        }

        List<PathNode> openSet =
            new List<PathNode> { startNode };

        HashSet<PathNode> closedSet =
            new HashSet<PathNode>();

        startNode.gCost = 0;
        startNode.hCost =
            GetHeuristic(startNode, targetNode);

        while (openSet.Count > 0)
        {
            PathNode current = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                PathNode candidate = openSet[i];

                if (candidate.FCost < current.FCost ||
                    (candidate.FCost == current.FCost &&
                     candidate.hCost < current.hCost))
                {
                    current = candidate;
                }
            }

            if (current == targetNode)
            {
                List<Vector2> path = ReconstructPath(
                    startNode,
                    targetNode
                );

                if (smoothCalculatedPath && path.Count > 1)
                {
                    path = SmoothPath(
                        path,
                        startPosition
                    );
                }

                return path;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            AddValidNeighbours(
                grid,
                current,
                targetNode,
                openSet,
                closedSet
            );
        }

        return new List<Vector2>();
    }

    private List<Vector2> FindPathToClosestReachableTarget(
        PathNode[,] grid,
        Vector2Int startIndex,
        Vector2 desiredTargetPosition,
        Vector2 startPosition)
    {
        int size = grid.GetLength(0);

        ResetPathData(grid, size);

        PathNode startNode =
            grid[startIndex.x, startIndex.y];

        List<PathNode> openSet =
            new List<PathNode> { startNode };

        HashSet<PathNode> closedSet =
            new HashSet<PathNode>();

        startNode.gCost = 0;

        PathNode closestReachableNode = startNode;

        float closestDistanceSqr =
            (startNode.world -
             desiredTargetPosition).sqrMagnitude;

        while (openSet.Count > 0)
        {
            PathNode current = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].gCost < current.gCost)
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            float distanceToDesiredSqr =
                (current.world -
                 desiredTargetPosition).sqrMagnitude;

            if (distanceToDesiredSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceToDesiredSqr;
                closestReachableNode = current;
            }

            AddValidNeighbours(
                grid,
                current,
                null,
                openSet,
                closedSet
            );
        }

        if (closestReachableNode == startNode)
        {
            return new List<Vector2>();
        }

        List<Vector2> path = ReconstructPath(
            startNode,
            closestReachableNode
        );

        if (smoothCalculatedPath && path.Count > 1)
        {
            path = SmoothPath(path, startPosition);
        }

        return path;
    }

    private void AddValidNeighbours(
        PathNode[,] grid,
        PathNode current,
        PathNode targetNode,
        List<PathNode> openSet,
        HashSet<PathNode> closedSet)
    {
        int size = grid.GetLength(0);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                int nextX = current.x + dx;
                int nextY = current.y + dy;

                if (nextX < 0 || nextX >= size ||
                    nextY < 0 || nextY >= size)
                {
                    continue;
                }

                if (dx != 0 && dy != 0)
                {
                    PathNode sideA =
                        grid[current.x + dx, current.y];

                    PathNode sideB =
                        grid[current.x, current.y + dy];

                    if (!sideA.walkable ||
                        !sideB.walkable)
                    {
                        continue;
                    }
                }

                PathNode neighbour =
                    grid[nextX, nextY];

                if (!neighbour.walkable ||
                    closedSet.Contains(neighbour))
                {
                    continue;
                }

                int tentativeG =
                    current.gCost +
                    GetMoveCost(current, neighbour);

                if (tentativeG >= neighbour.gCost)
                {
                    continue;
                }

                neighbour.parent = current;
                neighbour.gCost = tentativeG;

                neighbour.hCost = targetNode != null
                    ? GetHeuristic(neighbour, targetNode)
                    : 0;

                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                }
            }
        }
    }

    private void ResetPathData(
        PathNode[,] grid,
        int size)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                PathNode node = grid[x, y];

                node.gCost = int.MaxValue;
                node.hCost = 0;
                node.parent = null;
            }
        }
    }

    private List<Vector2> ReconstructPath(
        PathNode startNode,
        PathNode endNode)
    {
        List<Vector2> path = new List<Vector2>();
        PathNode current = endNode;

        while (current != startNode)
        {
            path.Add(current.world);
            current = current.parent;

            if (current == null)
            {
                return new List<Vector2>();
            }
        }

        path.Reverse();
        return path;
    }

    private List<Vector2> SmoothPath(
        List<Vector2> rawPath,
        Vector2 startPosition)
    {
        if (rawPath == null || rawPath.Count <= 1)
        {
            return rawPath;
        }

        List<Vector2> smoothedPath = new List<Vector2>();
        Vector2 anchor = startPosition;
        int index = 0;

        while (index < rawPath.Count)
        {
            int furthestVisible = index;

            for (int candidate = rawPath.Count - 1;
                 candidate >= index;
                 candidate--)
            {
                if (HasDirectPath(
                        anchor,
                        rawPath[candidate]))
                {
                    furthestVisible = candidate;
                    break;
                }
            }

            smoothedPath.Add(rawPath[furthestVisible]);
            anchor = rawPath[furthestVisible];
            index = furthestVisible + 1;
        }

        return smoothedPath;
    }

    private Vector2 GridToWorld(
        int x,
        int y,
        Vector2 center)
    {
        float offsetX =
            (x - gridHalfExtent) * nodeSize;

        float offsetY =
            (y - gridHalfExtent) * nodeSize;

        return center + new Vector2(offsetX, offsetY);
    }

    private Vector2Int WorldToGrid(
        Vector2 worldPosition,
        Vector2 center)
    {
        float localX =
            (worldPosition.x - center.x) / nodeSize;

        float localY =
            (worldPosition.y - center.y) / nodeSize;

        int x =
            Mathf.RoundToInt(localX) + gridHalfExtent;

        int y =
            Mathf.RoundToInt(localY) + gridHalfExtent;

        return new Vector2Int(x, y);
    }

    private bool IsInBounds(
        Vector2Int point,
        int size)
    {
        return point.x >= 0 &&
               point.x < size &&
               point.y >= 0 &&
               point.y < size;
    }

    private int GetMoveCost(
        PathNode from,
        PathNode to)
    {
        int xDistance = Mathf.Abs(from.x - to.x);
        int yDistance = Mathf.Abs(from.y - to.y);

        return xDistance == 1 && yDistance == 1
            ? 14
            : 10;
    }

    private int GetHeuristic(
        PathNode from,
        PathNode to)
    {
        int xDistance = Mathf.Abs(from.x - to.x);
        int yDistance = Mathf.Abs(from.y - to.y);

        int diagonalMoves =
            Mathf.Min(xDistance, yDistance);

        int straightMoves =
            Mathf.Abs(xDistance - yDistance);

        return 14 * diagonalMoves + 10 * straightMoves;
    }

    private void ClearCurrentPath()
    {
        currentPath.Clear();
        currentWaypointIndex = 0;
        stuckStartTime = -1f;
    }

    private void MigrateLegacyCollisionPaddingIfNeeded()
    {
        if (hasMigratedCollisionPadding)
        {
            return;
        }

        float legacyPadding = Mathf.Max(
            0f,
            collisionPadding
        );

        topPadding = legacyPadding;
        bottomPadding = legacyPadding;
        leftPadding = legacyPadding;
        rightPadding = legacyPadding;

        hasMigratedCollisionPadding = true;
    }

    private void ValidateSettings()
    {
        nodeSize = Mathf.Max(0.1f, nodeSize);
        gridHalfExtent = Mathf.Max(2, gridHalfExtent);

        collisionPadding = Mathf.Max(
            0f,
            collisionPadding
        );

        topPadding = Mathf.Max(0f, topPadding);
        bottomPadding = Mathf.Max(0f, bottomPadding);
        leftPadding = Mathf.Max(0f, leftPadding);
        rightPadding = Mathf.Max(0f, rightPadding);

        repathInterval = Mathf.Max(0.05f, repathInterval);

        directPathCheckInterval =
            Mathf.Max(0.05f, directPathCheckInterval);

        waypointReachDistance =
            Mathf.Max(0.05f, waypointReachDistance);

        playerMoveRepathThreshold =
            Mathf.Max(0.05f, playerMoveRepathThreshold);

        stuckVelocityThreshold =
            Mathf.Max(0.01f, stuckVelocityThreshold);

        stuckRepathDelay =
            Mathf.Max(0.05f, stuckRepathDelay);
    }

    private void RefreshCachedValues()
    {
        waypointReachDistanceSqr =
            waypointReachDistance * waypointReachDistance;

        playerMoveRepathThresholdSqr =
            playerMoveRepathThreshold *
            playerMoveRepathThreshold;

        stuckVelocityThresholdSqr =
            stuckVelocityThreshold *
            stuckVelocityThreshold;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawPathGizmos ||
            currentPath == null ||
            currentPath.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        Vector3 previousPoint = transform.position;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 waypoint = currentPath[i];

            Gizmos.DrawLine(previousPoint, waypoint);
            Gizmos.DrawWireSphere(waypoint, 0.08f);

            previousPoint = waypoint;
        }
    }

    private class PathNode
    {
        public int x;
        public int y;
        public Vector2 world;
        public bool walkable;

        public int gCost = int.MaxValue;
        public int hCost;
        public PathNode parent;

        public int FCost => gCost + hCost;

        public PathNode(
            int x,
            int y,
            Vector2 world,
            bool walkable)
        {
            this.x = x;
            this.y = y;
            this.world = world;
            this.walkable = walkable;
        }
    }
}