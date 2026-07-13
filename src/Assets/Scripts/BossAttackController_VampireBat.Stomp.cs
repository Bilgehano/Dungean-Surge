using System.Collections;
using UnityEngine;

public partial class BossAttackController_VampireBat
{
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

    private void StopSecondStompRoutine()
    {
        if (secondStompRoutine == null)
        {
            return;
        }

        StopCoroutine(secondStompRoutine);
        secondStompRoutine = null;
    }
}