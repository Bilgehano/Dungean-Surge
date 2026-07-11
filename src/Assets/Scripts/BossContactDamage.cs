using UnityEngine;

public class BossContactDamage : MonoBehaviour
{
    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = -1;
    [SerializeField] private float contactWidth = 2f;
    [SerializeField] private float contactHeight = 2f;
    [SerializeField] private float damageCooldown = 0.8f;
    [SerializeField] private LayerMask playerLayer;

    [Header("References")]
    [SerializeField] private Transform damageCenter;

    [SerializeField, HideInInspector]
    private float contactRadius = 1f;

    [SerializeField, HideInInspector]
    private bool hasMigratedContactArea;

    private float nextDamageTime;

    private void Awake()
    {
        MigrateLegacyContactAreaIfNeeded();

        if (damageCenter == null)
        {
            damageCenter = transform;
        }
    }

    private void OnValidate()
    {
        MigrateLegacyContactAreaIfNeeded();
    }

    private void Update()
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        Vector2 center = damageCenter != null
            ? damageCenter.position
            : transform.position;

        Collider2D[] playerHits = Physics2D.OverlapBoxAll(
            center,
            new Vector2(contactWidth, contactHeight),
            0f,
            playerLayer
        );

        foreach (Collider2D playerHit in playerHits)
        {
            PlayerHealth playerHealth =
                playerHit.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null)
            {
                continue;
            }

            if (!IsPlayerInsideContactEllipse(
                    center,
                    playerHealth.transform.position))
            {
                continue;
            }

            playerHealth.ChangeHealth(contactDamage);
            nextDamageTime = Time.time + damageCooldown;
            return;
        }
    }

    private bool IsPlayerInsideContactEllipse(
        Vector2 center,
        Vector2 playerPosition)
    {
        float horizontalRadius = Mathf.Max(
            contactWidth * 0.5f,
            0.01f
        );

        float verticalRadius = Mathf.Max(
            contactHeight * 0.5f,
            0.01f
        );

        Vector2 offset = playerPosition - center;

        float ellipseValue =
            (offset.x * offset.x) /
            (horizontalRadius * horizontalRadius) +
            (offset.y * offset.y) /
            (verticalRadius * verticalRadius);

        return ellipseValue <= 1f;
    }

    private void MigrateLegacyContactAreaIfNeeded()
    {
        if (hasMigratedContactArea)
        {
            return;
        }

        float oldRadius = Mathf.Max(contactRadius, 0.01f);
        float oldDiameter = oldRadius * 2f;

        contactWidth = oldDiameter;
        contactHeight = oldDiameter;

        hasMigratedContactArea = true;
    }

    private void OnDrawGizmosSelected()
    {
        Transform centerTransform = damageCenter != null
            ? damageCenter
            : transform;

        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.color = new Color(1f, 0.45f, 0f, 1f);
        Gizmos.matrix = Matrix4x4.TRS(
            centerTransform.position,
            Quaternion.identity,
            new Vector3(
                contactWidth,
                contactHeight,
                0.1f
            )
        );

        Gizmos.DrawWireSphere(Vector3.zero, 0.5f);

        Gizmos.matrix = previousMatrix;
    }
}