using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    // Assigned automatically by WaveManager when this enemy is spawned.
    [HideInInspector] public WaveManager waveManager;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Notify the wave manager before deactivating.
        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
        }

        gameObject.SetActive(false);
    }
}