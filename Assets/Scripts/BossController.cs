using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Transform attackCenter;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Attack Ranges")]
    [SerializeField] private float normalAttackRange = 1.5f;
    [SerializeField] private float throwMinRange = 2f;
    [SerializeField] private float throwMaxRange = 7f;
    [SerializeField] private float heavyAttackRange = 2f;

    [Header("Charge Attack")]
    [SerializeField] private float chargeStartMaxRange = 8f;
    [SerializeField] private float chargeDamageRadius = 1.4f;
    [SerializeField] private float chargeSpeed = 9f;
    [SerializeField] private float chargeStopDistance = 0.15f;
    [SerializeField] private float chargeWindupTime = 0.8f;
    [SerializeField] private float chargeMaxDuration = 1.5f;
    [SerializeField] private float chargeEndLag = 0.4f;
    [SerializeField] private float chargeCooldown = 8f;

    [Header("Attack Cooldowns")]
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float throwAttackCooldown = 4f;
    [SerializeField] private float heavyAttackCooldown = 7f;

    [Header("Attack Lock Times")]
    [SerializeField] private float normalAttackLockTime = 0.6f;
    [SerializeField] private float throwAttackLockTime = 0.8f;
    [SerializeField] private float heavyAttackLockTime = 1.0f;

    [Header("Damage")]
    [SerializeField] private int normalDamage = -3;
    [SerializeField] private int throwDamage = -2;
    [SerializeField] private int heavyDamage = -10;
    [SerializeField] private int chargeDamage = -8;

    private bool isActive = true;
    private bool isAttacking;
    private bool isCharging;

    private Vector2 movementDirection;
    private Vector2 chargeTargetPosition;

    private float nextNormalAttackTime;
    private float nextThrowAttackTime;
    private float nextHeavyAttackTime;
    private float nextChargeAttackTime;

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

        if (bossHealth == null)
        {
            bossHealth = GetComponent<BossHealth>();
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

        if (isAttacking)
        {
            if (!isCharging)
            {
                StopMoving();
            }

            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        if (TryStartAttack(distanceToPlayer))
        {
            return;
        }

        MoveTowardsPlayer();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        float currentSpeed = isCharging ? chargeSpeed : moveSpeed;

        Vector2 newPosition = rb.position + movementDirection * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private float GetDistanceToPlayer()
    {
        Vector2 origin = attackCenter != null ? attackCenter.position : transform.position;
        return Vector2.Distance(origin, player.position);
    }

    private bool TryStartAttack(float distanceToPlayer)
    {
        int phase = bossHealth != null ? bossHealth.CurrentPhase : 1;

        // Phase 4:
        // If the charge is ready, the boss uses Sneer and charges to the player's last position.
        if (phase >= 4 &&
            Time.time >= nextChargeAttackTime &&
            distanceToPlayer <= chargeStartMaxRange)
        {
            StartCoroutine(ChargeAttackRoutine());
            nextChargeAttackTime = Time.time + chargeCooldown;
            return true;
        }

        // Phase 2+:
        // Throw attack is preferred when the player is further away.
        if (phase >= 2 &&
            Time.time >= nextThrowAttackTime &&
            distanceToPlayer >= throwMinRange &&
            distanceToPlayer <= throwMaxRange)
        {
            StartBossAttack("ThrowAttack", throwAttackLockTime);
            nextThrowAttackTime = Time.time + throwAttackCooldown;
            return true;
        }

        // Phase 3+:
        // Heavy attack is used in close range with its own cooldown.
        if (phase >= 3 &&
            Time.time >= nextHeavyAttackTime &&
            distanceToPlayer <= heavyAttackRange)
        {
            StartBossAttack("HeavyAttack", heavyAttackLockTime);
            nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            return true;
        }

        // Phase 1+:
        // Normal close-range attack.
        if (Time.time >= nextNormalAttackTime &&
            distanceToPlayer <= normalAttackRange)
        {
            StartBossAttack("NormalAttack", normalAttackLockTime);
            nextNormalAttackTime = Time.time + normalAttackCooldown;
            return true;
        }

        return false;
    }

    private void StartBossAttack(string triggerName, float lockTime)
    {
        StopMoving();
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }

        StartCoroutine(AttackLockRoutine(lockTime));
    }

    private IEnumerator AttackLockRoutine(float lockTime)
    {
        yield return new WaitForSeconds(lockTime);
        isAttacking = false;
    }

    private IEnumerator ChargeAttackRoutine()
    {
        StopMoving();
        isAttacking = true;

        // Save the player's current position.
        // The boss will charge to this position, even if the player moves away.
        chargeTargetPosition = player.position;

        Vector2 directionToTarget = (chargeTargetPosition - (Vector2)transform.position).normalized;
        UpdateFacingDirection(directionToTarget);

        if (animator != null)
        {
            animator.SetTrigger("Sneer");
        }

        // Sneer / warning time before the charge starts.
        yield return new WaitForSeconds(chargeWindupTime);

        isCharging = true;

        float chargeTimer = 0f;

        while (chargeTimer < chargeMaxDuration)
        {
            Vector2 chargeOrigin = attackCenter != null ? attackCenter.position : transform.position;
            Vector2 direction = chargeTargetPosition - chargeOrigin;

            if (direction.magnitude <= chargeStopDistance)
            {
                break;
            }

            movementDirection = direction.normalized;
            UpdateFacingDirection(movementDirection);

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
        StopMoving();

        TryDamagePlayer(chargeDamageRadius, chargeDamage);

        yield return new WaitForSeconds(chargeEndLag);

        isAttacking = false;
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

    private void StopMoving()
    {
        movementDirection = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // If the boss faces the wrong direction, swap true and false here.
        if (direction.x < -0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x > 0.05f)
        {
            spriteRenderer.flipX = true;
        }
    }

    // Animation Event for normal attack hit frame
    public void DealNormalDamage()
    {
        TryDamagePlayer(normalAttackRange, normalDamage);
    }

    // Animation Event for throw attack hit frame
    // For now this is direct range damage.
    // Later we can replace this with a projectile using Ball_attack4.
    public void DealThrowDamage()
    {
        TryDamagePlayer(throwMaxRange, throwDamage);
    }

    // Animation Event for heavy attack hit frame
    public void DealHeavyDamage()
    {
        TryDamagePlayer(heavyAttackRange, heavyDamage);
    }

    private void TryDamagePlayer(float range, int damage)
    {
        if (player == null)
        {
            return;
        }

        Vector2 origin = attackCenter != null ? attackCenter.position : transform.position;
        float distanceToPlayer = Vector2.Distance(origin, player.position);

        if (distanceToPlayer > range)
        {
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(damage);
        }
        else
        {
            Debug.LogWarning("BossController: PlayerHealth script not found on player.");
        }
    }

    public void ActivateBoss()
    {
        isActive = true;
        isAttacking = false;
        isCharging = false;
    }

    public void DeactivateBoss()
    {
        isActive = false;
        isAttacking = false;
        isCharging = false;
        StopMoving();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = attackCenter != null ? attackCenter.position : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, normalAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, heavyAttackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, throwMaxRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origin, chargeDamageRadius);
    }
}