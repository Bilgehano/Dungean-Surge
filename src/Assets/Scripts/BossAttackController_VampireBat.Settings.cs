using UnityEngine;

public partial class BossAttackController_VampireBat
{
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
}