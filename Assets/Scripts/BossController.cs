using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackCenter;

    [Header("Attack Center Flip Alignment")]
    [Tooltip("Local X position of the visible body center used as mirror pivot for the AttackCenter.")]
    [SerializeField] private float attackCenterVisualPivotX = 0f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Min(0f)]
    [SerializeField] private float followStopDistance = 1.2f;

    [Min(0f)]
    [SerializeField] private float followResumeDistance = 1.5f;

    [Header("Long Distance Chase")]
    [Min(0.1f)]
    [SerializeField] private float globalChaseStepDistance = 6f;

    private BossNavigation navigation;

    private bool isActive = true;
    private bool movementEnabled = true;
    private bool isHoldingCombatPosition;

    private Vector2 movementDirection;
    private float currentMoveSpeed;

    private Vector3 attackCenterBaseLocalPosition;
    private bool hasAttackCenterBaseLocalPosition;

    public Transform Player => player;
    public Animator Animator => animator;
    public bool HasPlayer => player != null;
    public bool IsActive => isActive;

    public Vector2 AttackOrigin
    {
        get
        {
            return attackCenter != null
                ? attackCenter.position
                : transform.position;
        }
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        navigation = GetComponent<BossNavigation>();

        CacheAttackCenterBaseLocalPosition();

        followStopDistance = Mathf.Max(
            0f,
            followStopDistance
        );

        followResumeDistance = Mathf.Max(
            followStopDistance,
            followResumeDistance
        );

        globalChaseStepDistance = Mathf.Max(
            0.1f,
            globalChaseStepDistance
        );

        currentMoveSpeed = moveSpeed;

        UpdateAttackCenterFlip();
    }

    private void OnValidate()
    {
        followStopDistance = Mathf.Max(
            0f,
            followStopDistance
        );

        followResumeDistance = Mathf.Max(
            followStopDistance,
            followResumeDistance
        );

        globalChaseStepDistance = Mathf.Max(
            0.1f,
            globalChaseStepDistance
        );
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "BossController: No object with tag 'Player' found."
            );
        }

        if (navigation != null)
        {
            navigation.ResetNavigation();
        }
    }

    private void Update()
    {
        if (!isActive || player == null)
        {
            StopMoving();
            return;
        }

        if (!movementEnabled)
        {
            return;
        }

        MoveTowardsPlayer();
    }

    private void FixedUpdate()
    {
        if (rb == null ||
            movementDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector2 newPosition =
            rb.position +
            movementDirection *
            currentMoveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    public Vector2 GetAttackOrigin(
        Transform customAttackCenter)
    {
        return customAttackCenter != null
            ? customAttackCenter.position
            : AttackOrigin;
    }

    public float GetDistanceToPlayer()
    {
        return GetDistanceToPlayer(AttackOrigin);
    }

    public float GetDistanceToPlayer(Vector2 attackOrigin)
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(
            attackOrigin,
            player.position
        );
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (movementEnabled == enabled)
        {
            return;
        }

        movementEnabled = enabled;

        if (navigation != null)
        {
            navigation.ResetNavigation();
        }

        if (!movementEnabled)
        {
            StopMoving();
        }
    }

    public void MoveTowardsPlayer()
    {
        if (player == null)
        {
            StopMoving();
            return;
        }

        Vector2 bodyPosition = rb != null
            ? rb.position
            : transform.position;

        Vector2 combatOrigin = AttackOrigin;
        Vector2 playerPosition = player.position;

        Vector2 toPlayerFromCombatOrigin =
            playerPosition - combatOrigin;

        float stopDistance = Mathf.Max(
            0f,
            followStopDistance
        );

        float resumeDistance = Mathf.Max(
            stopDistance,
            followResumeDistance
        );

        if (isHoldingCombatPosition)
        {
            FacePosition(playerPosition);

            if (toPlayerFromCombatOrigin.sqrMagnitude <=
                resumeDistance * resumeDistance)
            {
                StopMoving();
                return;
            }

            isHoldingCombatPosition = false;

            if (navigation != null)
            {
                navigation.ResetNavigation();
            }
        }

        if (toPlayerFromCombatOrigin.sqrMagnitude <=
            stopDistance * stopDistance)
        {
            isHoldingCombatPosition = true;

            StopMoving();
            FacePosition(playerPosition);
            return;
        }

        Vector2 desiredCombatOrigin =
            playerPosition -
            toPlayerFromCombatOrigin.normalized * stopDistance;

        Vector2 combatOriginOffsetFromBody =
            combatOrigin - bodyPosition;

        Vector2 desiredBodyPosition =
            desiredCombatOrigin - combatOriginOffsetFromBody;

        Vector2 toDesiredBodyPosition =
            desiredBodyPosition - bodyPosition;

        float chaseStepDistance = Mathf.Max(
            0.1f,
            globalChaseStepDistance
        );

        Vector2 movementTarget;

        if (toDesiredBodyPosition.sqrMagnitude >
            chaseStepDistance * chaseStepDistance)
        {
            movementTarget =
                bodyPosition +
                toDesiredBodyPosition.normalized *
                chaseStepDistance;
        }
        else
        {
            movementTarget = desiredBodyPosition;
        }

        if (navigation != null)
        {
            bool foundDirection =
                navigation.TryGetMoveDirection(
                    movementTarget,
                    out Vector2 navigationDirection
                );

            if (foundDirection)
            {
                SetMovement(
                    navigationDirection,
                    moveSpeed,
                    true
                );
            }
            else
            {
                StopMoving();
                FacePosition(playerPosition);
            }

            return;
        }

        Vector2 directDirection =
            (movementTarget - bodyPosition).normalized;

        SetMovement(
            directDirection,
            moveSpeed,
            true
        );
    }

    public void SetMovement(
        Vector2 direction,
        float speed,
        bool useWalkAnimation)
    {
        movementDirection =
            direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.zero;

        currentMoveSpeed = speed;

        UpdateFacingDirection(movementDirection);

        if (animator != null)
        {
            animator.SetBool(
                "IsMoving",
                useWalkAnimation &&
                movementDirection != Vector2.zero
            );
        }
    }

    public void StopMoving()
    {
        movementDirection = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
    }

    public void FacePosition(Vector2 targetPosition)
    {
        Vector2 direction =
            targetPosition - (Vector2)transform.position;

        UpdateFacingDirection(direction);
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (direction.x < -0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x > 0.05f)
        {
            spriteRenderer.flipX = true;
        }

        UpdateAttackCenterFlip();
    }

    private void CacheAttackCenterBaseLocalPosition()
    {
        if (attackCenter == null)
        {
            hasAttackCenterBaseLocalPosition = false;
            return;
        }

        attackCenterBaseLocalPosition =
            attackCenter.localPosition;

        hasAttackCenterBaseLocalPosition = true;
    }

    private void UpdateAttackCenterFlip()
    {
        if (attackCenter == null ||
            spriteRenderer == null)
        {
            return;
        }

        if (!hasAttackCenterBaseLocalPosition)
        {
            CacheAttackCenterBaseLocalPosition();
        }

        Vector3 localPosition =
            attackCenterBaseLocalPosition;

        if (spriteRenderer.flipX)
        {
            localPosition.x =
                2f * attackCenterVisualPivotX -
                attackCenterBaseLocalPosition.x;
        }
        else
        {
            localPosition.x =
                attackCenterBaseLocalPosition.x;
        }

        attackCenter.localPosition = localPosition;

        Vector3 scale = attackCenter.localScale;

        float scaleX = Mathf.Max(
            Mathf.Abs(scale.x),
            0.0001f
        );

        scale.x = spriteRenderer.flipX
            ? -scaleX
            : scaleX;

        attackCenter.localScale = scale;
    }

    public bool TryDamagePlayerInFront(
        float width,
        float height,
        int damage)
    {
        return TryDamagePlayerInFront(
            AttackOrigin,
            width,
            height,
            damage
        );
    }

    public bool TryDamagePlayerInFront(
        Vector2 attackOrigin,
        float width,
        float height,
        int damage)
    {
        if (player == null)
        {
            return false;
        }

        Vector2 toPlayer =
            (Vector2)player.position - attackOrigin;

        bool facesRight =
            spriteRenderer != null &&
            spriteRenderer.flipX;

        float forwardDistance = facesRight
            ? toPlayer.x
            : -toPlayer.x;

        if (forwardDistance < 0f ||
            forwardDistance > width)
        {
            return false;
        }

        if (Mathf.Abs(toPlayer.y) > height * 0.5f)
        {
            return false;
        }

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return false;
        }

        playerHealth.ChangeHealth(damage);
        return true;
    }

    public bool TryDamagePlayer(float range, int damage)
    {
        return TryDamagePlayer(
            AttackOrigin,
            range,
            damage
        );
    }

    public bool TryDamagePlayer(
        Vector2 attackOrigin,
        float range,
        int damage)
    {
        if (player == null)
        {
            return false;
        }

        float distance = Vector2.Distance(
            attackOrigin,
            player.position
        );

        if (distance > range)
        {
            return false;
        }

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return false;
        }

        playerHealth.ChangeHealth(damage);
        return true;
    }

    public bool TryDamagePlayerInEllipse(
        float width,
        float height,
        int damage)
    {
        return TryDamagePlayerInEllipse(
            AttackOrigin,
            width,
            height,
            damage
        );
    }

    public bool TryDamagePlayerInEllipse(
        Vector2 attackOrigin,
        float width,
        float height,
        int damage)
    {
        if (player == null)
        {
            return false;
        }

        float horizontalRadius = Mathf.Max(
            width * 0.5f,
            0.01f
        );

        float verticalRadius = Mathf.Max(
            height * 0.5f,
            0.01f
        );

        Vector2 offset =
            (Vector2)player.position - attackOrigin;

        float ellipseValue =
            (offset.x * offset.x) /
            (horizontalRadius * horizontalRadius) +
            (offset.y * offset.y) /
            (verticalRadius * verticalRadius);

        if (ellipseValue > 1f)
        {
            return false;
        }

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return false;
        }

        playerHealth.ChangeHealth(damage);
        return true;
    }

    public void ActivateBoss()
    {
        isActive = true;
        movementEnabled = true;
        isHoldingCombatPosition = false;

        if (navigation != null)
        {
            navigation.TrySnapToNearestWalkablePosition();
            navigation.ResetNavigation();
        }
    }

    public void DeactivateBoss()
    {
        isActive = false;
        movementEnabled = false;
        isHoldingCombatPosition = false;

        if (navigation != null)
        {
            navigation.ResetNavigation();
        }

        StopMoving();
    }
}