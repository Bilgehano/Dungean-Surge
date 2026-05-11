using UnityEngine;

public class Boss_Health : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;
    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void ChangeHealth(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Boss died");

        Destroy(gameObject);
    }
}