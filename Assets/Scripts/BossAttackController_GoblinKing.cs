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

    [Header("Attack Boxes")]
    [SerializeField] private float normalAttackWidth = 1.5f;
    [SerializeField] private float normalAttackHeight = 1.2f;

    [SerializeField] private float heavyAttackWidth = 2f;
    [SerializeField] private float heavyAttackHeight = 2.2f;

    [Header("Throw Attack")]
    [SerializeField] private float throwMinRange = 2f;
    [SerializeField] private float throwMaxRange = 7f;

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
    [SerializeField] private float heavyAttackLockTime = 1f;

    [Header("Throw Projectile")]
    [SerializeField] private float throwTravelTime = 0.8f;
    [SerializeField] private float throwArcHeight = 1.2f;
    [SerializeField] private float throwImpactRadius = 0.8f;
    [SerializeField] private float throwAimLeadTime = 0.25f;
    [SerializeField] private bool throwInStraightLine = true;

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
        if (bossController == null ||
            !bossController.IsActive ||
            !bossController.HasPlayer)
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
        int phase = bossHealth != null
            ? bossHealth.CurrentPhase
            : 1;

        if (phase >= 4 &&
            Time.time >= nextChargeAttackTime &&
            distanceToPlayer <= chargeStartMaxRange)
        {
            StartCoroutine(ChargeAttackRoutine());
            nextChargeAttackTime = Time.time + chargeCooldown;
            return true;
        }

        if (phase >= 2 &&
            Time.time >= nextThrowAttackTime &&
            distanceToPlayer >= throwMinRange &&
            distanceToPlayer <= throwMaxRange)
        {
            StartBossAttack("ThrowAttack", throwAttackLockTime);
            nextThrowAttackTime = Time.time + throwAttackCooldown;
            return true;
        }

        if (phase >= 3 &&
            Time.time >= nextHeavyAttackTime &&
            distanceToPlayer <= heavyAttackWidth)
        {
            StartBossAttack("HeavyAttack", heavyAttackLockTime);
            nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            distanceToPlayer <= normalAttackWidth)
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

        if (bossController.HasPlayer)
        {
            bossController.FacePosition(bossController.Player.position);
        }

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

        chargeTargetPosition = bossController.Player.position;

        bossController.FacePosition(chargeTargetPosition);

        if (animator != null)
        {
            animator.SetTrigger("Sneer");
        }

        yield return new WaitForSeconds(chargeWindupTime);

        isCharging = true;

        float chargeTimer = 0f;
        bool chargeAlreadyHitPlayer = false;

        while (chargeTimer < chargeMaxDuration)
        {
            Vector2 direction =
                chargeTargetPosition - bossController.AttackOrigin;

            if (direction.magnitude <= chargeStopDistance)
            {
                break;
            }

            bossController.SetMovement(
                direction.normalized,
                chargeSpeed,
                true
            );

            if (!chargeAlreadyHitPlayer)
            {
                bool didHit = bossController.TryDamagePlayer(
                    chargeDamageRadius,
                    chargeDamage
                );

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

    public void DealNormalDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayerInFront(
                normalAttackWidth,
                normalAttackHeight,
                normalDamage
            );
        }
    }

    public void DealThrowDamage()
    {
        ThrowProjectile();
    }

    public void DealHeavyDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayerInFront(
                heavyAttackWidth,
                heavyAttackHeight,
                heavyDamage
            );
        }
    }

    private void ThrowProjectile()
    {
        if (throwProjectilePrefab == null ||
            bossController == null ||
            !bossController.HasPlayer)
        {
            Debug.LogWarning(
                "BossAttackController_GoblinKing: " +
                "Throw projectile prefab or player is missing."
            );

            return;
        }

        Vector2 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        Vector2 targetPosition = bossController.Player.position;

        Rigidbody2D playerRb =
            bossController.Player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            targetPosition +=
                playerRb.linearVelocity * throwAimLeadTime;
        }

        bossController.FacePosition(targetPosition);

        GameObject projectileObject = Instantiate(
            throwProjectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        BossProjectile projectile =
            projectileObject.GetComponent<BossProjectile>();

        if (projectile != null)
        {
            float arcHeight = throwInStraightLine
                ? 0f
                : throwArcHeight;

            projectile.Initialize(
                spawnPosition,
                targetPosition,
                throwTravelTime,
                arcHeight,
                throwImpactRadius,
                throwDamage,
                bossController.Player
            );
        }
        else
        {
            Debug.LogWarning(
                "BossAttackController_GoblinKing: " +
                "Projectile prefab has no BossProjectile script."
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        BossController controller = bossController != null
            ? bossController
            : GetComponent<BossController>();

        if (controller == null)
        {
            return;
        }

        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        bool facesRight = bossSpriteRenderer != null &&
                          bossSpriteRenderer.flipX;

        float direction = facesRight ? 1f : -1f;

        Vector3 origin = controller.AttackOrigin;

        Vector3 normalAttackBoxCenter = origin +
            Vector3.right * direction * (normalAttackWidth * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            normalAttackBoxCenter,
            new Vector3(
                normalAttackWidth,
                normalAttackHeight,
                0.1f
            )
        );

        Vector3 heavyAttackBoxCenter = origin +
            Vector3.right * direction * (heavyAttackWidth * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            heavyAttackBoxCenter,
            new Vector3(
                heavyAttackWidth,
                heavyAttackHeight,
                0.1f
            )
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, throwMaxRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origin, chargeDamageRadius);
    }
}