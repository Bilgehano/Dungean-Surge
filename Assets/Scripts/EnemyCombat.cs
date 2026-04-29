using JetBrains.Annotations;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public int damageAmount = -10;

    void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<PlayerHealth>().ChangeHealth(damageAmount);
    }
}
