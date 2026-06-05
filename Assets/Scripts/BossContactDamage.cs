using UnityEngine;

public class BossContactDamage : MonoBehaviour
{
    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = -1;
    [SerializeField] private float damageCooldown = 0.8f;
    [SerializeField] private float contactRadius = 1.0f;
    [SerializeField] private LayerMask playerLayer;

    [Header("References")]
    [SerializeField] private Transform damageCenter;

    private float nextDamageTime;

    private void Awake()
    {
        if (damageCenter == null)
        {
            damageCenter = transform;
        }
    }

    private void Update()
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        Collider2D playerHit = Physics2D.OverlapCircle(
            damageCenter.position,
            contactRadius,
            playerLayer
        );

        if (playerHit == null)
        {
            return;
        }

        PlayerHealth playerHealth = playerHit.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(contactDamage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform center = damageCenter != null ? damageCenter : transform;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, contactRadius);
    }
}