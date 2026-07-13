using System.Collections;
using UnityEngine;

public partial class BossAttackController_VampireBat
{
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
}