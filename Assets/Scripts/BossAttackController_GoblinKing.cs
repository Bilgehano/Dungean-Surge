using System.Collections;
using UnityEngine;

public class BossAttackController_GoblinKing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject throwProjectilePrefab;

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

    [Header("Throw Projectile")]
    [SerializeField] private float throwTravelTime = 0.8f;
    [SerializeField] private float throwArcHeight = 1.2f;
    [SerializeField] private float throwImpactRadius = 0.8f;

    [Header("Damage")]
    [SerializeField] private int normalDamage = -3;
    [SerializeField] private int throwDamage = -2;
    [SerializeField] private int heavyDamage = -10;
    [SerializeField] private int chargeDamage = -8;

    private bool isAttacking;
    private bool isCharging;

    private Vector2 chargeTargetPosition;

    private float nextNormalAttackTime;
    private float nextThrowAttackTime;
    private float nextHeavyAttackTime;
    private float nextChargeAttackTime;

    private void Awake()
    {
        if (bossController == null)
        {
            bossController = GetComponent<BossController>();
        }

        if (bossHealth == null)
        {
            bossHealth = GetComponent<BossHealth>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (bossController == null || !bossController.IsActive || !bossController.HasPlayer)
        {
            return;
        }

        if (isAttacking)
        {
            return;
        }

        float distanceToPlayer = bossController.GetDistanceToPlayer();

        if (TryStartAttack(distanceToPlayer))
        {
            return;
        }

        bossController.SetMovementEnabled(true);
    }

    private bool TryStartAttack(float distanceToPlayer)
    {
        int phase = bossHealth != null ? bossHealth.CurrentPhase : 1;

        // Phase 4:
        // Charge has highest priority when it is ready.
        // Boss sneers, saves the player's current position, then charges there.
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
        // Heavy attack in close range.
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
        isAttacking = true;

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();

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

        if (bossController != null && bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
    }

    private IEnumerator ChargeAttackRoutine()
    {
        isAttacking = true;

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();

        // Save the player's current position.
        // The boss will charge to this position, even if the player moves away.
        chargeTargetPosition = bossController.Player.position;

        bossController.FacePosition(chargeTargetPosition);

        if (animator != null)
        {
            animator.SetTrigger("Sneer");
        }

        // Sneer / warning time before the charge starts.
        yield return new WaitForSeconds(chargeWindupTime);

        isCharging = true;

        float chargeTimer = 0f;
        bool chargeAlreadyHitPlayer = false;

        while (chargeTimer < chargeMaxDuration)
        {
            Vector2 direction = chargeTargetPosition - bossController.AttackOrigin;

            if (direction.magnitude <= chargeStopDistance)
            {
                break;
            }

            bossController.SetMovement(direction.normalized, chargeSpeed, true);

            // During the charge, damage the player once if he is inside the charge radius.
            if (!chargeAlreadyHitPlayer)
            {
                bool didHit = bossController.TryDamagePlayer(chargeDamageRadius, chargeDamage);

                if (didHit)
                {
                    chargeAlreadyHitPlayer = true;
                }
            }

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        isCharging = false;

        bossController.StopMoving();

        yield return new WaitForSeconds(chargeEndLag);

        isAttacking = false;

        if (bossController != null && bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
    }

    // Animation Event: normal attack hit frame
    public void DealNormalDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayer(normalAttackRange, normalDamage);
        }
    }

    // Animation Event: throw frame
    public void DealThrowDamage()
    {
        ThrowProjectile();
    }

    // Animation Event: heavy attack hit frame
    public void DealHeavyDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayer(heavyAttackRange, heavyDamage);
        }
    }

    private void ThrowProjectile()
    {
        if (throwProjectilePrefab == null || bossController == null || !bossController.HasPlayer)
        {
            Debug.LogWarning("BossAttackController_GoblinKing: Throw projectile prefab or player is missing.");
            return;
        }

        Vector2 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        // Important:
        // This saves the player's current position only once.
        // The projectile will fly to this old position and will not follow the player.
        Vector2 targetPosition = bossController.Player.position;

        bossController.FacePosition(targetPosition);

        GameObject projectileObject = Instantiate(
            throwProjectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        BossProjectile projectile = projectileObject.GetComponent<BossProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                spawnPosition,
                targetPosition,
                throwTravelTime,
                throwArcHeight,
                throwImpactRadius,
                throwDamage,
                bossController.Player
            );
        }
        else
        {
            Debug.LogWarning("BossAttackController_GoblinKing: Projectile prefab has no BossProjectile script.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        BossController controller = bossController != null ? bossController : GetComponent<BossController>();

        if (controller == null)
        {
            return;
        }

        Vector3 origin = controller.AttackOrigin;

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