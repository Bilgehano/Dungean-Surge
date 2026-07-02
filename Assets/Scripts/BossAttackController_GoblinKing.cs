using System.Collections;
using UnityEngine;

public class BossAttackController_GoblinKing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Animator animator;

    [Header("Attack Centers")]
    [SerializeField] private Transform normalAttackCenter;
    [SerializeField] private Transform heavyAttackCenter;
    [SerializeField] private Transform throwAttackCenter;
    [SerializeField] private Transform chargeAttackCenter;

    [Header("Normal Attack")]
    [SerializeField] private int normalDamage = -3;
    [SerializeField] private float normalAttackWidth = 1.5f;
    [SerializeField] private float normalAttackHeight = 1.2f;
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float normalAttackLockTime = 0.6f;

    [Header("Heavy Attack")]
    [SerializeField] private int heavyDamage = -10;
    [SerializeField] private float heavyAttackWidth = 2f;
    [SerializeField] private float heavyAttackHeight = 2.2f;
    [SerializeField] private float heavyAttackCooldown = 7f;
    [SerializeField] private float heavyAttackLockTime = 1f;

    [Header("Throw Attack")]
    [SerializeField] private int throwDamage = -2;
    [SerializeField] private float throwMinRange = 2f;
    [SerializeField] private float throwMaxRange = 7f;
    [SerializeField] private float throwAttackCooldown = 4f;
    [SerializeField] private float throwAttackLockTime = 0.8f;

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject throwProjectilePrefab;
    [SerializeField] private float throwMinimumFlightDistance = 3.5f;
    [SerializeField] private float throwProjectileSpeed = 7f;
    [SerializeField] private float throwArcHeight = 1.2f;
    [SerializeField] private float throwImpactRadius = 0.8f;
    [SerializeField] private float throwAimLeadTime = 0.25f;
    [SerializeField] private bool throwInStraightLine = true;

    [Header("Charge Attack")]
    [SerializeField] private int chargeDamage = -8;
    [SerializeField] private float chargeDamageRadius = 1.4f;
    [SerializeField] private float chargeCooldown = 8f;

    [Min(2)]
    [SerializeField] private int minimumChargeCount = 2;

    [Range(0f, 1f)]
    [SerializeField] private float additionalChargeChance = 0.4f;

    [SerializeField] private float chargeComboPause = 0.2f;
    [SerializeField] private float chargeStartMaxRange = 8f;
    [SerializeField] private float chargeSpeed = 9f;
    [SerializeField] private float chargeMinDistance = 5f;
    [SerializeField] private float chargeMaxDistance = 10f;
    [SerializeField] private float chargeWindupTime = 0.8f;
    [SerializeField] private float chargeEndLag = 0.4f;

    private bool isAttacking;
    private bool isCharging;

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

        if (TryStartAttack())
        {
            return;
        }

        bossController.SetMovementEnabled(true);
    }

    private bool TryStartAttack()
    {
        int phase = bossHealth != null
            ? bossHealth.CurrentPhase
            : 1;

        Vector2 normalOrigin =
            bossController.GetAttackOrigin(normalAttackCenter);

        Vector2 heavyOrigin =
            bossController.GetAttackOrigin(heavyAttackCenter);

        Vector2 throwOrigin =
            bossController.GetAttackOrigin(throwAttackCenter);

        Vector2 chargeOrigin =
            bossController.GetAttackOrigin(chargeAttackCenter);

        float normalDistance =
            bossController.GetDistanceToPlayer(normalOrigin);

        float heavyDistance =
            bossController.GetDistanceToPlayer(heavyOrigin);

        float throwDistance =
            bossController.GetDistanceToPlayer(throwOrigin);

        float chargeDistance =
            bossController.GetDistanceToPlayer(chargeOrigin);

        if (phase >= 4 &&
            Time.time >= nextChargeAttackTime &&
            chargeDistance <= chargeStartMaxRange)
        {
            StartCoroutine(ChargeAttackRoutine());
            return true;
        }

        if (phase >= 2 &&
            Time.time >= nextThrowAttackTime &&
            throwDistance >= throwMinRange &&
            throwDistance <= throwMaxRange)
        {
            StartBossAttack(
                "ThrowAttack",
                throwAttackLockTime
            );

            nextThrowAttackTime =
                Time.time + throwAttackCooldown;

            return true;
        }

        if (phase >= 3 &&
            Time.time >= nextHeavyAttackTime &&
            heavyDistance <= heavyAttackWidth)
        {
            StartBossAttack(
                "HeavyAttack",
                heavyAttackLockTime
            );

            nextHeavyAttackTime =
                Time.time + heavyAttackCooldown;

            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            normalDistance <= normalAttackWidth)
        {
            StartBossAttack(
                "NormalAttack",
                normalAttackLockTime
            );

            nextNormalAttackTime =
                Time.time + normalAttackCooldown;

            return true;
        }

        return false;
    }

    private void StartBossAttack(
        string triggerName,
        float lockTime)
    {
        isAttacking = true;

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();

        if (bossController.HasPlayer)
        {
            bossController.FacePosition(
                bossController.Player.position
            );
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

        if (bossController != null &&
            bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
    }

    private IEnumerator ChargeAttackRoutine()
    {
        isAttacking = true;

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();

        int completedChargeCount = 0;

        int guaranteedChargeCount = Mathf.Max(
            2,
            minimumChargeCount
        );

        bool continueCharging = true;

        while (continueCharging &&
               bossController != null &&
               bossController.IsActive &&
               bossController.HasPlayer)
        {
            yield return StartCoroutine(
                PerformSingleChargeRoutine()
            );

            completedChargeCount++;

            if (completedChargeCount < guaranteedChargeCount)
            {
                yield return new WaitForSeconds(
                    chargeComboPause
                );

                continue;
            }

            continueCharging =
                Random.value < additionalChargeChance;

            if (continueCharging)
            {
                yield return new WaitForSeconds(
                    chargeComboPause
                );
            }
        }

        isCharging = false;
        bossController.StopMoving();

        yield return new WaitForSeconds(chargeEndLag);

        nextChargeAttackTime =
            Time.time + chargeCooldown;

        isAttacking = false;

        if (bossController != null &&
            bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
    }

    private IEnumerator PerformSingleChargeRoutine()
    {
        if (bossController == null ||
            !bossController.HasPlayer)
        {
            yield break;
        }

        Vector2 chargeOrigin =
            bossController.GetAttackOrigin(chargeAttackCenter);

        Vector2 chargeTargetPosition =
            bossController.Player.position;

        Vector2 chargeDirection =
            chargeTargetPosition - chargeOrigin;

        if (chargeDirection.sqrMagnitude <= 0.0001f)
        {
            chargeDirection =
                chargeTargetPosition -
                (Vector2)transform.position;
        }

        if (chargeDirection.sqrMagnitude <= 0.0001f)
        {
            yield break;
        }

        chargeDirection.Normalize();

        float minimumDistance = Mathf.Max(
            0.1f,
            Mathf.Min(
                chargeMinDistance,
                chargeMaxDistance
            )
        );

        float maximumDistance = Mathf.Max(
            minimumDistance,
            Mathf.Max(
                chargeMinDistance,
                chargeMaxDistance
            )
        );

        float selectedChargeDistance = Random.Range(
            minimumDistance,
            maximumDistance
        );

        Vector2 chargeStartPosition = transform.position;

        float safetyDuration =
            selectedChargeDistance /
            Mathf.Max(chargeSpeed, 0.01f) +
            0.5f;

        float safetyTimer = 0f;
        bool chargeAlreadyHitPlayer = false;

        bossController.FacePosition(chargeTargetPosition);

        if (animator != null)
        {
            animator.SetTrigger("Sneer");
        }

        yield return new WaitForSeconds(chargeWindupTime);

        isCharging = true;

        while (Vector2.Distance(
                   chargeStartPosition,
                   transform.position
               ) < selectedChargeDistance &&
               safetyTimer < safetyDuration &&
               bossController != null &&
               bossController.IsActive &&
               bossController.HasPlayer)
        {
            Vector2 currentChargeOrigin =
                bossController.GetAttackOrigin(
                    chargeAttackCenter
                );

            bossController.SetMovement(
                chargeDirection,
                chargeSpeed,
                true
            );

            if (!chargeAlreadyHitPlayer)
            {
                bool didHit = bossController.TryDamagePlayer(
                    currentChargeOrigin,
                    chargeDamageRadius,
                    chargeDamage
                );

                if (didHit)
                {
                    chargeAlreadyHitPlayer = true;
                }
            }

            safetyTimer += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
        bossController.StopMoving();
    }

    public void DealNormalDamage()
    {
        if (bossController == null)
        {
            return;
        }

        Vector2 normalOrigin =
            bossController.GetAttackOrigin(
                normalAttackCenter
            );

        bossController.TryDamagePlayerInFront(
            normalOrigin,
            normalAttackWidth,
            normalAttackHeight,
            normalDamage
        );
    }

    public void DealThrowDamage()
    {
        ThrowProjectile();
    }

    public void DealHeavyDamage()
    {
        if (bossController == null)
        {
            return;
        }

        Vector2 heavyOrigin =
            bossController.GetAttackOrigin(
                heavyAttackCenter
            );

        bossController.TryDamagePlayerInFront(
            heavyOrigin,
            heavyAttackWidth,
            heavyAttackHeight,
            heavyDamage
        );
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

        Vector2 fallbackThrowOrigin =
            bossController.GetAttackOrigin(throwAttackCenter);

        Vector2 spawnPosition = projectileSpawnPoint != null
            ? bossController.GetAttackOrigin(
                projectileSpawnPoint
            )
            : fallbackThrowOrigin;

        Vector2 targetPosition =
            bossController.Player.position;

        Rigidbody2D playerRb =
            bossController.Player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            targetPosition +=
                playerRb.linearVelocity * throwAimLeadTime;
        }

        targetPosition = GetMinimumFlightTarget(
            spawnPosition,
            targetPosition
        );

        float travelTime = CalculateThrowTravelTime(
            spawnPosition,
            targetPosition
        );

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
                travelTime,
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

    private Vector2 GetMinimumFlightTarget(
        Vector2 spawnPosition,
        Vector2 intendedTargetPosition)
    {
        float minimumDistance = Mathf.Max(
            0f,
            throwMinimumFlightDistance
        );

        Vector2 direction =
            intendedTargetPosition - spawnPosition;

        float currentDistance = direction.magnitude;

        if (minimumDistance <= 0f ||
            currentDistance >= minimumDistance)
        {
            return intendedTargetPosition;
        }

        if (currentDistance <= 0.0001f)
        {
            direction = GetFallbackThrowDirection();
        }
        else
        {
            direction /= currentDistance;
        }

        return spawnPosition +
            direction * minimumDistance;
    }

    private float CalculateThrowTravelTime(
        Vector2 startPosition,
        Vector2 endPosition)
    {
        float distance = Vector2.Distance(
            startPosition,
            endPosition
        );

        float speed = Mathf.Max(
            0.1f,
            throwProjectileSpeed
        );

        return Mathf.Max(
            0.1f,
            distance / speed
        );
    }

    private Vector2 GetFallbackThrowDirection()
    {
        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        bool facesRight = bossSpriteRenderer != null &&
                          bossSpriteRenderer.flipX;

        return facesRight
            ? Vector2.right
            : Vector2.left;
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

        Vector3 normalOrigin =
            controller.GetAttackOrigin(normalAttackCenter);

        Vector3 heavyOrigin =
            controller.GetAttackOrigin(heavyAttackCenter);

        Vector3 throwOrigin =
            controller.GetAttackOrigin(throwAttackCenter);

        Vector3 chargeOrigin =
            controller.GetAttackOrigin(chargeAttackCenter);

        Vector3 normalAttackBoxCenter =
            normalOrigin +
            Vector3.right * direction *
            (normalAttackWidth * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            normalAttackBoxCenter,
            new Vector3(
                normalAttackWidth,
                normalAttackHeight,
                0.1f
            )
        );

        Vector3 heavyAttackBoxCenter =
            heavyOrigin +
            Vector3.right * direction *
            (heavyAttackWidth * 0.5f);

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
        Gizmos.DrawWireSphere(
            throwOrigin,
            throwMaxRange
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            chargeOrigin,
            chargeDamageRadius
        );
    }
}