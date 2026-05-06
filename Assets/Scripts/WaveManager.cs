using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class EnemyWaveEntry
{
    [Tooltip("Drag from the PROJECT window (Assets/Prefabs), NOT from the Hierarchy.")]
    public GameObject prefab;
    public int count = 4;
    public float timeBetweenSpawns = 0.4f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Points (shared)")]
    public Transform[] spawnPoints;

    [Header("Enemy Types")]
    public EnemyWaveEntry[] enemyEntries;

    [Header("State (Read Only)")]
    [SerializeField] private int enemiesAlive;
    [SerializeField] private bool waveActive;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (enemyEntries == null) return;
        for (int i = 0; i < enemyEntries.Length; i++)
        {
            GameObject p = enemyEntries[i].prefab;
            if (p != null && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(p))
            {
                Debug.LogWarning($"WaveManager: Enemy Entries[{i}] '{p.name}' is a scene object, not a prefab asset. " +
                    "Drag it from the Project window (Assets/Prefabs) instead.", this);
            }
        }
    }
#endif

    void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        if (enemyEntries == null || enemyEntries.Length == 0)
        {
            Debug.LogError("WaveManager: no enemy entries assigned.", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("WaveManager: no spawn points assigned.", this);
            return;
        }

        enemiesAlive = 0;
        waveActive = true;

        for (int i = 0; i < enemyEntries.Length; i++)
        {
            if (enemyEntries[i].prefab != null)
            {
                StartCoroutine(SpawnEnemyType(enemyEntries[i]));
            }
        }
    }

    private IEnumerator SpawnEnemyType(EnemyWaveEntry entry)
    {
        for (int i = 0; i < entry.count; i++)
        {
            if (entry.prefab == null)
            {
                Debug.LogError("WaveManager: prefab is null or was destroyed. Assign a Project asset prefab, not a scene object.", this);
                yield break;
            }
            SpawnEnemy(entry.prefab);
            yield return new WaitForSeconds(entry.timeBetweenSpawns);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        Enemy_Health health = enemy.GetComponent<Enemy_Health>();
        if (health != null)
        {
            health.waveManager = this;
        }

        enemiesAlive++;
    }

    // Called by Enemy_Health when an enemy dies.
    public void OnEnemyDied()
    {
        enemiesAlive--;

        if (enemiesAlive <= 0 && waveActive)
        {
            waveActive = false;
            OnWaveComplete();
        }
    }

    private void OnWaveComplete()
    {
        Debug.LogWarning("WaveManager: Wave complete! All enemies defeated.");
        // TODO: trigger next wave, show UI, unlock door, load next scene, etc.
    }
}
