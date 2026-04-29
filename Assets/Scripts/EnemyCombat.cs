using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public int damageAmount = -10;

    void OnCollisionEnter2D(Collision2D collision)
    {
        var playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(damageAmount);
        }
    }
}
