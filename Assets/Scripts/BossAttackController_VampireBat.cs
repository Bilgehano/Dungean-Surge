using System.Collections;
using UnityEngine;

public class BossAttackController_VampireBat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Animator animator;

    [Header("Attack Ranges")]
    [SerializeField] private float normalAttackRange = 1.4f;
    [SerializeField] private float heavyAttackRange = 1.8f;
    [SerializeField] private float stompAttackRange = 2.2f;

    [Header("Future Cast Attack")]
    [SerializeField] private bool enableCastAttack = false;
    [SerializeField] private float castAttackRange = 5f;

    [Header("Attack Cooldowns")]
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float heavyAttackCooldown = 4f;
    [SerializeField] private float stompAttackCooldown = 6f;
    [SerializeField] private float castAttackCooldown = 8f;

    [Header("Attack Lock Times")]
    [SerializeField] private float normalAttackLockTime = 0.6f;
    [SerializeField] private float heavyAttackLockTime = 0.9f;
    [SerializeField] private float stompAttackLockTime = 1.2f;
    [SerializeField] private float castAttackLockTime = 1.4f;

    [Header("Damage")]
    [SerializeField] private int normalDamage = -2;
    [SerializeField] private int heavyDamage = -5;
    [SerializeField] private int stompDamage = -6;
    [SerializeField] private int castDamage = -4;

    private bool isAttacking;

    private float nextNormalAttackTime;
    private float nextHeavyAttackTime;
    private float nextStompAttackTime;
    private float nextCastAttackTime;

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
        if (bossController == null || !bossController.IsActive || !bossController.HasPlayer)
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
        int phase = bossHealth != null ? bossHealth.CurrentPhase : 1;

        if (enableCastAttack &&
            phase >= 4 &&
            Time.time >= nextCastAttackTime &&
            distanceToPlayer <= castAttackRange)
        {
            StartBossAttack("Sneer", castAttackLockTime);
            nextCastAttackTime = Time.time + castAttackCooldown;
            return true;
        }

        if (phase >= 3 &&
            Time.time >= nextStompAttackTime &&
            distanceToPlayer <= stompAttackRange)
        {
            StartBossAttack("StompAttack", stompAttackLockTime);
            nextStompAttackTime = Time.time + stompAttackCooldown;
            return true;
        }

        if (phase >= 2 &&
            Time.time >= nextHeavyAttackTime &&
            distanceToPlayer <= heavyAttackRange)
        {
            StartBossAttack("HeavyAttack", heavyAttackLockTime);
            nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            distanceToPlayer <= normalAttackRange)
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

    public void DealNormalDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayer(normalAttackRange, normalDamage);
        }
    }

    public void DealHeavyDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayer(heavyAttackRange, heavyDamage);
        }
    }

    public void DealStompDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayer(stompAttackRange, stompDamage);
        }
    }

    public void DealCastDamage()
    {
        if (!enableCastAttack)
        {
            return;
        }

        if (bossController != null)
        {
            bossController.TryDamagePlayer(castAttackRange, castDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BossController controller = bossController != null ? bossController : GetComponent<BossController>();

        if (controller == null)
        {
            return;
        }

        Vector3 origin = controller.AttackOrigin;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, normalAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, heavyAttackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origin, stompAttackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, castAttackRange);
    }
}