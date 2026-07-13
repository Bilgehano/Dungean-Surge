using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class BossAttackController_VampireBat : MonoBehaviour
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
}