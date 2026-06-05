using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Animator anim;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public LayerMask bossLayer;
    public int damageAmount = -1;
    public float timeBetweenAttacks = 1f;

    private float nextAttackTime;

    public void Attack()
    {
        if (!CanStartAttack())
        {
            return;
        }

        nextAttackTime = Time.time + timeBetweenAttacks;
        anim.SetBool("isAttacking", true);
    }

    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Call this from the attack animation event on the hit frame.
    public void DealDamageAtAttackFrame()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("Player_Combat: attackPoint is not assigned.", this);
            return;
        }

        // Play swing/miss sound every time at 30% volume
        if (missSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(missSound, 0.3f);
        }

        int attackHitLayers = enemyLayer.value | bossLayer.value;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, attackHitLayers);

        if (enemies.Length > 0)
        {
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound, 0.5f);
            }

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, attackPoint.position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
            }
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy_Health enemyHealth = enemies[i].GetComponentInParent<Enemy_Health>();
            if (enemyHealth != null)
            {
                enemyHealth.ChangeHealth(damageAmount);
            }

            BossHealth bossHealth = enemies[i].GetComponentInParent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.ChangeHealth(damageAmount);
            }
        }
    }

    private bool CanStartAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return false;
        }

        if (anim == null)
        {
            Debug.LogWarning("Player_Combat: anim is not assigned.", this);
            return false;
        }

        return true;
    }


    public void FinishAttack()
    {
        anim.SetBool("isAttacking", false);
    }

}
