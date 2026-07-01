using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BossAttackController_VampireBat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossController bossController;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Animator animator;

    [Header("Normal Attack")]
    [SerializeField] private int normalDamage = -2;

    [FormerlySerializedAs("normalAttackRange")]
    [SerializeField] private float normalAttackWidth = 1.4f;

    [SerializeField] private float normalAttackHeight = 1.2f;
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float normalAttackLockTime = 0.6f;

    [Header("Heavy Attack")]
    [SerializeField] private int heavyDamage = -5;

    [FormerlySerializedAs("heavyAttackRange")]
    [SerializeField] private float heavyAttackWidth = 1.8f;

    [SerializeField] private float heavyAttackHeight = 2f;
    [SerializeField] private float heavyAttackCooldown = 4f;
    [SerializeField] private float heavyAttackLockTime = 0.9f;

    [Header("Stomp Attack")]
    [SerializeField] private int stompDamage = -6;
    [SerializeField] private float stompAttackWidth = 4.4f;
    [SerializeField] private float stompAttackHeight = 4.4f;
    [SerializeField] private float stompAttackCooldown = 6f;
    [SerializeField] private float stompAttackLockTime = 1.2f;

    [Header("Future Cast Attack")]
    [SerializeField] private int castDamage = -4;
    [SerializeField] private float castAttackRange = 5f;
    [SerializeField] private float castAttackCooldown = 8f;
    [SerializeField] private float castAttackLockTime = 1.4f;
    [SerializeField] private bool enableCastAttack = false;

    [SerializeField, HideInInspector]
    private float stompAttackRange = 2.2f;

    [SerializeField, HideInInspector]
    private bool hasMigratedStompArea;

    private bool isAttacking;

    private float nextNormalAttackTime;
    private float nextHeavyAttackTime;
    private float nextStompAttackTime;
    private float nextCastAttackTime;

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
    }

    private void OnValidate()
    {
        MigrateLegacyStompAreaIfNeeded();
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

        float distanceToPlayer = bossController.GetDistanceToPlayer();

        if (TryStartAttack(distanceToPlayer))
        {
            return;
        }

        bossController.SetMovementEnabled(true);
    }

    private bool TryStartAttack(float distanceToPlayer)
    {
        int phase = bossHealth != null
            ? bossHealth.CurrentPhase
            : 1;

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
            IsPlayerInsideStompArea())
        {
            StartBossAttack("StompAttack", stompAttackLockTime);
            nextStompAttackTime = Time.time + stompAttackCooldown;
            return true;
        }

        if (phase >= 2 &&
            Time.time >= nextHeavyAttackTime &&
            distanceToPlayer <= heavyAttackWidth)
        {
            StartBossAttack("HeavyAttack", heavyAttackLockTime);
            nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            distanceToPlayer <= normalAttackWidth)
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

    public void DealNormalDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayerInFront(
                normalAttackWidth,
                normalAttackHeight,
                normalDamage
            );
        }
    }

    public void DealHeavyDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayerInFront(
                heavyAttackWidth,
                heavyAttackHeight,
                heavyDamage
            );
        }
    }

    public void DealStompDamage()
    {
        if (bossController != null)
        {
            bossController.TryDamagePlayerInEllipse(
                stompAttackWidth,
                stompAttackHeight,
                stompDamage
            );
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
            bossController.TryDamagePlayer(
                castAttackRange,
                castDamage
            );
        }
    }

    private bool IsPlayerInsideStompArea()
    {
        if (bossController == null ||
            !bossController.HasPlayer)
        {
            return false;
        }

        float horizontalRadius = Mathf.Max(
            stompAttackWidth * 0.5f,
            0.01f
        );

        float verticalRadius = Mathf.Max(
            stompAttackHeight * 0.5f,
            0.01f
        );

        Vector2 offset =
            (Vector2)bossController.Player.position -
            bossController.AttackOrigin;

        float ellipseValue =
            (offset.x * offset.x) /
            (horizontalRadius * horizontalRadius) +
            (offset.y * offset.y) /
            (verticalRadius * verticalRadius);

        return ellipseValue <= 1f;
    }

    private void MigrateLegacyStompAreaIfNeeded()
    {
        if (hasMigratedStompArea)
        {
            return;
        }

        float oldRadius = Mathf.Max(stompAttackRange, 0.01f);
        float diameter = oldRadius * 2f;

        stompAttackWidth = diameter;
        stompAttackHeight = diameter;

        hasMigratedStompArea = true;
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

        Vector3 origin = controller.AttackOrigin;

        Vector3 normalAttackBoxCenter = origin +
            Vector3.right * direction * (normalAttackWidth * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            normalAttackBoxCenter,
            new Vector3(
                normalAttackWidth,
                normalAttackHeight,
                0.1f
            )
        );

        Vector3 heavyAttackBoxCenter = origin +
            Vector3.right * direction * (heavyAttackWidth * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            heavyAttackBoxCenter,
            new Vector3(
                heavyAttackWidth,
                heavyAttackHeight,
                0.1f
            )
        );

        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.color = Color.magenta;
        Gizmos.matrix = Matrix4x4.TRS(
            origin,
            Quaternion.identity,
            new Vector3(
                stompAttackWidth,
                stompAttackHeight,
                0.1f
            )
        );

        Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
        Gizmos.matrix = previousMatrix;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, castAttackRange);
    }
}