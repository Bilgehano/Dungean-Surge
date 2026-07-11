using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    private bool isDead;
    [SerializeField] private EnemyResources enemyResources;

    // Assigned automatically by WaveManager when this enemy is spawned.
    [HideInInspector] public WaveManager waveManager;

    void Awake()
    {
        // Allow manual assignment, then fall back to hierarchy lookup.
        if (enemyResources == null)
        {
            enemyResources = GetComponent<EnemyResources>();
        }

        if (enemyResources == null)
        {
            enemyResources = GetComponentInParent<EnemyResources>();
        }

        if (enemyResources == null)
        {
            enemyResources = GetComponentInChildren<EnemyResources>(true);
        }
    }

    void Start()
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

        // Trigger hit reaction if damage is taken
        if (amount < 0)
        {
            Enemy_gethit getHit = GetComponent<Enemy_gethit>();
            if (getHit != null)
            {
                getHit.TriggerHit();
            }
        }

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

        Debug.Log("Enemy_Health: Die called for " + gameObject.name + ".", this);

        if (enemyResources == null)
        {
            enemyResources = GetComponent<EnemyResources>();
            if (enemyResources == null)
            {
                enemyResources = GetComponentInParent<EnemyResources>();
            }
            if (enemyResources == null)
            {
                enemyResources = GetComponentInChildren<EnemyResources>(true);
            }
        }

        if (enemyResources != null)
        {
            enemyResources.DropAll();
        }
        else
        {
            Debug.LogWarning("Enemy_Health: EnemyResources was not found in this enemy hierarchy, so no drops were spawned.", this);
        }

        // Notify the wave manager before deactivating.
        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
        }

        gameObject.SetActive(false);
    }
}