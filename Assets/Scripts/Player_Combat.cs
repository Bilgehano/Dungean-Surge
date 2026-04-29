using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Animator anim;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
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

    // Call this from the attack animation event on the hit frame.
    public void DealDamageAtAttackFrame()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("Player_Combat: attackPoint is not assigned.", this);
            return;
        }

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy_Health enemyHealth = enemies[i].GetComponentInParent<Enemy_Health>();
            if (enemyHealth != null)
            {
                enemyHealth.ChangeHealth(damageAmount);
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
