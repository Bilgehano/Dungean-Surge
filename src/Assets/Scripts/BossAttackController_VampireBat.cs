using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BossAttackController_VampireBat : MonoBehaviour
{
    private enum MeleeAttackType
    {
        Normal,
        Heavy,
    }

    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Animator animator;

    [Header("Attack Centers")]
    [SerializeField] private Transform normalAttackCenter;
    [SerializeField] private Transform heavyAttackCenter;
    [SerializeField] private Transform stompAttackCenter;
    [SerializeField] private Transform castAttackCenter;
    [SerializeField] private Transform castProjectileSpawnPoint;

    [Header("Normal Attack")]
    [SerializeField] private int normalDamage = -2;

    [FormerlySerializedAs("normalAttackRange")]
    [SerializeField] private float normalAttackWidth = 1.4f;

    [SerializeField] private float normalAttackHeight = 1.2f;
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float normalAttackLockTime = 0.6f;

    [Header("Enhanced Normal Attack")]
    [Min(1)]
    [SerializeField] private int enhancedNormalPhase = 3;

    [SerializeField] private float enhancedNormalAttackWidth = 1.7f;
    [SerializeField] private float enhancedNormalAttackHeight = 1.4f;
    [SerializeField] private float enhancedNormalAttackCooldown = 1.2f;

    [Range(0f, 1f)]
    [SerializeField] private float enhancedNormalCritChance = 0.2f;

    [Min(1f)]
    [SerializeField] private float enhancedNormalCritMultiplier = 2f;

    [Header("Heavy Attack")]
    [SerializeField] private int heavyDamage = -5;

    [FormerlySerializedAs("heavyAttackRange")]
    [SerializeField] private float heavyAttackWidth = 1.8f;

    [SerializeField] private float heavyAttackHeight = 2f;
    [SerializeField] private float heavyAttackCooldown = 4f;
    [SerializeField] private float heavyAttackLockTime = 0.9f;

    [Header("Enhanced Heavy Attack")]
    [Min(1)]
    [SerializeField] private int enhancedHeavyPhase = 3;

    [SerializeField] private float enhancedHeavyAttackWidth = 2.2f;
    [SerializeField] private float enhancedHeavyAttackHeight = 2.3f;
    [SerializeField] private float enhancedHeavyAttackCooldown = 3.5f;

    [Header("Melee Combo Settings")]
    [SerializeField] private bool enableNormalToHeavyCombo = true;

    [Range(0f, 1f)]
    [SerializeField] private float normalToHeavyChance = 0.25f;

    [SerializeField] private bool enableHeavyToNormalCombo = true;

    [Range(0f, 1f)]
    [SerializeField] private float heavyToNormalChance = 0.25f;

    [SerializeField] private float meleeComboPause = 0.15f;

    [Min(0)]
    [SerializeField] private int meleeComboMaxFollowUps = 1;

    [Header("Stomp Attack")]
    [SerializeField] private int stompDamage = -6;

    [Min(1)]
    [SerializeField] private int stompPhase = 2;

    [SerializeField] private float stompAttackWidth = 4.4f;
    [SerializeField] private float stompAttackHeight = 4.4f;
    [SerializeField] private float stompAttackCooldown = 6f;
    [SerializeField] private float stompAttackLockTime = 1.2f;

    [Header("Enhanced Stomp Attack")]
    [Min(1)]
    [SerializeField] private int enhancedStompPhase = 4;

    [SerializeField] private float enhancedStompAttackWidth = 5.2f;
    [SerializeField] private float enhancedStompAttackHeight = 5.2f;
    [SerializeField] private float enhancedStompAttackCooldown = 5.5f;

    [SerializeField] private bool enableSecondStompEllipse = true;
    [SerializeField] private float secondStompDelay = 0.35f;
    [SerializeField] private float secondStompWarningDuration = 0.35f;
    [SerializeField] private float secondStompAttackWidth = 7f;
    [SerializeField] private float secondStompAttackHeight = 7f;

    [Min(0f)]
    [SerializeField] private float secondStompDamageMultiplier = 1f;

    [Header("Stomp Warning Visual")]
    [SerializeField]
    private Color stompWarningPreHitColor =
        new Color(1f, 0.35f, 0.75f, 0.28f);

    [SerializeField]
    private Color stompWarningImpactColor =
        new Color(0.65f, 0.15f, 1f, 0.38f);

    [SerializeField]
    private Color secondStompWarningPreHitColor =
        new Color(0.75f, 0.25f, 1f, 0.24f);

    [SerializeField]
    private Color secondStompWarningImpactColor =
        new Color(1f, 0.45f, 0.05f, 0.36f);

    [SerializeField, Min(0f)]
    private float stompWarningAfterHitDuration = 0.2f;

    [SerializeField]
    private int stompWarningSortingOrderOffset = -1;

    [SerializeField]
    private string stompWarningObjectName = "StompWarningVisual";

    [Header("Cast Eruption Attack")]
    [SerializeField] private bool enableCastAttack = false;

    [Min(1)]
    [SerializeField] private int castPhase = 2;

    [SerializeField] private int castDamage = -4;
    [SerializeField] private float castMinRange = 2.5f;

    [FormerlySerializedAs("castAttackRange")]
    [SerializeField] private float castMaxRange = 7f;

    [SerializeField] private float castAttackCooldown = 8f;
    [SerializeField] private float castAttackLockTime = 2.2f;

    [Min(1)]
    [SerializeField] private int castEruptionCount = 5;

    [SerializeField] private float castEruptionStartDistance = 1.2f;
    [SerializeField] private float castEruptionSpacing = 1.1f;
    [SerializeField] private float castEruptionWidth = 1.5f;
    [SerializeField] private float castEruptionHeight = 1.5f;
    [SerializeField] private float castEruptionWarningDuration = 0.35f;
    [SerializeField] private float castEruptionImpactDuration = 0.15f;

    [Header("Enhanced Cast Eruption Attack")]
    [SerializeField] private bool enableEnhancedCastAttack = false;

    [Min(1)]
    [SerializeField] private int enhancedCastPhase = 4;

    [SerializeField] private int enhancedCastDamage = -5;
    [SerializeField] private float enhancedCastMinRange = 2.5f;
    [SerializeField] private float enhancedCastMaxRange = 8f;
    [SerializeField] private float enhancedCastAttackCooldown = 7f;
    [SerializeField] private float enhancedCastAttackLockTime = 2.4f;

    [Min(1)]
    [SerializeField] private int enhancedCastLineCount = 3;

    [SerializeField] private float enhancedCastSideAngle = 15f;

    [Min(1)]
    [SerializeField] private int enhancedCastEruptionCount = 5;

    [SerializeField] private float enhancedCastEruptionStartDistance = 1.2f;
    [SerializeField] private float enhancedCastEruptionSpacing = 1.1f;
    [SerializeField] private float enhancedCastEruptionWidth = 1.6f;
    [SerializeField] private float enhancedCastEruptionHeight = 1.6f;
    [SerializeField] private float enhancedCastEruptionWarningDuration = 0.32f;
    [SerializeField] private float enhancedCastEruptionImpactDuration = 0.15f;

    [Header("Cast Eruption Visual")]
    [SerializeField]
    private Color castEruptionWarningColor =
        new Color(0.45f, 0.1f, 1f, 0.28f);

    [SerializeField]
    private Color castEruptionImpactColor =
        new Color(1f, 0.15f, 0.95f, 0.4f);

    [SerializeField]
    private string castEruptionObjectNamePrefix = "CastEruptionVisual";

    [Header("Gizmo Display")]
    [SerializeField] private bool showNormalMovesetGizmos = true;
    [SerializeField] private bool showEnhancedMovesetGizmos = true;

    [SerializeField, HideInInspector]
    private float stompAttackRange = 2.2f;

    [SerializeField, HideInInspector]
    private bool hasMigratedStompArea;

    private bool isAttacking;
    private bool currentAttackIsStomp;
    private bool stompDamageWasDealt;

    private bool currentAttackIsCast;
    private bool castEruptionWasStarted;
    private bool castSequenceRunning;
    private bool hasStoredCastTarget;

    private Vector2 storedCastTargetPosition;

    private float nextNormalAttackTime;
    private float nextHeavyAttackTime;
    private float nextStompAttackTime;
    private float nextCastAttackTime;

    private GameObject stompWarningObject;
    private SpriteRenderer stompWarningRenderer;
    private Coroutine hideStompWarningCoroutine;
    private Coroutine secondStompRoutine;

    private Coroutine castEruptionRoutine;
    private int castVisualVersion;

    private float activeStompWarningWidth = 1f;
    private float activeStompWarningHeight = 1f;

    private readonly List<SpriteRenderer> castEruptionRenderers =
        new List<SpriteRenderer>();

    private static Sprite cachedFilledStompWarningSprite;
    private static Sprite cachedRingStompWarningSprite;
    private static float cachedRingInnerWidthRatio = -1f;
    private static float cachedRingInnerHeightRatio = -1f;

    private void Awake()
    {
        MigrateLegacyStompAreaIfNeeded();

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

        ValidateSettings();
        EnsureStompWarningVisual();
        HideStompWarningImmediately();
        HideCastEruptionVisuals();
    }

    private void OnValidate()
    {
        MigrateLegacyStompAreaIfNeeded();
        ValidateSettings();
    }

    private void OnDisable()
    {
        StopSecondStompRoutine();
        StopCastEruptionRoutine();
        HideStompWarningImmediately();
        HideCastEruptionVisuals();

        currentAttackIsCast = false;
        castEruptionWasStarted = false;
        castSequenceRunning = false;
        hasStoredCastTarget = false;
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

        Vector2 normalOrigin =
            bossController.GetAttackOrigin(normalAttackCenter);

        Vector2 heavyOrigin =
            bossController.GetAttackOrigin(heavyAttackCenter);

        Vector2 stompOrigin =
            bossController.GetAttackOrigin(stompAttackCenter);

        Vector2 castOrigin =
            bossController.GetAttackOrigin(castAttackCenter);

        float normalDistance =
            bossController.GetDistanceToPlayer(normalOrigin);

        float heavyDistance =
            bossController.GetDistanceToPlayer(heavyOrigin);

        float castDistance =
            bossController.GetDistanceToPlayer(castOrigin);

        if (enableCastAttack &&
            phase >= castPhase &&
            Time.time >= nextCastAttackTime &&
            castDistance >= GetCastMinRange() &&
            castDistance <= GetCastMaxRange())
        {
            StartCastAttack();
            return true;
        }

        if (phase >= stompPhase &&
            Time.time >= nextStompAttackTime &&
            IsPlayerInsideStompArea(stompOrigin))
        {
            StartBossAttack(
                "StompAttack",
                stompAttackLockTime
            );

            nextStompAttackTime =
                Time.time + GetStompAttackCooldown();

            return true;
        }

        if (Time.time >= nextHeavyAttackTime &&
            heavyDistance <= GetHeavyAttackWidth())
        {
            StartMeleeAttack(MeleeAttackType.Heavy);
            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            normalDistance <= GetNormalAttackWidth())
        {
            StartMeleeAttack(MeleeAttackType.Normal);
            return true;
        }

        return false;
    }

    private void StartCastAttack()
    {
        if (bossController == null ||
            !bossController.HasPlayer)
        {
            return;
        }

        storedCastTargetPosition =
            bossController.Player.position;

        hasStoredCastTarget = true;

        bool useEnhancedCast =
            IsEnhancedCastActive();

        float castLockTime = Mathf.Max(
            GetCastAttackLockTime(),
            GetCastSequenceDuration(useEnhancedCast) + 0.15f
        );

        StartBossAttack(
            "Sneer",
            castLockTime,
            true
        );

        nextCastAttackTime =
            Time.time + GetCastAttackCooldown();
    }

    private void StartMeleeAttack(
        MeleeAttackType attackType)
    {
        StartCoroutine(
            MeleeAttackRoutine(
                attackType,
                0
            )
        );
    }

    private IEnumerator MeleeAttackRoutine(
        MeleeAttackType attackType,
        int comboDepth)
    {
        isAttacking = true;

        SetMeleeCooldown(attackType);

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
            animator.SetTrigger(
                GetMeleeTriggerName(attackType)
            );
        }

        yield return new WaitForSeconds(
            GetMeleeLockTime(attackType)
        );

        if (TryGetMeleeFollowUp(
                attackType,
                comboDepth,
                out MeleeAttackType followUpAttack))
        {
            yield return new WaitForSeconds(
                meleeComboPause
            );

            yield return StartCoroutine(
                MeleeAttackRoutine(
                    followUpAttack,
                    comboDepth + 1
                )
            );

            yield break;
        }

        isAttacking = false;

        if (bossController != null &&
            bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
    }

    private bool TryGetMeleeFollowUp(
        MeleeAttackType currentAttack,
        int comboDepth,
        out MeleeAttackType followUpAttack)
    {
        followUpAttack = currentAttack;

        if (comboDepth >= meleeComboMaxFollowUps)
        {
            return false;
        }

        if (currentAttack == MeleeAttackType.Normal)
        {
            if (!enableNormalToHeavyCombo ||
                Random.value > normalToHeavyChance)
            {
                return false;
            }

            followUpAttack = MeleeAttackType.Heavy;
            return CanStartMeleeFollowUp(followUpAttack);
        }

        if (currentAttack == MeleeAttackType.Heavy)
        {
            if (!enableHeavyToNormalCombo ||
                Random.value > heavyToNormalChance)
            {
                return false;
            }

            followUpAttack = MeleeAttackType.Normal;
            return CanStartMeleeFollowUp(followUpAttack);
        }

        return false;
    }

    private bool CanStartMeleeFollowUp(
        MeleeAttackType attackType)
    {
        if (bossController == null ||
            !bossController.IsActive ||
            !bossController.HasPlayer)
        {
            return false;
        }

        Transform attackCenter =
            attackType == MeleeAttackType.Normal
                ? normalAttackCenter
                : heavyAttackCenter;

        Vector2 attackOrigin =
            bossController.GetAttackOrigin(attackCenter);

        float distance =
            bossController.GetDistanceToPlayer(attackOrigin);

        float attackWidth =
            attackType == MeleeAttackType.Normal
                ? GetNormalAttackWidth()
                : GetHeavyAttackWidth();

        return distance <= attackWidth;
    }

    private void SetMeleeCooldown(
        MeleeAttackType attackType)
    {
        if (attackType == MeleeAttackType.Normal)
        {
            nextNormalAttackTime =
                Time.time + GetNormalAttackCooldown();

            return;
        }

        nextHeavyAttackTime =
            Time.time + GetHeavyAttackCooldown();
    }

    private string GetMeleeTriggerName(
        MeleeAttackType attackType)
    {
        return attackType == MeleeAttackType.Normal
            ? "NormalAttack"
            : "HeavyAttack";
    }

    private float GetMeleeLockTime(
        MeleeAttackType attackType)
    {
        return attackType == MeleeAttackType.Normal
            ? normalAttackLockTime
            : heavyAttackLockTime;
    }

    private void StartBossAttack(
        string triggerName,
        float lockTime,
        bool isCastAttack = false)
    {
        isAttacking = true;

        bool isStompAttack =
            triggerName == "StompAttack";

        if (isStompAttack)
        {
            currentAttackIsStomp = true;
            stompDamageWasDealt = false;
            StopSecondStompRoutine();
        }

        if (isCastAttack)
        {
            currentAttackIsCast = true;
            castEruptionWasStarted = false;
            castSequenceRunning = false;

            StopCastEruptionRoutine();
            HideCastEruptionVisuals();
        }

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();

        if (bossController.HasPlayer)
        {
            bossController.FacePosition(
                bossController.Player.position
            );
        }

        if (isStompAttack)
        {
            ShowStompWarningPreHit();
        }

        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }

        StartCoroutine(
            AttackLockRoutine(
                lockTime,
                isStompAttack,
                isCastAttack
            )
        );
    }

    private IEnumerator AttackLockRoutine(
        float lockTime,
        bool wasStompAttack,
        bool wasCastAttack)
    {
        yield return new WaitForSeconds(lockTime);

        if (wasCastAttack)
        {
            while (castSequenceRunning)
            {
                yield return null;
            }

            currentAttackIsCast = false;
            castEruptionWasStarted = false;
            hasStoredCastTarget = false;
        }

        if (wasStompAttack)
        {
            currentAttackIsStomp = false;

            if (!stompDamageWasDealt)
            {
                HideStompWarningImmediately();
            }
        }

        isAttacking = false;

        if (bossController != null &&
            bossController.IsActive)
        {
            bossController.SetMovementEnabled(true);
        }
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

        int finalDamage =
            GetNormalDamageForCurrentHit();

        bossController.TryDamagePlayerInFront(
            normalOrigin,
            GetNormalAttackWidth(),
            GetNormalAttackHeight(),
            finalDamage
        );
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
            GetHeavyAttackWidth(),
            GetHeavyAttackHeight(),
            heavyDamage
        );
    }

    public void ShowStompWarning()
    {
        if (stompDamageWasDealt)
        {
            return;
        }

        ShowStompWarningPreHit();
    }

    private void ShowStompWarningPreHit()
    {
        Vector2 stompOrigin =
            GetCurrentStompOrigin();

        ShowFilledStompWarningWithColor(
            stompWarningPreHitColor,
            stompOrigin,
            GetStompAttackWidth(),
            GetStompAttackHeight()
        );
    }

    private void ShowStompWarningImpact(
        Vector2 stompOrigin,
        float width,
        float height)
    {
        ShowFilledStompWarningWithColor(
            stompWarningImpactColor,
            stompOrigin,
            width,
            height
        );
    }

    private void ShowSecondStompWarningPreHit(
        Vector2 stompOrigin,
        float innerWidth,
        float innerHeight)
    {
        ShowRingStompWarningWithColor(
            secondStompWarningPreHitColor,
            stompOrigin,
            innerWidth,
            innerHeight,
            secondStompAttackWidth,
            secondStompAttackHeight
        );
    }

    private void ShowSecondStompWarningImpact(
        Vector2 stompOrigin,
        float innerWidth,
        float innerHeight)
    {
        ShowRingStompWarningWithColor(
            secondStompWarningImpactColor,
            stompOrigin,
            innerWidth,
            innerHeight,
            secondStompAttackWidth,
            secondStompAttackHeight
        );
    }

    private void ShowFilledStompWarningWithColor(
        Color color,
        Vector2 stompOrigin,
        float width,
        float height)
    {
        ShowStompWarningWithColor(
            color,
            stompOrigin,
            width,
            height,
            false,
            0f,
            0f
        );
    }

    private void ShowRingStompWarningWithColor(
        Color color,
        Vector2 stompOrigin,
        float innerWidth,
        float innerHeight,
        float outerWidth,
        float outerHeight)
    {
        ShowStompWarningWithColor(
            color,
            stompOrigin,
            outerWidth,
            outerHeight,
            true,
            innerWidth,
            innerHeight
        );
    }

    private void ShowStompWarningWithColor(
        Color color,
        Vector2 stompOrigin,
        float width,
        float height,
        bool useRingSprite,
        float innerWidth,
        float innerHeight)
    {
        if (bossController == null)
        {
            return;
        }

        EnsureStompWarningVisual();
        CancelScheduledStompWarningHide();

        if (stompWarningObject == null ||
            stompWarningRenderer == null)
        {
            return;
        }

        activeStompWarningWidth = Mathf.Max(
            0.01f,
            width
        );

        activeStompWarningHeight = Mathf.Max(
            0.01f,
            height
        );

        if (useRingSprite)
        {
            float innerWidthRatio =
                Mathf.Clamp(
                    innerWidth / activeStompWarningWidth,
                    0.01f,
                    0.99f
                );

            float innerHeightRatio =
                Mathf.Clamp(
                    innerHeight / activeStompWarningHeight,
                    0.01f,
                    0.99f
                );

            stompWarningRenderer.sprite =
                GetOrCreateStompRingWarningSprite(
                    innerWidthRatio,
                    innerHeightRatio
                );
        }
        else
        {
            stompWarningRenderer.sprite =
                GetOrCreateFilledStompWarningSprite();
        }

        stompWarningObject.transform.position =
            new Vector3(
                stompOrigin.x,
                stompOrigin.y,
                transform.position.z
            );

        ApplyStompWarningWorldSize();

        stompWarningRenderer.color = color;
        UpdateStompWarningSorting();

        stompWarningObject.SetActive(true);
    }

    private void ApplyStompWarningWorldSize()
    {
        if (stompWarningObject == null ||
            stompWarningRenderer == null ||
            stompWarningRenderer.sprite == null)
        {
            return;
        }

        ApplyAreaVisualWorldSize(
            stompWarningRenderer,
            activeStompWarningWidth,
            activeStompWarningHeight
        );
    }

    public void DealStompDamage()
    {
        stompDamageWasDealt = true;

        Vector2 stompOrigin =
            GetCurrentStompOrigin();

        float firstStompWidth =
            GetStompAttackWidth();

        float firstStompHeight =
            GetStompAttackHeight();

        ShowStompWarningImpact(
            stompOrigin,
            firstStompWidth,
            firstStompHeight
        );

        if (bossController != null)
        {
            bossController.TryDamagePlayerInEllipse(
                stompOrigin,
                firstStompWidth,
                firstStompHeight,
                stompDamage
            );
        }

        StartStompWarningHideAfterHit();

        if (IsEnhancedStompActive() &&
            enableSecondStompEllipse)
        {
            StopSecondStompRoutine();

            secondStompRoutine = StartCoroutine(
                SecondStompEllipseRoutine(
                    stompOrigin,
                    firstStompWidth,
                    firstStompHeight
                )
            );
        }
    }

    private IEnumerator SecondStompEllipseRoutine(
        Vector2 stompOrigin,
        float innerWidth,
        float innerHeight)
    {
        yield return new WaitForSeconds(
            secondStompDelay
        );

        ShowSecondStompWarningPreHit(
            stompOrigin,
            innerWidth,
            innerHeight
        );

        yield return new WaitForSeconds(
            secondStompWarningDuration
        );

        ShowSecondStompWarningImpact(
            stompOrigin,
            innerWidth,
            innerHeight
        );

        int secondDamage =
            Mathf.RoundToInt(
                stompDamage *
                secondStompDamageMultiplier
            );

        TryDamagePlayerInEllipseRing(
            stompOrigin,
            innerWidth,
            innerHeight,
            secondStompAttackWidth,
            secondStompAttackHeight,
            secondDamage
        );

        StartStompWarningHideAfterHit();

        secondStompRoutine = null;
    }

    private bool TryDamagePlayerInEllipseRing(
        Vector2 origin,
        float innerWidth,
        float innerHeight,
        float outerWidth,
        float outerHeight,
        int damage)
    {
        if (bossController == null ||
            !bossController.HasPlayer ||
            bossController.Player == null)
        {
            return false;
        }

        Vector2 playerPosition =
            bossController.Player.position;

        bool isInsideOuterEllipse =
            IsPointInsideEllipse(
                playerPosition,
                origin,
                outerWidth,
                outerHeight
            );

        if (!isInsideOuterEllipse)
        {
            return false;
        }

        bool isInsideInnerEllipse =
            IsPointInsideEllipse(
                playerPosition,
                origin,
                innerWidth,
                innerHeight
            );

        if (isInsideInnerEllipse)
        {
            return false;
        }

        return bossController.TryDamagePlayer(
            playerPosition,
            0.15f,
            damage
        );
    }

    public void DealCastDamage()
    {
        if (!enableCastAttack ||
            bossController == null ||
            !currentAttackIsCast ||
            castEruptionWasStarted)
        {
            return;
        }

        Vector2 targetPosition =
            hasStoredCastTarget
                ? storedCastTargetPosition
                : bossController.Player.position;

        castEruptionWasStarted = true;

        StopCastEruptionRoutine();

        castEruptionRoutine = StartCoroutine(
            CastEruptionSequenceRoutine(
                targetPosition,
                IsEnhancedCastActive()
            )
        );
    }

    private IEnumerator CastEruptionSequenceRoutine(
        Vector2 targetPosition,
        bool useEnhancedCast)
    {
        castSequenceRunning = true;

        castVisualVersion++;
        int visualVersion = castVisualVersion;

        Vector2 castOrigin =
            GetCastEruptionOrigin();

        Vector2 baseDirection =
            targetPosition - castOrigin;

        if (baseDirection.sqrMagnitude <= 0.0001f)
        {
            baseDirection = GetFallbackFacingDirection();
        }

        baseDirection.Normalize();

        int lineCount =
            GetCastLineCount(useEnhancedCast);

        int eruptionCount =
            GetCastEruptionCount(useEnhancedCast);

        float sideAngle =
            GetCastSideAngle(useEnhancedCast);

        float startDistance =
            GetCastEruptionStartDistance(useEnhancedCast);

        float spacing =
            GetCastEruptionSpacing(useEnhancedCast);

        float width =
            GetCastEruptionWidth(useEnhancedCast);

        float height =
            GetCastEruptionHeight(useEnhancedCast);

        float warningDuration =
            GetCastEruptionWarningDuration(useEnhancedCast);

        float impactDuration =
            GetCastEruptionImpactDuration(useEnhancedCast);

        int damage =
            GetCastDamage();

        int requiredVisualCount =
            lineCount * eruptionCount;

        EnsureCastEruptionVisualPool(requiredVisualCount);
        HideCastEruptionVisuals();

        ShowCastEruptionStageWarning(
            0,
            lineCount,
            baseDirection,
            castOrigin,
            sideAngle,
            startDistance,
            spacing,
            width,
            height
        );

        yield return new WaitForSeconds(
            warningDuration
        );

        for (int eruptionIndex = 0;
             eruptionIndex < eruptionCount;
             eruptionIndex++)
        {
            bool playerDamagedThisStage = false;

            for (int lineIndex = 0;
                 lineIndex < lineCount;
                 lineIndex++)
            {
                int visualIndex =
                    GetCastVisualIndex(
                        eruptionIndex,
                        lineIndex,
                        lineCount
                    );

                Vector2 eruptionPosition =
                    GetCastEruptionPosition(
                        castOrigin,
                        baseDirection,
                        lineIndex,
                        lineCount,
                        sideAngle,
                        eruptionIndex,
                        startDistance,
                        spacing
                    );

                ShowCastEruptionVisual(
                    visualIndex,
                    eruptionPosition,
                    width,
                    height,
                    castEruptionImpactColor
                );

                StartCoroutine(
                    HideCastEruptionVisualAfter(
                        visualIndex,
                        impactDuration,
                        visualVersion
                    )
                );

                if (!playerDamagedThisStage)
                {
                    playerDamagedThisStage =
                        TryDamagePlayerInCastEruption(
                            eruptionPosition,
                            width,
                            height,
                            damage
                        );
                }
            }

            int nextEruptionIndex =
                eruptionIndex + 1;

            if (nextEruptionIndex < eruptionCount)
            {
                ShowCastEruptionStageWarning(
                    nextEruptionIndex,
                    lineCount,
                    baseDirection,
                    castOrigin,
                    sideAngle,
                    startDistance,
                    spacing,
                    width,
                    height
                );

                yield return new WaitForSeconds(
                    warningDuration
                );
            }
            else
            {
                yield return new WaitForSeconds(
                    impactDuration
                );
            }
        }

        castSequenceRunning = false;
        castEruptionRoutine = null;
    }

    private void ShowCastEruptionStageWarning(
        int eruptionIndex,
        int lineCount,
        Vector2 baseDirection,
        Vector2 castOrigin,
        float sideAngle,
        float startDistance,
        float spacing,
        float width,
        float height)
    {
        for (int lineIndex = 0;
             lineIndex < lineCount;
             lineIndex++)
        {
            int visualIndex =
                GetCastVisualIndex(
                    eruptionIndex,
                    lineIndex,
                    lineCount
                );

            Vector2 eruptionPosition =
                GetCastEruptionPosition(
                    castOrigin,
                    baseDirection,
                    lineIndex,
                    lineCount,
                    sideAngle,
                    eruptionIndex,
                    startDistance,
                    spacing
                );

            ShowCastEruptionVisual(
                visualIndex,
                eruptionPosition,
                width,
                height,
                castEruptionWarningColor
            );
        }
    }

    private Vector2 GetCastEruptionPosition(
        Vector2 castOrigin,
        Vector2 baseDirection,
        int lineIndex,
        int lineCount,
        float sideAngle,
        int eruptionIndex,
        float startDistance,
        float spacing)
    {
        Vector2 lineDirection =
            GetCastLineDirection(
                baseDirection,
                lineIndex,
                lineCount,
                sideAngle
            );

        float distance =
            startDistance +
            spacing * eruptionIndex;

        return castOrigin +
               lineDirection * distance;
    }

    private Vector2 GetCastLineDirection(
        Vector2 baseDirection,
        int lineIndex,
        int lineCount,
        float sideAngle)
    {
        if (lineCount <= 1)
        {
            return baseDirection.normalized;
        }

        float centerIndex =
            (lineCount - 1) * 0.5f;

        float angleOffset =
            (lineIndex - centerIndex) *
            sideAngle;

        return RotateVector(
            baseDirection,
            angleOffset
        ).normalized;
    }

    private int GetCastVisualIndex(
        int eruptionIndex,
        int lineIndex,
        int lineCount)
    {
        return eruptionIndex * lineCount +
               lineIndex;
    }

    private bool TryDamagePlayerInCastEruption(
        Vector2 eruptionOrigin,
        float width,
        float height,
        int damage)
    {
        if (bossController == null ||
            !bossController.HasPlayer ||
            bossController.Player == null)
        {
            return false;
        }

        Vector2 playerPosition =
            bossController.Player.position;

        bool isInsideEruption =
            IsPointInsideEllipse(
                playerPosition,
                eruptionOrigin,
                width,
                height
            );

        if (!isInsideEruption)
        {
            return false;
        }

        return bossController.TryDamagePlayer(
            playerPosition,
            0.15f,
            damage
        );
    }

    private void EnsureCastEruptionVisualPool(
        int requiredCount)
    {
        requiredCount =
            Mathf.Max(0, requiredCount);

        for (int i = castEruptionRenderers.Count;
             i < requiredCount;
             i++)
        {
            GameObject visualObject =
                new GameObject(
                    $"{castEruptionObjectNamePrefix}_{i}"
                );

            visualObject.transform.SetParent(
                transform,
                false
            );

            SpriteRenderer renderer =
                visualObject.AddComponent<SpriteRenderer>();

            renderer.sprite =
                GetOrCreateFilledStompWarningSprite();

            renderer.color =
                castEruptionWarningColor;

            UpdateAreaVisualSorting(renderer);

            visualObject.SetActive(false);

            castEruptionRenderers.Add(renderer);
        }

        for (int i = 0;
             i < castEruptionRenderers.Count;
             i++)
        {
            if (castEruptionRenderers[i] != null)
            {
                UpdateAreaVisualSorting(
                    castEruptionRenderers[i]
                );
            }
        }
    }

    private void ShowCastEruptionVisual(
        int visualIndex,
        Vector2 position,
        float width,
        float height,
        Color color)
    {
        if (visualIndex < 0 ||
            visualIndex >= castEruptionRenderers.Count)
        {
            return;
        }

        SpriteRenderer renderer =
            castEruptionRenderers[visualIndex];

        if (renderer == null)
        {
            return;
        }

        renderer.sprite =
            GetOrCreateFilledStompWarningSprite();

        renderer.transform.position =
            new Vector3(
                position.x,
                position.y,
                transform.position.z
            );

        ApplyAreaVisualWorldSize(
            renderer,
            width,
            height
        );

        renderer.color = color;
        UpdateAreaVisualSorting(renderer);

        renderer.gameObject.SetActive(true);
    }

    private IEnumerator HideCastEruptionVisualAfter(
        int visualIndex,
        float delay,
        int visualVersion)
    {
        yield return new WaitForSeconds(delay);

        if (visualVersion != castVisualVersion)
        {
            yield break;
        }

        HideCastEruptionVisual(visualIndex);
    }

    private void HideCastEruptionVisual(
        int visualIndex)
    {
        if (visualIndex < 0 ||
            visualIndex >= castEruptionRenderers.Count)
        {
            return;
        }

        SpriteRenderer renderer =
            castEruptionRenderers[visualIndex];

        if (renderer != null)
        {
            renderer.gameObject.SetActive(false);
        }
    }

    private void HideCastEruptionVisuals()
    {
        for (int i = 0;
             i < castEruptionRenderers.Count;
             i++)
        {
            if (castEruptionRenderers[i] != null)
            {
                castEruptionRenderers[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }

    private void StopCastEruptionRoutine()
    {
        castVisualVersion++;

        if (castEruptionRoutine != null)
        {
            StopCoroutine(castEruptionRoutine);
            castEruptionRoutine = null;
        }

        castSequenceRunning = false;
        HideCastEruptionVisuals();
    }

    private Vector2 GetCastEruptionOrigin()
    {
        if (bossController == null)
        {
            return transform.position;
        }

        Transform originTransform =
            castProjectileSpawnPoint != null
                ? castProjectileSpawnPoint
                : castAttackCenter;

        return bossController.GetAttackOrigin(
            originTransform
        );
    }

    private Vector2 GetFallbackFacingDirection()
    {
        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        bool facesRight =
            bossSpriteRenderer != null &&
            bossSpriteRenderer.flipX;

        return facesRight
            ? Vector2.right
            : Vector2.left;
    }

    public void DealCastDamageOld()
    {
        DealCastDamage();
    }

    private void StartStompWarningHideAfterHit()
    {
        CancelScheduledStompWarningHide();

        hideStompWarningCoroutine = StartCoroutine(
            HideStompWarningAfterHitRoutine()
        );
    }

    private IEnumerator HideStompWarningAfterHitRoutine()
    {
        yield return new WaitForSeconds(
            stompWarningAfterHitDuration
        );

        if (stompWarningObject != null)
        {
            stompWarningObject.SetActive(false);
        }

        hideStompWarningCoroutine = null;
    }

    private void CancelScheduledStompWarningHide()
    {
        if (hideStompWarningCoroutine == null)
        {
            return;
        }

        StopCoroutine(hideStompWarningCoroutine);
        hideStompWarningCoroutine = null;
    }

    private void StopSecondStompRoutine()
    {
        if (secondStompRoutine == null)
        {
            return;
        }

        StopCoroutine(secondStompRoutine);
        secondStompRoutine = null;
    }

    private void HideStompWarningImmediately()
    {
        CancelScheduledStompWarningHide();

        if (stompWarningObject != null)
        {
            stompWarningObject.SetActive(false);
        }
    }

    private bool IsPlayerInsideStompArea(
        Vector2 stompOrigin)
    {
        if (bossController == null ||
            !bossController.HasPlayer)
        {
            return false;
        }

        return IsPointInsideEllipse(
            bossController.Player.position,
            stompOrigin,
            GetStompAttackWidth(),
            GetStompAttackHeight()
        );
    }

    private bool IsPointInsideEllipse(
        Vector2 point,
        Vector2 origin,
        float width,
        float height)
    {
        float horizontalRadius = Mathf.Max(
            width * 0.5f,
            0.01f
        );

        float verticalRadius = Mathf.Max(
            height * 0.5f,
            0.01f
        );

        Vector2 offset =
            point - origin;

        float ellipseValue =
            (offset.x * offset.x) /
            (horizontalRadius * horizontalRadius) +
            (offset.y * offset.y) /
            (verticalRadius * verticalRadius);

        return ellipseValue <= 1f;
    }

    private Vector2 GetCurrentStompOrigin()
    {
        if (bossController == null)
        {
            return transform.position;
        }

        return bossController.GetAttackOrigin(
            stompAttackCenter
        );
    }

    private void EnsureStompWarningVisual()
    {
        if (stompWarningObject != null &&
            stompWarningRenderer != null)
        {
            return;
        }

        Transform existingChild =
            transform.Find(stompWarningObjectName);

        if (existingChild != null)
        {
            stompWarningObject =
                existingChild.gameObject;

            stompWarningRenderer =
                stompWarningObject.GetComponent<SpriteRenderer>();
        }

        if (stompWarningObject == null)
        {
            stompWarningObject =
                new GameObject(stompWarningObjectName);

            stompWarningObject.transform.SetParent(
                transform,
                false
            );

            stompWarningRenderer =
                stompWarningObject.AddComponent<SpriteRenderer>();
        }

        if (stompWarningRenderer == null)
        {
            stompWarningRenderer =
                stompWarningObject.GetComponent<SpriteRenderer>();

            if (stompWarningRenderer == null)
            {
                stompWarningRenderer =
                    stompWarningObject.AddComponent<SpriteRenderer>();
            }
        }

        stompWarningRenderer.sprite =
            GetOrCreateFilledStompWarningSprite();

        stompWarningRenderer.color =
            stompWarningPreHitColor;

        UpdateStompWarningSorting();
    }

    private void UpdateStompWarningSorting()
    {
        if (stompWarningRenderer == null)
        {
            return;
        }

        UpdateAreaVisualSorting(
            stompWarningRenderer
        );
    }

    private void UpdateAreaVisualSorting(
        SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        if (bossSpriteRenderer == null)
        {
            return;
        }

        renderer.sortingLayerID =
            bossSpriteRenderer.sortingLayerID;

        renderer.sortingOrder =
            bossSpriteRenderer.sortingOrder +
            stompWarningSortingOrderOffset;
    }

    private void ApplyAreaVisualWorldSize(
        SpriteRenderer renderer,
        float width,
        float height)
    {
        if (renderer == null ||
            renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize =
            renderer.sprite.bounds.size;

        float spriteWidth = Mathf.Max(
            spriteSize.x,
            0.0001f
        );

        float spriteHeight = Mathf.Max(
            spriteSize.y,
            0.0001f
        );

        Vector3 parentScale = Vector3.one;

        if (renderer.transform.parent != null)
        {
            parentScale =
                renderer.transform.parent.lossyScale;
        }

        float parentScaleX = Mathf.Max(
            Mathf.Abs(parentScale.x),
            0.0001f
        );

        float parentScaleY = Mathf.Max(
            Mathf.Abs(parentScale.y),
            0.0001f
        );

        renderer.transform.localScale =
            new Vector3(
                width / (spriteWidth * parentScaleX),
                height / (spriteHeight * parentScaleY),
                1f
            );
    }

    private static Sprite GetOrCreateFilledStompWarningSprite()
    {
        if (cachedFilledStompWarningSprite != null)
        {
            return cachedFilledStompWarningSprite;
        }

        const int size = 128;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "RuntimeStompWarningTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(
            (size - 1) * 0.5f,
            (size - 1) * 0.5f
        );

        float radius = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelPosition =
                    new Vector2(x, y);

                float normalizedDistance =
                    Vector2.Distance(
                        pixelPosition,
                        center
                    ) / radius;

                float alpha;

                if (normalizedDistance <= 0.92f)
                {
                    alpha = 1f;
                }
                else if (normalizedDistance <= 1f)
                {
                    alpha = Mathf.InverseLerp(
                        1f,
                        0.92f,
                        normalizedDistance
                    );
                }
                else
                {
                    alpha = 0f;
                }

                texture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        texture.Apply();

        cachedFilledStompWarningSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        cachedFilledStompWarningSprite.name =
            "RuntimeStompWarningSprite";

        return cachedFilledStompWarningSprite;
    }

    private static Sprite GetOrCreateStompRingWarningSprite(
        float innerWidthRatio,
        float innerHeightRatio)
    {
        innerWidthRatio = Mathf.Clamp(
            innerWidthRatio,
            0.01f,
            0.99f
        );

        innerHeightRatio = Mathf.Clamp(
            innerHeightRatio,
            0.01f,
            0.99f
        );

        bool canReuseCachedRing =
            cachedRingStompWarningSprite != null &&
            Mathf.Approximately(
                cachedRingInnerWidthRatio,
                innerWidthRatio
            ) &&
            Mathf.Approximately(
                cachedRingInnerHeightRatio,
                innerHeightRatio
            );

        if (canReuseCachedRing)
        {
            return cachedRingStompWarningSprite;
        }

        const int size = 128;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "RuntimeStompRingWarningTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(
            (size - 1) * 0.5f,
            (size - 1) * 0.5f
        );

        float radius = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelPosition =
                    new Vector2(x, y);

                float normalizedX =
                    (pixelPosition.x - center.x) / radius;

                float normalizedY =
                    (pixelPosition.y - center.y) / radius;

                float outerDistance =
                    Mathf.Sqrt(
                        normalizedX * normalizedX +
                        normalizedY * normalizedY
                    );

                float innerDistance =
                    Mathf.Sqrt(
                        (normalizedX / innerWidthRatio) *
                        (normalizedX / innerWidthRatio) +
                        (normalizedY / innerHeightRatio) *
                        (normalizedY / innerHeightRatio)
                    );

                float outerAlpha;

                if (outerDistance <= 0.92f)
                {
                    outerAlpha = 1f;
                }
                else if (outerDistance <= 1f)
                {
                    outerAlpha = Mathf.InverseLerp(
                        1f,
                        0.92f,
                        outerDistance
                    );
                }
                else
                {
                    outerAlpha = 0f;
                }

                float innerCutoutAlpha;

                if (innerDistance <= 0.92f)
                {
                    innerCutoutAlpha = 0f;
                }
                else if (innerDistance <= 1f)
                {
                    innerCutoutAlpha = Mathf.InverseLerp(
                        0.92f,
                        1f,
                        innerDistance
                    );
                }
                else
                {
                    innerCutoutAlpha = 1f;
                }

                float finalAlpha =
                    outerAlpha * innerCutoutAlpha;

                texture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, finalAlpha)
                );
            }
        }

        texture.Apply();

        cachedRingStompWarningSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        cachedRingStompWarningSprite.name =
            "RuntimeStompRingWarningSprite";

        cachedRingInnerWidthRatio = innerWidthRatio;
        cachedRingInnerHeightRatio = innerHeightRatio;

        return cachedRingStompWarningSprite;
    }

    private void MigrateLegacyStompAreaIfNeeded()
    {
        if (hasMigratedStompArea)
        {
            return;
        }

        float oldRadius = Mathf.Max(
            stompAttackRange,
            0.01f
        );

        float diameter = oldRadius * 2f;

        stompAttackWidth = diameter;
        stompAttackHeight = diameter;

        hasMigratedStompArea = true;
    }

    private int GetCurrentPhase()
    {
        return bossHealth != null
            ? bossHealth.CurrentPhase
            : 1;
    }

    private bool IsEnhancedNormalActive()
    {
        return GetCurrentPhase() >= enhancedNormalPhase;
    }

    private bool IsEnhancedHeavyActive()
    {
        return GetCurrentPhase() >= enhancedHeavyPhase;
    }

    private bool IsEnhancedStompActive()
    {
        return GetCurrentPhase() >= enhancedStompPhase;
    }

    private bool IsEnhancedCastActive()
    {
        return enableEnhancedCastAttack &&
               GetCurrentPhase() >= enhancedCastPhase;
    }

    private int GetNormalDamageForCurrentHit()
    {
        if (!IsEnhancedNormalActive())
        {
            return normalDamage;
        }

        bool isCriticalHit =
            Random.value < enhancedNormalCritChance;

        if (!isCriticalHit)
        {
            return normalDamage;
        }

        return Mathf.RoundToInt(
            normalDamage *
            enhancedNormalCritMultiplier
        );
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

    private float GetStompAttackWidth()
    {
        return IsEnhancedStompActive()
            ? enhancedStompAttackWidth
            : stompAttackWidth;
    }

    private float GetStompAttackHeight()
    {
        return IsEnhancedStompActive()
            ? enhancedStompAttackHeight
            : stompAttackHeight;
    }

    private float GetStompAttackCooldown()
    {
        return IsEnhancedStompActive()
            ? enhancedStompAttackCooldown
            : stompAttackCooldown;
    }

    private int GetCastDamage()
    {
        return IsEnhancedCastActive()
            ? enhancedCastDamage
            : castDamage;
    }

    private float GetCastMinRange()
    {
        return IsEnhancedCastActive()
            ? enhancedCastMinRange
            : castMinRange;
    }

    private float GetCastMaxRange()
    {
        return IsEnhancedCastActive()
            ? enhancedCastMaxRange
            : castMaxRange;
    }

    private float GetCastAttackCooldown()
    {
        return IsEnhancedCastActive()
            ? enhancedCastAttackCooldown
            : castAttackCooldown;
    }

    private float GetCastAttackLockTime()
    {
        return IsEnhancedCastActive()
            ? enhancedCastAttackLockTime
            : castAttackLockTime;
    }

    private int GetCastLineCount(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastLineCount
            : 1;
    }

    private float GetCastSideAngle(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastSideAngle
            : 0f;
    }

    private int GetCastEruptionCount(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionCount
            : castEruptionCount;
    }

    private float GetCastEruptionStartDistance(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionStartDistance
            : castEruptionStartDistance;
    }

    private float GetCastEruptionSpacing(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionSpacing
            : castEruptionSpacing;
    }

    private float GetCastEruptionWidth(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionWidth
            : castEruptionWidth;
    }

    private float GetCastEruptionHeight(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionHeight
            : castEruptionHeight;
    }

    private float GetCastEruptionWarningDuration(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionWarningDuration
            : castEruptionWarningDuration;
    }

    private float GetCastEruptionImpactDuration(
        bool useEnhancedCast)
    {
        return useEnhancedCast
            ? enhancedCastEruptionImpactDuration
            : castEruptionImpactDuration;
    }

    private float GetCastSequenceDuration(
        bool useEnhancedCast)
    {
        return
            GetCastEruptionWarningDuration(useEnhancedCast) *
            GetCastEruptionCount(useEnhancedCast) +
            GetCastEruptionImpactDuration(useEnhancedCast);
    }

    private void ValidateSettings()
    {
        enhancedNormalPhase = Mathf.Max(1, enhancedNormalPhase);
        enhancedHeavyPhase = Mathf.Max(1, enhancedHeavyPhase);
        stompPhase = Mathf.Max(1, stompPhase);
        enhancedStompPhase = Mathf.Max(1, enhancedStompPhase);
        castPhase = Mathf.Max(1, castPhase);
        enhancedCastPhase = Mathf.Max(1, enhancedCastPhase);

        normalAttackWidth = Mathf.Max(0.01f, normalAttackWidth);
        normalAttackHeight = Mathf.Max(0.01f, normalAttackHeight);
        enhancedNormalAttackWidth = Mathf.Max(0.01f, enhancedNormalAttackWidth);
        enhancedNormalAttackHeight = Mathf.Max(0.01f, enhancedNormalAttackHeight);

        heavyAttackWidth = Mathf.Max(0.01f, heavyAttackWidth);
        heavyAttackHeight = Mathf.Max(0.01f, heavyAttackHeight);
        enhancedHeavyAttackWidth = Mathf.Max(0.01f, enhancedHeavyAttackWidth);
        enhancedHeavyAttackHeight = Mathf.Max(0.01f, enhancedHeavyAttackHeight);

        stompAttackWidth = Mathf.Max(0.01f, stompAttackWidth);
        stompAttackHeight = Mathf.Max(0.01f, stompAttackHeight);
        enhancedStompAttackWidth = Mathf.Max(0.01f, enhancedStompAttackWidth);
        enhancedStompAttackHeight = Mathf.Max(0.01f, enhancedStompAttackHeight);

        secondStompAttackWidth = Mathf.Max(
            enhancedStompAttackWidth + 0.01f,
            secondStompAttackWidth
        );

        secondStompAttackHeight = Mathf.Max(
            enhancedStompAttackHeight + 0.01f,
            secondStompAttackHeight
        );

        normalAttackCooldown = Mathf.Max(0f, normalAttackCooldown);
        enhancedNormalAttackCooldown = Mathf.Max(0f, enhancedNormalAttackCooldown);
        heavyAttackCooldown = Mathf.Max(0f, heavyAttackCooldown);
        enhancedHeavyAttackCooldown = Mathf.Max(0f, enhancedHeavyAttackCooldown);
        stompAttackCooldown = Mathf.Max(0f, stompAttackCooldown);
        enhancedStompAttackCooldown = Mathf.Max(0f, enhancedStompAttackCooldown);
        castAttackCooldown = Mathf.Max(0f, castAttackCooldown);
        enhancedCastAttackCooldown = Mathf.Max(0f, enhancedCastAttackCooldown);

        enhancedNormalCritChance = Mathf.Clamp01(enhancedNormalCritChance);
        enhancedNormalCritMultiplier = Mathf.Max(1f, enhancedNormalCritMultiplier);

        normalToHeavyChance = Mathf.Clamp01(normalToHeavyChance);
        heavyToNormalChance = Mathf.Clamp01(heavyToNormalChance);

        meleeComboPause = Mathf.Max(0f, meleeComboPause);
        meleeComboMaxFollowUps = Mathf.Max(0, meleeComboMaxFollowUps);

        secondStompDelay = Mathf.Max(0f, secondStompDelay);
        secondStompWarningDuration = Mathf.Max(0f, secondStompWarningDuration);
        secondStompDamageMultiplier = Mathf.Max(0f, secondStompDamageMultiplier);

        stompWarningAfterHitDuration = Mathf.Max(0f, stompWarningAfterHitDuration);

        castMinRange = Mathf.Max(0f, castMinRange);
        castMaxRange = Mathf.Max(castMinRange, castMaxRange);

        enhancedCastMinRange = Mathf.Max(0f, enhancedCastMinRange);
        enhancedCastMaxRange = Mathf.Max(enhancedCastMinRange, enhancedCastMaxRange);

        castEruptionCount = Mathf.Max(1, castEruptionCount);
        castEruptionStartDistance = Mathf.Max(0f, castEruptionStartDistance);
        castEruptionSpacing = Mathf.Max(0.01f, castEruptionSpacing);
        castEruptionWidth = Mathf.Max(0.01f, castEruptionWidth);
        castEruptionHeight = Mathf.Max(0.01f, castEruptionHeight);
        castEruptionWarningDuration = Mathf.Max(0f, castEruptionWarningDuration);
        castEruptionImpactDuration = Mathf.Max(0f, castEruptionImpactDuration);

        enhancedCastLineCount = Mathf.Max(1, enhancedCastLineCount);
        enhancedCastSideAngle = Mathf.Max(0f, enhancedCastSideAngle);
        enhancedCastEruptionCount = Mathf.Max(1, enhancedCastEruptionCount);
        enhancedCastEruptionStartDistance = Mathf.Max(0f, enhancedCastEruptionStartDistance);
        enhancedCastEruptionSpacing = Mathf.Max(0.01f, enhancedCastEruptionSpacing);
        enhancedCastEruptionWidth = Mathf.Max(0.01f, enhancedCastEruptionWidth);
        enhancedCastEruptionHeight = Mathf.Max(0.01f, enhancedCastEruptionHeight);
        enhancedCastEruptionWarningDuration = Mathf.Max(0f, enhancedCastEruptionWarningDuration);
        enhancedCastEruptionImpactDuration = Mathf.Max(0f, enhancedCastEruptionImpactDuration);

        activeStompWarningWidth = Mathf.Max(0.01f, activeStompWarningWidth);
        activeStompWarningHeight = Mathf.Max(0.01f, activeStompWarningHeight);
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

        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        bool facesRight =
            bossSpriteRenderer != null &&
            bossSpriteRenderer.flipX;

        float direction = facesRight ? 1f : -1f;

        Vector2 facingDirection =
            facesRight
                ? Vector2.right
                : Vector2.left;

        Vector3 normalOrigin =
            controller.GetAttackOrigin(normalAttackCenter);

        Vector3 heavyOrigin =
            controller.GetAttackOrigin(heavyAttackCenter);

        Vector3 stompOrigin =
            controller.GetAttackOrigin(stompAttackCenter);

        Vector3 castOrigin =
            controller.GetAttackOrigin(castAttackCenter);

        Transform castPreviewTransform =
            castProjectileSpawnPoint != null
                ? castProjectileSpawnPoint
                : castAttackCenter;

        Vector3 castSpawnOrigin =
            controller.GetAttackOrigin(castPreviewTransform);

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

            DrawEllipseGizmo(
                stompOrigin,
                stompAttackWidth,
                stompAttackHeight,
                HexColor("#FF4FD8")
            );

            Gizmos.color = HexColor("#4CAF50");

            Gizmos.DrawWireSphere(
                castOrigin,
                castMaxRange
            );

            Gizmos.DrawWireSphere(
                castOrigin,
                castMinRange
            );

            DrawCastEruptionGizmos(
                castSpawnOrigin,
                facingDirection,
                1,
                0f,
                castEruptionCount,
                castEruptionStartDistance,
                castEruptionSpacing,
                castEruptionWidth,
                castEruptionHeight,
                HexColor("#4CAF50")
            );
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

            DrawEllipseGizmo(
                stompOrigin,
                enhancedStompAttackWidth,
                enhancedStompAttackHeight,
                HexColor("#8E24AA")
            );

            if (enableSecondStompEllipse)
            {
                DrawEllipseRingGizmo(
                    stompOrigin,
                    enhancedStompAttackWidth,
                    enhancedStompAttackHeight,
                    secondStompAttackWidth,
                    secondStompAttackHeight,
                    HexColor("#FF9800")
                );
            }

            Gizmos.color = HexColor("#1B5E20");

            Gizmos.DrawWireSphere(
                castOrigin,
                enhancedCastMaxRange
            );

            Gizmos.DrawWireSphere(
                castOrigin,
                enhancedCastMinRange
            );

            DrawCastEruptionGizmos(
                castSpawnOrigin,
                facingDirection,
                enhancedCastLineCount,
                enhancedCastSideAngle,
                enhancedCastEruptionCount,
                enhancedCastEruptionStartDistance,
                enhancedCastEruptionSpacing,
                enhancedCastEruptionWidth,
                enhancedCastEruptionHeight,
                HexColor("#1B5E20")
            );
        }
    }

    private void DrawCastEruptionGizmos(
        Vector3 origin,
        Vector2 baseDirection,
        int lineCount,
        float sideAngle,
        int eruptionCount,
        float startDistance,
        float spacing,
        float width,
        float height,
        Color color)
    {
        lineCount = Mathf.Max(1, lineCount);
        eruptionCount = Mathf.Max(1, eruptionCount);

        for (int lineIndex = 0;
             lineIndex < lineCount;
             lineIndex++)
        {
            Vector2 lineDirection =
                GetCastLineDirection(
                    baseDirection,
                    lineIndex,
                    lineCount,
                    sideAngle
                );

            for (int eruptionIndex = 0;
                 eruptionIndex < eruptionCount;
                 eruptionIndex++)
            {
                float distance =
                    startDistance +
                    spacing * eruptionIndex;

                Vector3 eruptionPosition =
                    origin +
                    (Vector3)(lineDirection * distance);

                DrawEllipseGizmo(
                    eruptionPosition,
                    width,
                    height,
                    color
                );
            }
        }
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
            new Vector3(
                width,
                height,
                0.1f
            )
        );
    }

    private void DrawEllipseGizmo(
        Vector3 origin,
        float width,
        float height,
        Color color)
    {
        Matrix4x4 previousMatrix =
            Gizmos.matrix;

        Gizmos.color = color;

        Gizmos.matrix = Matrix4x4.TRS(
            origin,
            Quaternion.identity,
            new Vector3(
                width,
                height,
                0.1f
            )
        );

        Gizmos.DrawWireSphere(
            Vector3.zero,
            0.5f
        );

        Gizmos.matrix = previousMatrix;
    }

    private void DrawEllipseRingGizmo(
        Vector3 origin,
        float innerWidth,
        float innerHeight,
        float outerWidth,
        float outerHeight,
        Color color)
    {
        DrawEllipseGizmo(
            origin,
            outerWidth,
            outerHeight,
            color
        );

        DrawEllipseGizmo(
            origin,
            innerWidth,
            innerHeight,
            color
        );
    }

    private Vector2 RotateVector(
        Vector2 vector,
        float degrees)
    {
        float radians =
            degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private Color HexColor(
        string hex)
    {
        if (ColorUtility.TryParseHtmlString(
                hex,
                out Color color))
        {
            return color;
        }

        return Color.white;
    }
}