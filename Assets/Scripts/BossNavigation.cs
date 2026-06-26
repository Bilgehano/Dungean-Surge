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
    [SerializeField] private float obstacleCheckRadius = 0.55f;

    [Header("Repathing")]
    [SerializeField] private float repathInterval = 0.4f;
    [SerializeField] private float playerMoveRepathThreshold = 0.75f;
    [SerializeField] private float directPathCheckInterval = 0.15f;
    [SerializeField] private float waypointReachDistance = 0.2f;
    [SerializeField] private float stuckVelocityThreshold = 0.05f;
    [SerializeField] private float stuckRepathDelay = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool drawPathGizmos;

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

        ValidateSettings();
        RefreshCachedValues();
        ResetNavigation();
    }

    private void OnValidate()
    {
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
        Vector2 directDifference = targetPosition - currentPosition;

        if (directDifference.sqrMagnitude <= waypointReachDistanceSqr)
        {
            ClearCurrentPath();
            return false;
        }

        // Falls keine Hindernis-Layer gesetzt sind:
        // Boss läuft einfach direkt zum Ziel.
        if (obstacleMask.value == 0)
        {
            ClearCurrentPath();
            moveDirection = directDifference.normalized;
            return true;
        }

        if (Time.time >= nextDirectPathCheckTime)
        {
            hasDirectPath = HasDirectPath(
                currentPosition,
                targetPosition
            );

            nextDirectPathCheckTime =
                Time.time + directPathCheckInterval;
        }

        // Direkter Weg ist frei.
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

        bool periodicRepath = Time.time >= nextRepathTime;
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

        if (periodicRepath ||
            targetMovedEnough ||
            hasNoPath ||
            shouldForceStuckRepath)
        {
            currentPath = FindPathAStar(
                currentPosition,
                targetPosition
            );

            currentWaypointIndex = 0;
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

    private bool HasDirectPath(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;

        if (distance <= waypointReachDistance)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            from,
            obstacleCheckRadius,
            direction / distance,
            distance,
            obstacleMask
        );

        return hit.collider == null;
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

                bool walkable = !Physics2D.OverlapCircle(
                    worldPosition,
                    obstacleCheckRadius,
                    obstacleMask
                );

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
        grid[targetIndex.x, targetIndex.y].walkable = true;

        PathNode startNode =
            grid[startIndex.x, startIndex.y];

        PathNode targetNode =
            grid[targetIndex.x, targetIndex.y];

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
                return ReconstructPath(
                    startNode,
                    targetNode
                );
            }

            openSet.Remove(current);
            closedSet.Add(current);

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

                    // Kein diagonales Schneiden durch Ecken.
                    if (dx != 0 && dy != 0)
                    {
                        PathNode sideA =
                            grid[current.x + dx, current.y];

                        PathNode sideB =
                            grid[current.x, current.y + dy];

                        if (!sideA.walkable || !sideB.walkable)
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

                    if (tentativeG < neighbour.gCost)
                    {
                        neighbour.parent = current;
                        neighbour.gCost = tentativeG;
                        neighbour.hCost =
                            GetHeuristic(
                                neighbour,
                                targetNode
                            );

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }
        }

        return result;
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

    private bool IsInBounds(Vector2Int point, int size)
    {
        return point.x >= 0 &&
               point.x < size &&
               point.y >= 0 &&
               point.y < size;
    }

    private int GetMoveCost(PathNode from, PathNode to)
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

    private void ValidateSettings()
    {
        nodeSize = Mathf.Max(0.1f, nodeSize);
        gridHalfExtent = Mathf.Max(2, gridHalfExtent);

        obstacleCheckRadius =
            Mathf.Max(0.05f, obstacleCheckRadius);

        repathInterval = Mathf.Max(0.05f, repathInterval);

        directPathCheckInterval =
            Mathf.Max(0.05f, directPathCheckInterval);

        waypointReachDistance =
            Mathf.Max(0.05f, waypointReachDistance);

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