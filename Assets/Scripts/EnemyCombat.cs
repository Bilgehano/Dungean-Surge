using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public int damageAmount = -1;
    public Transform AttackPoint;
    public float attackRange = 0.5f;
    public LayerMask playerLayer;
    public float knockbackForce = 8f;
    public float stunTime = 0.35f;
    public AudioClip attackSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Call this from the attack animation event at the exact hit frame.
    public void DealDamageAtAttackFrame()
    {
        if (AttackPoint == null)
        {
            Debug.LogError("EnemyCombat: AttackPoint is not assigned.", this);
            return;
        }

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Debug.Log("EnemyCombat: Attack frame event fired.", this);

        Collider2D[] hits = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, playerLayer);

        if (hits.Length == 0)
        {
            Debug.Log("EnemyCombat: Attack frame hit nothing.", this);
            return;
        }

        foreach (var hit in hits)
        {
            var playerHealth = hit.GetComponentInParent<PlayerHealth>();
            var playerMovement = hit.GetComponentInParent<PlayerMovement>();

            if (playerMovement != null && playerMovement.IsHitImmune)
            {
                continue;
            }

            if (playerMovement != null)
            {
                playerMovement.ApplyKnockback(transform.position, stunTime, knockbackForce);
            }

            if (playerHealth != null)
            {
                Debug.Log("EnemyCombat: Damaged player on attack frame.", this);
                playerHealth.ChangeHealth(damageAmount);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (AttackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AttackPoint.position, attackRange);
    }
}
