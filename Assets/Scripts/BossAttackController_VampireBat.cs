using System.Collections;
using UnityEngine;

public class BossAttackController_VampireBat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private Animator animator;

    [Header("Normal Attack")]
    [SerializeField] private float normalAttackRange = 1.3f;
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float normalAttackLockTime = 0.6f;
    [SerializeField] private int normalDamage = -2;

    private bool isAttacking;
    private float nextNormalAttackTime;

    private void Awake()
    {
        if (bossController == null)
        {
            bossController = GetComponent<BossController>();
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

        if (Time.time >= nextNormalAttackTime && distanceToPlayer <= normalAttackRange)
        {
            StartNormalAttack();
            nextNormalAttackTime = Time.time + normalAttackCooldown;
            return;
        }

        bossController.SetMovementEnabled(true);
    }

    private void StartNormalAttack()
    {
        isAttacking = true;

        bossController.SetMovementEnabled(false);
        bossController.StopMoving();

        if (animator != null)
        {
            animator.SetTrigger("NormalAttack");
        }

        StartCoroutine(AttackLockRoutine());
    }

    private IEnumerator AttackLockRoutine()
    {
        yield return new WaitForSeconds(normalAttackLockTime);

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
            bossController.TryDamagePlayer(normalAttackRange, normalDamage);
        }
    }
}