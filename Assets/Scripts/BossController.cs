using UnityEngine;

public class Boss_Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damageAmount = -20;

    private float attackTimer;
    private bool isActive = true;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
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
            return;
        }

        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

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

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void AttackPlayer()
    {
        Debug.Log("Boss attacks player");

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(damageAmount);
        }
        else
        {
            Debug.LogWarning("Boss_Controller: PlayerHealth script not found on player.");
        }
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