using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BossAttackController_VampireBat : MonoBehaviour
{
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

    [Header("Stomp Warning Visual")]
    [SerializeField]
    private Color stompWarningPreHitColor =
        new Color(1f, 0.35f, 0.75f, 0.28f);

    [SerializeField]
    private Color stompWarningImpactColor =
        new Color(0.65f, 0.15f, 1f, 0.38f);

    [SerializeField, Min(0f)]
    private float stompWarningAfterHitDuration = 0.2f;

    [SerializeField]
    private int stompWarningSortingOrderOffset = -1;

    [SerializeField]
    private string stompWarningObjectName = "StompWarningVisual";

    [Header("Future Cast Attack")]
    [SerializeField] private int castDamage = -4;
    [SerializeField] private float castMinRange = 2.5f;

    [FormerlySerializedAs("castAttackRange")]
    [SerializeField] private float castMaxRange = 5f;

    [SerializeField] private float castAttackCooldown = 8f;
    [SerializeField] private float castAttackLockTime = 1.4f;
    [SerializeField] private bool enableCastAttack = false;

    [SerializeField, HideInInspector]
    private float stompAttackRange = 2.2f;

    [SerializeField, HideInInspector]
    private bool hasMigratedStompArea;

    private bool isAttacking;
    private bool currentAttackIsStomp;
    private bool stompDamageWasDealt;

    private float nextNormalAttackTime;
    private float nextHeavyAttackTime;
    private float nextStompAttackTime;
    private float nextCastAttackTime;

    private GameObject stompWarningObject;
    private SpriteRenderer stompWarningRenderer;
    private Coroutine hideStompWarningCoroutine;

    private static Sprite cachedStompWarningSprite;

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
    }

    private void OnValidate()
    {
        MigrateLegacyStompAreaIfNeeded();
        ValidateSettings();
    }

    private void OnDisable()
    {
        HideStompWarningImmediately();
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
        int phase = bossHealth != null
            ? bossHealth.CurrentPhase
            : 1;

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
            phase >= 4 &&
            Time.time >= nextCastAttackTime &&
            castDistance >= castMinRange &&
            castDistance <= castMaxRange)
        {
            StartBossAttack("Sneer", castAttackLockTime);

            nextCastAttackTime =
                Time.time + castAttackCooldown;

            return true;
        }

        if (phase >= 3 &&
            Time.time >= nextStompAttackTime &&
            IsPlayerInsideStompArea(stompOrigin))
        {
            StartBossAttack(
                "StompAttack",
                stompAttackLockTime
            );

            nextStompAttackTime =
                Time.time + stompAttackCooldown;

            return true;
        }

        if (phase >= 2 &&
            Time.time >= nextHeavyAttackTime &&
            heavyDistance <= heavyAttackWidth)
        {
            StartBossAttack(
                "HeavyAttack",
                heavyAttackLockTime
            );

            nextHeavyAttackTime =
                Time.time + heavyAttackCooldown;

            return true;
        }

        if (Time.time >= nextNormalAttackTime &&
            normalDistance <= normalAttackWidth)
        {
            StartBossAttack(
                "NormalAttack",
                normalAttackLockTime
            );

            nextNormalAttackTime =
                Time.time + normalAttackCooldown;

            return true;
        }

        return false;
    }

    private void StartBossAttack(
        string triggerName,
        float lockTime)
    {
        isAttacking = true;

        bool isStompAttack =
            triggerName == "StompAttack";

        if (isStompAttack)
        {
            currentAttackIsStomp = true;
            stompDamageWasDealt = false;
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
            AttackLockRoutine(lockTime, isStompAttack)
        );
    }

    private IEnumerator AttackLockRoutine(
        float lockTime,
        bool wasStompAttack)
    {
        yield return new WaitForSeconds(lockTime);

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

        bossController.TryDamagePlayerInFront(
            normalOrigin,
            normalAttackWidth,
            normalAttackHeight,
            normalDamage
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
            heavyAttackWidth,
            heavyAttackHeight,
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
        ShowStompWarningWithColor(
            stompWarningPreHitColor
        );
    }

    private void ShowStompWarningImpact()
    {
        ShowStompWarningWithColor(
            stompWarningImpactColor
        );
    }

    private void ShowStompWarningWithColor(Color color)
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

        Vector2 stompOrigin =
            bossController.GetAttackOrigin(
                stompAttackCenter
            );

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

        Vector2 spriteSize =
            stompWarningRenderer.sprite.bounds.size;

        float spriteWidth = Mathf.Max(
            spriteSize.x,
            0.0001f
        );

        float spriteHeight = Mathf.Max(
            spriteSize.y,
            0.0001f
        );

        Vector3 parentScale = Vector3.one;

        if (stompWarningObject.transform.parent != null)
        {
            parentScale =
                stompWarningObject.transform.parent.lossyScale;
        }

        float parentScaleX = Mathf.Max(
            Mathf.Abs(parentScale.x),
            0.0001f
        );

        float parentScaleY = Mathf.Max(
            Mathf.Abs(parentScale.y),
            0.0001f
        );

        stompWarningObject.transform.localScale =
            new Vector3(
                stompAttackWidth / (spriteWidth * parentScaleX),
                stompAttackHeight / (spriteHeight * parentScaleY),
                1f
            );
    }

    public void DealStompDamage()
    {
        stompDamageWasDealt = true;

        ShowStompWarningImpact();

        if (bossController != null)
        {
            Vector2 stompOrigin =
                bossController.GetAttackOrigin(
                    stompAttackCenter
                );

            bossController.TryDamagePlayerInEllipse(
                stompOrigin,
                stompAttackWidth,
                stompAttackHeight,
                stompDamage
            );
        }

        StartStompWarningHideAfterHit();
    }

    public void DealCastDamage()
    {
        if (!enableCastAttack ||
            bossController == null)
        {
            return;
        }

        Vector2 castOrigin =
            bossController.GetAttackOrigin(
                castAttackCenter
            );

        bossController.TryDamagePlayer(
            castOrigin,
            castMaxRange,
            castDamage
        );
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
            stompOrigin;

        float ellipseValue =
            (offset.x * offset.x) /
            (horizontalRadius * horizontalRadius) +
            (offset.y * offset.y) /
            (verticalRadius * verticalRadius);

        return ellipseValue <= 1f;
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
            GetOrCreateStompWarningSprite();

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

        SpriteRenderer bossSpriteRenderer =
            GetComponent<SpriteRenderer>();

        if (bossSpriteRenderer == null)
        {
            return;
        }

        stompWarningRenderer.sortingLayerID =
            bossSpriteRenderer.sortingLayerID;

        stompWarningRenderer.sortingOrder =
            bossSpriteRenderer.sortingOrder +
            stompWarningSortingOrderOffset;
    }

    private static Sprite GetOrCreateStompWarningSprite()
    {
        if (cachedStompWarningSprite != null)
        {
            return cachedStompWarningSprite;
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

        cachedStompWarningSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        cachedStompWarningSprite.name =
            "RuntimeStompWarningSprite";

        return cachedStompWarningSprite;
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

    private void ValidateSettings()
    {
        normalAttackWidth = Mathf.Max(
            0.01f,
            normalAttackWidth
        );

        normalAttackHeight = Mathf.Max(
            0.01f,
            normalAttackHeight
        );

        heavyAttackWidth = Mathf.Max(
            0.01f,
            heavyAttackWidth
        );

        heavyAttackHeight = Mathf.Max(
            0.01f,
            heavyAttackHeight
        );

        stompAttackWidth = Mathf.Max(
            0.01f,
            stompAttackWidth
        );

        stompAttackHeight = Mathf.Max(
            0.01f,
            stompAttackHeight
        );

        stompWarningAfterHitDuration =
            Mathf.Max(0f, stompWarningAfterHitDuration);

        castMinRange = Mathf.Max(
            0f,
            castMinRange
        );

        castMaxRange = Mathf.Max(
            castMinRange,
            castMaxRange
        );
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

        Vector3 normalOrigin =
            controller.GetAttackOrigin(
                normalAttackCenter
            );

        Vector3 heavyOrigin =
            controller.GetAttackOrigin(
                heavyAttackCenter
            );

        Vector3 stompOrigin =
            controller.GetAttackOrigin(
                stompAttackCenter
            );

        Vector3 castOrigin =
            controller.GetAttackOrigin(
                castAttackCenter
            );

        Vector3 castSpawnOrigin =
            controller.GetAttackOrigin(
                castProjectileSpawnPoint
            );

        Vector3 normalAttackBoxCenter =
            normalOrigin +
            Vector3.right * direction *
            (normalAttackWidth * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            normalAttackBoxCenter,
            new Vector3(
                normalAttackWidth,
                normalAttackHeight,
                0.1f
            )
        );

        Vector3 heavyAttackBoxCenter =
            heavyOrigin +
            Vector3.right * direction *
            (heavyAttackWidth * 0.5f);

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
            stompOrigin,
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
        Gizmos.DrawWireSphere(
            castOrigin,
            castMaxRange
        );

        Gizmos.DrawWireSphere(
            castOrigin,
            castMinRange
        );

        Gizmos.DrawWireSphere(
            castSpawnOrigin,
            0.08f
        );
    }
}