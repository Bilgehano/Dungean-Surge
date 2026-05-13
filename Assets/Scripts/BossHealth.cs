using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 500;

    [Header("State (Read Only)")]
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead;

    [HideInInspector] public BossManager bossManager;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log("Boss died.");

        if (bossManager != null)
        {
            bossManager.OnBossDied();
        }

        Destroy(gameObject);
    }
}