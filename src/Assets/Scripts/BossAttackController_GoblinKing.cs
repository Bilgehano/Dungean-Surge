using System.Collections;
using UnityEngine;

public class BossAttackController_GoblinKing : MonoBehaviour
{
    private const string IsChargingParameter = "IsCharging";
    private const string SneerTriggerName = "Sneer";

    [Header("Animation State Names")]
    [SerializeField] private string chargeAnimationStateName = "Boss_GoblinKing_attack_2";

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

    [Header("Enhanced Normal Attack")]
    [Min(1)]
    [SerializeField] private int enhancedNormalPhase = 3;
    [SerializeField] private float enhancedNormalAttackWidth = 1.8f;
    [SerializeField] private float enhancedNormalAttackHeight = 1.4f;
    [SerializeField] private float enhancedNormalAttackCooldown = 1.3f;

    [Range(0f, 1f)]
    [SerializeField] private float enhancedNormalCritChance = 0.25f;

    [Min(1f)]
    [SerializeField] private float enhancedNormalCritMultiplier = 2f;

    [Header("Heavy Attack")]
    [SerializeField] private int heavyDamage = -10;
    [SerializeField] private float heavyAttackWidth = 2f;
    [SerializeField] private float heavyAttackHeight = 2.2f;
    [SerializeField] private float heavyAttackCooldown = 7f;
    [SerializeField] private float heavyAttackLockTime = 1f;

    [Header("Enhanced Heavy Attack")]
    [Min(1)]
    [SerializeField] private int enhancedHeavyPhase = 4;
    [SerializeField] private float enhancedHeavyAttackWidth = 2.5f;
    [SerializeField] private float enhancedHeavyAttackHeight = 2.6f;
    [SerializeField] private float enhancedHeavyAttackCooldown = 6f;

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

    [Header("Enhanced Throw Attack")]
    [Min(1)]
    [SerializeField] private int enhancedThrowPhase = 3;
    [SerializeField] private float enhancedThrowAttackCooldown = 4f;

    [Min(1)]
    [SerializeField] private int enhancedThrowProjectileCount = 3;

    [SerializeField] private float enhancedThrowSideAngle = 15f;

    [Header("Charge Attack")]
    [SerializeField] private int chargeDamage = -8;
    [SerializeField] private float chargeDamageRadius = 1.4f;

    [Min(1)]
    [SerializeField] private int chargePhase = 2;

    [SerializeField] private float chargeCooldown = 8f;
    [SerializeField] private float chargeComboPause = 0.2f;
    [SerializeField] private float chargeStartMaxRange = 8f;
    [SerializeField] private float chargeSpeed = 9f;
    [SerializeField] private float chargeMinDistance = 5f;
    [SerializeField] private float chargeMaxDistance = 10f;
    [SerializeField] private float chargeWindupTime = 0.8f;
    [SerializeField] private float chargeEndLag = 0.4f;

    [Header("Enhanced Charge Attack")]
    [Min(1)]
    [SerializeField] private int enhancedChargePhase = 4;
    [SerializeField] private float enhancedChargeCooldown = 8f;
    [SerializeField] private float enhancedChargeSpeed = 11f;

    [Min(2)]
    [SerializeField] private int enhancedMinimumChargeCount = 2;

    [Range(0f, 1f)]
    [SerializeField] private float enhancedAdditionalChargeChance = 0.4f;

    [Header("Charge Obstacle Detection")]
    [SerializeField] private BoxCollider2D chargeBodyCollider;
    [SerializeField] private LayerMask chargeObstacleLayers;
    [SerializeField] private float chargeObstaclePadding = 0.5f;

    [Range(0.1f, 2f)]
    [SerializeField] private float chargeBoxCastScale = 0.9f;

    [Header("Gizmo Display")]
    [SerializeField] private bool showNormalMovesetGizmos = true;
    [SerializeField] private bool showEnhancedMovesetGizmos = true;

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

        if (chargeBodyCollider == null)
        {
            chargeBodyCollider = GetComponent<BoxCollider2D>();
        }
    }

    private void OnValidate()
    {
        enhancedNormalPhase = Mathf.Max(1, enhancedNormalPhase);
        enhancedHeavyPhase = Mathf.Max(1, enhancedHeavyPhase);
        enhancedThrowPhase = Mathf.Max(1, enhancedThrowPhase);
        chargePhase = Mathf.Max(1, chargePhase);
        enhancedChargePhase = Mathf.Max(1, enhancedChargePhase);

        normalAttackWidth = Mathf.Max(0.1f, normalAttackWidth);
        normalAttackHeight = Mathf.Max(0.1f, normalAttackHeight);
        enhancedNormalAttackWidth = Mathf.Max(0.1f, enhancedNormalAttackWidth);
        enhancedNormalAttackHeight = Mathf.Max(0.1f, enhancedNormalAttackHeight);

        heavyAttackWidth = Mathf.Max(0.1f, heavyAttackWidth);
        heavyAttackHeight = Mathf.Max(0.1f, heavyAttackHeight);
        enhancedHeavyAttackWidth = Mathf.Max(0.1f, enhancedHeavyAttackWidth);
        enhancedHeavyAttackHeight = Mathf.Max(0.1f, enhancedHeavyAttackHeight);

        normalAttackCooldown = Mathf.Max(0f, normalAttackCooldown);
        enhancedNormalAttackCooldown = Mathf.Max(0f, enhancedNormalAttackCooldown);
        heavyAttackCooldown = Mathf.Max(0f, heavyAttackCooldown);
        enhancedHeavyAttackCooldown = Mathf.Max(0f, enhancedHeavyAttackCooldown);
        throwAttackCooldown = Mathf.Max(0f, throwAttackCooldown);
        enhancedThrowAttackCooldown = Mathf.Max(0f, enhancedThrowAttackCooldown);
        chargeCooldown = Mathf.Max(0f, chargeCooldown);
        enhancedChargeCooldown = Mathf.Max(0f, enhancedChargeCooldown);

        enhancedNormalCritChance = Mathf.Clamp01(enhancedNormalCritChance);
        enhancedNormalCritMultiplier = Mathf.Max(1f, enhancedNormalCritMultiplier);

        enhancedThrowProjectileCount = Mathf.Max(1, enhancedThrowProjectileCount);
        enhancedMinimumChargeCount = Mathf.Max(2, enhancedMinimumChargeCount);
        enhancedAdditionalChargeChance = Mathf.Clamp01(enhancedAdditionalChargeChance);

        chargeObstaclePadding = Mathf.Max(0f, chargeObstaclePadding);
        chargeBoxCastScale = Mathf.Clamp(chargeBoxCastScale, 0.1f, 2f);
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
        int phase = GetCurrentPhase();

        Vector2 normalOrigin = bossController.GetAttackOrigin(normalAttackCenter);
        Vector2 heavyOrigin = bossController.GetAttackOrigin(heavyAttackCenter);
        Vector2 throwOrigin = bossController.GetAttackOrigin(throwAttackCenter);
        Vector2 chargeOrigin = bossController.GetAttackOrigin(chargeAttackCenter);

        float normalDistance = bossController.GetDistanceToPlayer(normalOrigin);
        float heavyDistance = bossController.GetDistanceToPlayer(heavyOrigin);
        float throwDistance = bossController.GetDistanceToPlayer(throwOrigin);
        float chargeDistance = bossController.GetDistanceToPlayer(chargeOrigin);

        if (phase >= chargePhase &&
            Time.time >= nextChargeAttackTime &&
            chargeDistance <= chargeStartMaxRange)
        {
            StartCoroutine(ChargeAttackRoutine());
            return true;
        }

        if (phase >= 1 &&
            Time.time >= nextThrowAttackTime &&
            throwDistance >= throwMinRange &&
            throwDistance <= throwMaxRange)
        {
            StartBossAttack("ThrowAttack", throwAttackLockTime);
            nextThrowAttackTime = Time.time + GetThrowAttackCooldown();
            return true;
        }

        if (phase >= 2 &&
            Time.time >= nextHeavyAttackTime &&
            heavyDistance <= GetHeavyAttackWidth())
        {
            StartBossAttack("HeavyAttack", heavyAttackLockTime);
            nextHeavyAttackTime = Time.time + GetHeavyAttackCooldown();
            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            normalDistance <= GetNormalAttackWidth())
        {
            StartBossAttack("NormalAttack", normalAttackLockTime);
            nextNormalAttackTime = Time.time + GetNormalAttackCooldown();
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

        bool useEnhancedCharge = IsEnhancedChargeActive();

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();
        SetChargeAnimationState(false);

        int completedChargeCount = 0;

        int guaranteedChargeCount = useEnhancedCharge
            ? Mathf.Max(2, enhancedMinimumChargeCount)
            : 2;

        float extraChargeChance = useEnhancedCharge
            ? enhancedAdditionalChargeChance
            : 0f;

        bool continueCharging = true;

        while (continueCharging &&
               bossController != null &&
               bossController.IsActive &&
               bossController.HasPlayer)
        {
            yield return StartCoroutine(PerformSingleChargeRoutine(useEnhancedCharge));

            completedChargeCount++;

            if (completedChargeCount < guaranteedChargeCount)
            {
                yield return new WaitForSeconds(chargeComboPause);
                continue;
            }

            continueCharging = Random.value < extraChargeChance;

            if (continueCharging)
            {
                yield return new WaitForSeconds(chargeComboPause);
            }
        }

        isCharging = false;
        bossController.StopMoving();
        SetChargeAnimationState(false);

        yield return new WaitForSeconds(chargeEndLag);

        nextChargeAttackTime = Time.time + GetChargeCooldown(useEnhancedCharge);
        isAttacking = false;

        if (bossController != null && bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
    }

    private IEnumerator PerformSingleChargeRoutine(bool useEnhancedCharge)
    {
        if (bossController == null || !bossController.HasPlayer)
        {
            isCharging = false;
            SetChargeAnimationState(false);
            yield break;
        }

        Vector2 chargeOrigin = bossController.GetAttackOrigin(chargeAttackCenter);
        Vector2 chargeTargetPosition = bossController.Player.position;

        Vector2 chargeDirection = chargeTargetPosition - chargeOrigin;

        if (chargeDirection.sqrMagnitude <= 0.0001f)
        {
            chargeDirection = chargeTargetPosition - (Vector2)transform.position;
        }

        if (chargeDirection.sqrMagnitude <= 0.0001f)
        {
            isCharging = false;
            SetChargeAnimationState(false);
            yield break;
        }

        chargeDirection.Normalize();

        float minimumDistance = Mathf.Max(
            0.1f,
            Mathf.Min(chargeMinDistance, chargeMaxDistance)
        );

        float maximumDistance = Mathf.Max(
            minimumDistance,
            Mathf.Max(chargeMinDistance, chargeMaxDistance)
        );

        float selectedChargeDistance = Random.Range(minimumDistance, maximumDistance);
        Vector2 chargeStartPosition = transform.position;

        float effectiveChargeSpeed = GetChargeSpeed(useEnhancedCharge);

        float safetyDuration =
            selectedChargeDistance / Mathf.Max(effectiveChargeSpeed, 0.01f) + 0.5f;

        float safetyTimer = 0f;
        bool chargeAlreadyHitPlayer = false;

        bossController.FacePosition(chargeTargetPosition);

        SetChargeAnimationState(false);

        if (animator != null)
        {
            animator.ResetTrigger(SneerTriggerName);
            animator.SetTrigger(SneerTriggerName);
        }

        yield return new WaitForSeconds(chargeWindupTime);

        isCharging = true;
        PlayChargeAnimation();

        while (Vector2.Distance(chargeStartPosition, transform.position) < selectedChargeDistance &&
               safetyTimer < safetyDuration &&
               bossController != null &&
               bossController.IsActive &&
               bossController.HasPlayer)
        {
            float expectedStepDistance = Mathf.Max(
                0.01f,
                effectiveChargeSpeed * Time.fixedDeltaTime
            );

            if (IsChargeBlockedByObstacle(chargeDirection, expectedStepDistance))
            {
                break;
            }

            Vector2 currentChargeOrigin = bossController.GetAttackOrigin(chargeAttackCenter);

            bossController.SetMovement(
                chargeDirection,
                effectiveChargeSpeed,
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
        SetChargeAnimationState(false);
    }

    private void PlayChargeAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(SneerTriggerName);
        animator.SetBool(IsChargingParameter, true);
        animator.Play(chargeAnimationStateName, 0, 0f);
    }

    private void SetChargeAnimationState(bool value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsChargingParameter, value);

        if (!value)
        {
            animator.ResetTrigger(SneerTriggerName);
        }
    }

    private bool IsChargeBlockedByObstacle(Vector2 chargeDirection, float expectedStepDistance)
    {
        if (chargeBodyCollider == null ||
            chargeObstacleLayers.value == 0 ||
            chargeDirection.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        Vector2 castOrigin = chargeBodyCollider.bounds.center;

        Vector2 castSize = new Vector2(
            chargeBodyCollider.bounds.size.x,
            chargeBodyCollider.bounds.size.y
        ) * chargeBoxCastScale;

        float castDistance = Mathf.Max(
            0f,
            expectedStepDistance + chargeObstaclePadding
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            castOrigin,
            castSize,
            transform.eulerAngles.z,
            chargeDirection.normalized,
            castDistance,
            chargeObstacleLayers
        );

        return hit.collider != null;
    }

    public void DealNormalDamage()
    {
        if (bossController == null)
        {
            return;
        }

        Vector2 normalOrigin = bossController.GetAttackOrigin(normalAttackCenter);
        int finalDamage = GetNormalDamageForCurrentHit();

        bossController.TryDamagePlayerInFront(
            normalOrigin,
            GetNormalAttackWidth(),
            GetNormalAttackHeight(),
            finalDamage
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

        Vector2 heavyOrigin = bossController.GetAttackOrigin(heavyAttackCenter);

        bossController.TryDamagePlayerInFront(
            heavyOrigin,
            GetHeavyAttackWidth(),
            GetHeavyAttackHeight(),
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
                "BossAttackController_GoblinKing: Throw projectile prefab or player is missing."
            );

            return;
        }

        Vector2 fallbackThrowOrigin = bossController.GetAttackOrigin(throwAttackCenter);

        Vector2 spawnPosition = projectileSpawnPoint != null
            ? bossController.GetAttackOrigin(projectileSpawnPoint)
            : fallbackThrowOrigin;

        Vector2 targetPosition = bossController.Player.position;

        Rigidbody2D playerRb = bossController.Player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            targetPosition += playerRb.linearVelocity * throwAimLeadTime;
        }

        bossController.FacePosition(targetPosition);

        int projectileCount = IsEnhancedThrowActive()
            ? Mathf.Max(1, enhancedThrowProjectileCount)
            : 1;

        if (projectileCount <= 1)
        {
            Vector2 finalTarget = GetMinimumFlightTarget(
                spawnPosition,
                targetPosition
            );

            SpawnThrowProjectile(spawnPosition, finalTarget);
            return;
        }

        Vector2 baseDirection = targetPosition - spawnPosition;
        float targetDistance = baseDirection.magnitude;

        if (targetDistance <= 0.0001f)
        {
            baseDirection = GetFallbackThrowDirection();
            targetDistance = throwMinimumFlightDistance;
        }
        else
        {
            baseDirection /= targetDistance;
        }

        targetDistance = Mathf.Max(targetDistance, throwMinimumFlightDistance);

        float centerIndex = (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = (i - centerIndex) * enhancedThrowSideAngle;

            Vector2 projectileDirection = RotateVector(
                baseDirection,
                angleOffset
            ).normalized;

            Vector2 intendedTarget =
                spawnPosition + projectileDirection * targetDistance;

            Vector2 finalTarget = GetMinimumFlightTarget(
                spawnPosition,
                intendedTarget
            );

            SpawnThrowProjectile(spawnPosition, finalTarget);
        }
    }

    private void SpawnThrowProjectile(Vector2 spawnPosition, Vector2 targetPosition)
    {
        float travelTime = CalculateThrowTravelTime(spawnPosition, targetPosition);

        GameObject projectileObject = Instantiate(
            throwProjectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        BossProjectile projectile = projectileObject.GetComponent<BossProjectile>();

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
                "BossAttackController_GoblinKing: Projectile prefab has no BossProjectile script."
            );
        }
    }

    private Vector2 GetMinimumFlightTarget(Vector2 spawnPosition, Vector2 intendedTargetPosition)
    {
        float minimumDistance = Mathf.Max(0f, throwMinimumFlightDistance);

        Vector2 direction = intendedTargetPosition - spawnPosition;
        float currentDistance = direction.magnitude;

        if (minimumDistance <= 0f || currentDistance >= minimumDistance)
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

        return spawnPosition + direction * minimumDistance;
    }

    private float CalculateThrowTravelTime(Vector2 startPosition, Vector2 endPosition)
    {
        float distance = Vector2.Distance(startPosition, endPosition);
        float speed = Mathf.Max(0.1f, throwProjectileSpeed);

        return Mathf.Max(0.1f, distance / speed);
    }

    private Vector2 GetFallbackThrowDirection()
    {
        SpriteRenderer bossSpriteRenderer = GetComponent<SpriteRenderer>();

        bool facesRight = bossSpriteRenderer != null &&
                          bossSpriteRenderer.flipX;

        return facesRight
            ? Vector2.right
            : Vector2.left;
    }

    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private int GetCurrentPhase()
    {
        return bossHealth != null
            ? bossHealth.CurrentPhase
            : 1;
    }

    private bool IsEnhancedNormalActive()
    {
        return IsEnhancedNormalActiveForPhase(GetCurrentPhase());
    }

    private bool IsEnhancedHeavyActive()
    {
        return IsEnhancedHeavyActiveForPhase(GetCurrentPhase());
    }

    private bool IsEnhancedThrowActive()
    {
        return IsEnhancedThrowActiveForPhase(GetCurrentPhase());
    }

    private bool IsEnhancedChargeActive()
    {
        return IsEnhancedChargeActiveForPhase(GetCurrentPhase());
    }

    private bool IsEnhancedNormalActiveForPhase(int phase)
    {
        return phase >= enhancedNormalPhase;
    }

    private bool IsEnhancedHeavyActiveForPhase(int phase)
    {
        return phase >= enhancedHeavyPhase;
    }

    private bool IsEnhancedThrowActiveForPhase(int phase)
    {
        return phase >= enhancedThrowPhase;
    }

    private bool IsEnhancedChargeActiveForPhase(int phase)
    {
        return phase >= enhancedChargePhase;
    }

    private int GetNormalDamageForCurrentHit()
    {
        if (!IsEnhancedNormalActive())
        {
            return normalDamage;
        }

        bool isCriticalHit = Random.value < enhancedNormalCritChance;

        if (!isCriticalHit)
        {
            return normalDamage;
        }

        float scaledDamage = normalDamage * enhancedNormalCritMultiplier;

        return Mathf.RoundToInt(scaledDamage);
    }

    private float GetNormalAttackWidth()
    {
        return IsEnhancedNormalActive()
            ? enhancedNormalAttackWidth
            : normalAttackWidth;
    }

    private float GetNormalAttackHeight()
    {
        return IsEnhancedNormalActive()
            ? enhancedNormalAttackHeight
            : normalAttackHeight;
    }

    private float GetNormalAttackCooldown()
    {
        return IsEnhancedNormalActive()
            ? enhancedNormalAttackCooldown
            : normalAttackCooldown;
    }

    private float GetHeavyAttackWidth()
    {
        return IsEnhancedHeavyActive()
            ? enhancedHeavyAttackWidth
            : heavyAttackWidth;
    }

    private float GetHeavyAttackHeight()
    {
        return IsEnhancedHeavyActive()
            ? enhancedHeavyAttackHeight
            : heavyAttackHeight;
    }

    private float GetHeavyAttackCooldown()
    {
        return IsEnhancedHeavyActive()
            ? enhancedHeavyAttackCooldown
            : heavyAttackCooldown;
    }

    private float GetThrowAttackCooldown()
    {
        return IsEnhancedThrowActive()
            ? enhancedThrowAttackCooldown
            : throwAttackCooldown;
    }

    private float GetChargeCooldown(bool useEnhancedCharge)
    {
        return useEnhancedCharge
            ? enhancedChargeCooldown
            : chargeCooldown;
    }

    private float GetChargeSpeed(bool useEnhancedCharge)
    {
        return useEnhancedCharge
            ? enhancedChargeSpeed
            : chargeSpeed;
    }

    private void OnDrawGizmos()
    {
        BossController controller = bossController != null
            ? bossController
            : GetComponent<BossController>();

        if (controller == null)
        {
            return;
        }

        if (!showNormalMovesetGizmos &&
            !showEnhancedMovesetGizmos)
        {
            return;
        }

        SpriteRenderer bossSpriteRenderer = GetComponent<SpriteRenderer>();

        bool facesRight = bossSpriteRenderer != null &&
                          bossSpriteRenderer.flipX;

        float direction = facesRight ? 1f : -1f;

        Vector3 normalOrigin = controller.GetAttackOrigin(normalAttackCenter);
        Vector3 heavyOrigin = controller.GetAttackOrigin(heavyAttackCenter);
        Vector3 throwOrigin = controller.GetAttackOrigin(throwAttackCenter);
        Vector3 chargeOrigin = controller.GetAttackOrigin(chargeAttackCenter);

        if (showNormalMovesetGizmos)
        {
            DrawAttackBoxGizmo(
                normalOrigin,
                direction,
                normalAttackWidth,
                normalAttackHeight,
                HexColor("#2196F3")
            );

            DrawAttackBoxGizmo(
                heavyOrigin,
                direction,
                heavyAttackWidth,
                heavyAttackHeight,
                HexColor("#F44336")
            );

            Gizmos.color = HexColor("#4CAF50");
            Gizmos.DrawWireSphere(throwOrigin, throwMaxRange);

            Gizmos.color = HexColor("#FF4FD8");
            Gizmos.DrawWireSphere(chargeOrigin, chargeDamageRadius);
        }

        if (showEnhancedMovesetGizmos)
        {
            DrawAttackBoxGizmo(
                normalOrigin,
                direction,
                enhancedNormalAttackWidth,
                enhancedNormalAttackHeight,
                HexColor("#0D47A1")
            );

            DrawAttackBoxGizmo(
                heavyOrigin,
                direction,
                enhancedHeavyAttackWidth,
                enhancedHeavyAttackHeight,
                HexColor("#8B0000")
            );

            DrawEnhancedThrowGizmo(throwOrigin, direction);

            Gizmos.color = HexColor("#8E24AA");
            Gizmos.DrawWireSphere(chargeOrigin, chargeDamageRadius + 0.08f);
        }

        DrawChargeObstacleBoxCastGizmo();
    }

    private void DrawAttackBoxGizmo(
        Vector3 origin,
        float direction,
        float width,
        float height,
        Color color)
    {
        Vector3 boxCenter =
            origin +
            Vector3.right * direction *
            (width * 0.5f);

        Gizmos.color = color;

        Gizmos.DrawWireCube(
            boxCenter,
            new Vector3(width, height, 0.1f)
        );
    }

    private void DrawEnhancedThrowGizmo(Vector3 throwOrigin, float direction)
    {
        int projectileCount = Mathf.Max(1, enhancedThrowProjectileCount);

        if (projectileCount <= 1)
        {
            return;
        }

        Gizmos.color = HexColor("#1B5E20");

        Vector2 baseDirection = direction > 0f
            ? Vector2.right
            : Vector2.left;

        float previewLength = throwMaxRange;
        float centerIndex = (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = (i - centerIndex) * enhancedThrowSideAngle;

            Vector2 projectileDirection = RotateVector(
                baseDirection,
                angleOffset
            ).normalized;

            Vector3 endPoint =
                throwOrigin +
                (Vector3)(projectileDirection * previewLength);

            Gizmos.DrawLine(throwOrigin, endPoint);
            Gizmos.DrawWireSphere(endPoint, 0.12f);
        }
    }

    private void DrawChargeObstacleBoxCastGizmo()
    {
        if (chargeBodyCollider == null ||
            chargeObstacleLayers.value == 0)
        {
            return;
        }

        Vector2 castOrigin = chargeBodyCollider.bounds.center;

        Vector2 castSize = new Vector2(
            chargeBodyCollider.bounds.size.x,
            chargeBodyCollider.bounds.size.y
        ) * chargeBoxCastScale;

        SpriteRenderer bossSpriteRenderer = GetComponent<SpriteRenderer>();

        bool facesRight = bossSpriteRenderer != null &&
                          bossSpriteRenderer.flipX;

        Vector2 previewDirection = facesRight
            ? Vector2.right
            : Vector2.left;

        float previewDistance = chargeObstaclePadding;

        Vector2 previewCenter =
            castOrigin +
            previewDirection * previewDistance;

        Gizmos.color = HexColor("#FFD600");

        Gizmos.DrawWireCube(
            previewCenter,
            new Vector3(castSize.x, castSize.y, 0.1f)
        );
    }

    private Color HexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }

        return Color.white;
    }
}