using System.Collections.Generic;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private string idleStateName = "Idle";

    [Header("Pathfinding")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float nodeSize = 0.5f;
    [SerializeField] private int gridHalfExtent = 12;
    [SerializeField] private float repathInterval = 0.4f;
    [SerializeField] private float playerMoveRepathThreshold = 0.75f;
    [SerializeField] private float directPathCheckInterval = 0.15f;
    [SerializeField] private float waypointReachDistance = 0.15f;
    [SerializeField] private float stuckVelocityThreshold = 0.05f;
    [SerializeField] private float stuckRepathDelay = 0.35f;

    private Rigidbody2D rb;
    private Vector3 baseScale;
    public Transform player;
    private EnemyState enemyState;
    public float attackRange = 2f;
    private Animator anim;
    private bool isAttackInProgress;
    private float idleUntilTime;
    private float nextRepathTime;
    private float nextDirectPathCheckTime;
    private float stuckStartTime = -1f;
    private List<Vector2> currentPath = new List<Vector2>();
    private int currentWaypointIndex;
    private Vector2 lastRepathTargetPosition;
    private bool hasDirectPath = true;
    private float attackRangeSqr;
    private float waypointReachDistanceSqr;
    private float playerMoveRepathThresholdSqr;
    private float stuckVelocityThresholdSqr;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Enemy_Movement requires a Rigidbody2D component.", this);
            enabled = false;
            return;
        }

        baseScale = transform.localScale;
        rb.freezeRotation = true;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        ChangeState(EnemyState.Chasing);
        attackRangeSqr = attackRange * attackRange;
        waypointReachDistanceSqr = waypointReachDistance * waypointReachDistance;
        playerMoveRepathThresholdSqr = playerMoveRepathThreshold * playerMoveRepathThreshold;
        stuckVelocityThresholdSqr = stuckVelocityThreshold * stuckVelocityThreshold;

        // Stagger path checks/rebuilds so many enemies do not spike on the same frame.
        float jitter = Random.Range(0f, repathInterval);
        nextRepathTime = Time.time + jitter;
        nextDirectPathCheckTime = Time.time + Random.Range(0f, directPathCheckInterval);
        lastRepathTargetPosition = player != null ? (Vector2)player.position : rb.position;
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        switch (enemyState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Chasing:
            case EnemyState.Attacking:
                HandleCombat();
                break;
            case EnemyState.Stunned:
                currentPath.Clear();
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    public void Stun(float duration)
    {
        if (enemyState == EnemyState.Stunned)
        {
            CancelInvoke(nameof(EndStun));
        }

        currentPath.Clear();
        ChangeState(EnemyState.Stunned);
        Invoke(nameof(EndStun), duration);
    }

    private void EndStun()
    {
        if (enemyState == EnemyState.Stunned)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;
        currentPath.Clear();

        if (player == null)
        {
            return;
        }

        FacePlayer();
        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distanceToPlayerSqr = toPlayer.sqrMagnitude;

        if (distanceToPlayerSqr > attackRangeSqr)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        if (Time.time >= idleUntilTime)
        {
            ChangeState(EnemyState.Attacking);
            isAttackInProgress = true;
        }
    }

    void HandleCombat()
    {
        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distanceToPlayerSqr = toPlayer.sqrMagnitude;
        FacePlayer();

        if (isAttackInProgress)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (distanceToPlayerSqr <= attackRangeSqr)
        {
            ChangeState(EnemyState.Attacking);
            isAttackInProgress = true;
            rb.linearVelocity = Vector2.zero;
            currentPath.Clear();
            return;
        }

        ChangeState(EnemyState.Chasing);
        MoveTowardsTarget((Vector2)player.position);
    }

    // Call this from the last frame of the attack animation via Animation Event.
    public void OnAttackAnimationFinished()
    {
        isAttackInProgress = false;
        idleUntilTime = Time.time + attackCooldown;
        ChangeState(EnemyState.Idle);

        // Fallback: force the animator to leave Attack even if transitions are misconfigured.
        if (anim != null)
        {
            int idleHash = Animator.StringToHash(idleStateName);
            if (anim.HasState(0, idleHash))
            {
                anim.CrossFade(idleHash, 0.05f, 0);
            }
            else
            {
                Debug.LogWarning("Idle state name is not found on Animator layer 0: " + idleStateName, this);
            }
        }
    }

    void MoveTowardsTarget(Vector2 targetPosition)
    {
        Vector2 currentPosition = rb.position;

        if (Time.time >= nextDirectPathCheckTime)
        {
            hasDirectPath = HasDirectPath(currentPosition, targetPosition);
            nextDirectPathCheckTime = Time.time + directPathCheckInterval;
        }

        if (hasDirectPath)
        {
            currentPath.Clear();
            currentWaypointIndex = 0;
            stuckStartTime = -1f;
            Vector2 directDirection = (targetPosition - currentPosition).normalized;
            rb.linearVelocity = directDirection * moveSpeed;
            return;
        }

        bool targetMovedEnough = (targetPosition - lastRepathTargetPosition).sqrMagnitude >= playerMoveRepathThresholdSqr;
        bool hasNoPath = currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count;
        bool periodicRepath = Time.time >= nextRepathTime;
        bool shouldForceStuckRepath = false;

        if (!hasNoPath)
        {
            Vector2 waypoint = currentPath[currentWaypointIndex];
            float waypointDistanceSqr = (waypoint - currentPosition).sqrMagnitude;
            if (waypointDistanceSqr > waypointReachDistanceSqr * 4f && rb.linearVelocity.sqrMagnitude <= stuckVelocityThresholdSqr)
            {
                if (stuckStartTime < 0f)
                {
                    stuckStartTime = Time.time;
                }
                else if (Time.time - stuckStartTime >= stuckRepathDelay)
                {
                    shouldForceStuckRepath = true;
                }
            }
            else
            {
                stuckStartTime = -1f;
            }
        }

        if (periodicRepath || targetMovedEnough || hasNoPath || shouldForceStuckRepath)
        {
            currentPath = FindPathAStar(currentPosition, targetPosition);
            currentWaypointIndex = 0;
            nextRepathTime = Time.time + repathInterval;
            lastRepathTargetPosition = targetPosition;
            stuckStartTime = -1f;
        }

        if (currentPath.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        while (currentWaypointIndex < currentPath.Count &&
         (currentPath[currentWaypointIndex] - currentPosition).sqrMagnitude <= waypointReachDistanceSqr)
        {
            currentWaypointIndex++;
        }

        if (currentWaypointIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (currentPath[currentWaypointIndex] - currentPosition).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    bool HasDirectPath(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider == null;
    }

    List<Vector2> FindPathAStar(Vector2 startPosition, Vector2 targetPosition)
    {
        List<Vector2> result = new List<Vector2>();
        int size = gridHalfExtent * 2 + 1;
        Vector2 gridCenter = (startPosition + targetPosition) * 0.5f;
        PathNode[,] grid = new PathNode[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 world = GridToWorld(x, y, gridCenter);
                bool walkable = !Physics2D.OverlapCircle(world, nodeSize * 0.45f, obstacleMask);
                grid[x, y] = new PathNode(x, y, world, walkable);
            }
        }

        Vector2Int startIndex = WorldToGrid(startPosition, gridCenter);
        Vector2Int targetIndex = WorldToGrid(targetPosition, gridCenter);
        if (!IsInBounds(startIndex, size) || !IsInBounds(targetIndex, size))
        {
            return result;
        }

        grid[startIndex.x, startIndex.y].walkable = true;
        grid[targetIndex.x, targetIndex.y].walkable = true;

        PathNode startNode = grid[startIndex.x, startIndex.y];
        PathNode targetNode = grid[targetIndex.x, targetIndex.y];
        List<PathNode> openSet = new List<PathNode> { startNode };
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(startNode, targetNode);

        while (openSet.Count > 0)
        {
            PathNode current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                PathNode candidate = openSet[i];
                if (candidate.FCost < current.FCost ||
                    (candidate.FCost == current.FCost && candidate.hCost < current.hCost))
                {
                    current = candidate;
                }
            }

            if (current == targetNode)
            {
                return ReconstructPath(startNode, targetNode);
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

                    int nx = current.x + dx;
                    int ny = current.y + dy;
                    if (nx < 0 || nx >= size || ny < 0 || ny >= size)
                    {
                        continue;
                    }

                    if (dx != 0 && dy != 0)
                    {
                        PathNode sideA = grid[current.x + dx, current.y];
                        PathNode sideB = grid[current.x, current.y + dy];
                        if (!sideA.walkable || !sideB.walkable)
                        {
                            continue;
                        }
                    }

                    PathNode neighbor = grid[nx, ny];
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    int tentativeG = current.gCost + GetMoveCost(current, neighbor);
                    if (tentativeG < neighbor.gCost)
                    {
                        neighbor.parent = current;
                        neighbor.gCost = tentativeG;
                        neighbor.hCost = GetHeuristic(neighbor, targetNode);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }

        return result;
    }

    List<Vector2> ReconstructPath(PathNode startNode, PathNode endNode)
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

    Vector2 GridToWorld(int x, int y, Vector2 center)
    {
        float offsetX = (x - gridHalfExtent) * nodeSize;
        float offsetY = (y - gridHalfExtent) * nodeSize;
        return center + new Vector2(offsetX, offsetY);
    }

    Vector2Int WorldToGrid(Vector2 world, Vector2 center)
    {
        float localX = (world.x - center.x) / nodeSize;
        float localY = (world.y - center.y) / nodeSize;
        int x = Mathf.RoundToInt(localX) + gridHalfExtent;
        int y = Mathf.RoundToInt(localY) + gridHalfExtent;
        return new Vector2Int(x, y);
    }

    bool IsInBounds(Vector2Int p, int size)
    {
        return p.x >= 0 && p.x < size && p.y >= 0 && p.y < size;
    }

    int GetMoveCost(PathNode from, PathNode to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return (dx == 1 && dy == 1) ? 14 : 10;
    }

    int GetHeuristic(PathNode from, PathNode to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        int diagonal = Mathf.Min(dx, dy);
        int straight = Mathf.Abs(dx - dy);
        return 14 * diagonal + 10 * straight;
    }

    void FacePlayer()
    {
        float horizontalOffset = player.position.x - transform.position.x;

        if (Mathf.Abs(horizontalOffset) < 0.01f)
        {
            return;
        }

        float facingX = horizontalOffset > 0 ? Mathf.Abs(baseScale.x) : -Mathf.Abs(baseScale.x);
        transform.localScale = new Vector3(facingX, baseScale.y, baseScale.z);
    }

    void ChangeState(EnemyState newState)
    {
        if (anim == null)
        {
            enemyState = newState;
            return;
        }

        if (newState == EnemyState.Idle)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isIdle", true);
            anim.SetBool("isAttacking", false);
        }
        else if (newState == EnemyState.Chasing)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isMoving", true);
            anim.SetBool("isAttacking", false);
        }
        else if (newState == EnemyState.Attacking)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isIdle", false);
            anim.SetBool("isAttacking", true);
        }
        else if (newState == EnemyState.Stunned)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isIdle", true);
            anim.SetBool("isAttacking", false);
        }

        enemyState = newState;
    }

    class PathNode
    {
        public int x;
        public int y;
        public Vector2 world;
        public bool walkable;
        public int gCost = int.MaxValue;
        public int hCost;
        public PathNode parent;

        public int FCost => gCost + hCost;

        public PathNode(int x, int y, Vector2 world, bool walkable)
        {
            this.x = x;
            this.y = y;
            this.world = world;
            this.walkable = walkable;
        }
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Stunned
}

