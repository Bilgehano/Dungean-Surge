using Unity.AppUI.UI;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    
    [Header("Attack Range")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackRadius = 1.5f;

    [Header("Attack")]
    [SerializeField] private Transform attackCenter;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damageAmount = -3;
    

    private float attackTimer = 0f;
    private bool isActive = true;
    private Vector2 movementDirection;

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
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log("BossController: Player found: " + player.name);
        }
        else
        {
            Debug.LogWarning("BossController: No object with tag 'Player' found.");
        }
    }

    private void Update()
    {
        if (!isActive || player == null)
        {
            StopMoving();
            return;
        }

        attackTimer -= Time.deltaTime;

        Vector2 attackOrigin = attackCenter != null ? attackCenter.position : transform.position;
        float distanceToPlayer = Vector2.Distance(attackOrigin, player.position);

        if (distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();

            if (attackTimer <= 0f)
            {
                AttackPlayer();
            
                attackTimer = attackCooldown;
            }
        }
    }

    private void FixedUpdate()
    {
        if(rb == null)
        {
            return;
        }

        Vector2 newPosition = rb.position + movementDirection * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        movementDirection = direction;

        UpdateFacingDirection(direction);

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if(direction.x < -0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x > 0.05f)
        {
            spriteRenderer.flipX = true;
        }
    }    

    private void StopMoving()
    {
        movementDirection = Vector2.zero;

        if (animator != null)
        {
        animator.SetBool("IsMoving", false);
        }
    }

    private void AttackPlayer()
    {
        Debug.Log("Boss attacks player");

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(damageAmount);
        }
        else
        {
            Debug.LogWarning("BossController: PlayerHealth script not found on player.");
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = attackCenter != null ? attackCenter.position : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRange);
    }

    public void ActivateBoss()
    {
        isActive = true;
    }

    public void DeactivateBoss()
    {
        isActive = false;
        StopMoving();
    }
}