using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackCenter;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    private BossNavigation navigation;

    private bool isActive = true;
    private bool movementEnabled = true;
    private Vector2 movementDirection;
    private float currentMoveSpeed;

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
        currentMoveSpeed = moveSpeed;
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

    public float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(
            AttackOrigin,
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

        if (navigation != null)
        {
            bool foundDirection =
                navigation.TryGetMoveDirection(
                    player.position,
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
            }

            return;
        }

        Vector2 directDirection =
            (player.position - transform.position).normalized;

        SetMovement(directDirection, moveSpeed, true);
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
    }

    // Rechteckiger Trefferbereich nur vor dem Boss.
    public bool TryDamagePlayerInFront(
        float range,
        float width,
        int damage)
    {
        if (player == null)
        {
            return false;
        }

        Vector2 toPlayer =
            (Vector2)player.position - AttackOrigin;

        bool facesRight =
            spriteRenderer != null &&
            spriteRenderer.flipX;

        float forwardDistance = facesRight
            ? toPlayer.x
            : -toPlayer.x;

        if (forwardDistance < 0f ||
            forwardDistance > range)
        {
            return false;
        }

        if (Mathf.Abs(toPlayer.y) > width * 0.5f)
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

    // Kreisförmiger Angriff für Charge, Stomp usw.
    public bool TryDamagePlayer(float range, int damage)
    {
        if (player == null)
        {
            return false;
        }

        float distance = Vector2.Distance(
            AttackOrigin,
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

    public void ActivateBoss()
    {
        isActive = true;
        movementEnabled = true;

        if (navigation != null)
        {
            navigation.ResetNavigation();
        }
    }

    public void DeactivateBoss()
    {
        isActive = false;
        movementEnabled = false;

        if (navigation != null)
        {
            navigation.ResetNavigation();
        }

        StopMoving();
    }
}