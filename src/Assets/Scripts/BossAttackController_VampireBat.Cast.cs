using System.Collections;
using UnityEngine;

public partial class BossAttackController_VampireBat
{
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

    public void DealCastDamageOld()
    {
        DealCastDamage();
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
}