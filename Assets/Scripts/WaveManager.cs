using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int waveEnemyCount = 8;
    public float timeBetweenSpawns = 0.4f;

    [Header("State (Read Only)")] 
    [SerializeField] private int enemiesAlive;
    [SerializeField] private bool waveActive;

    void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveManager: enemyPrefab is not assigned.", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("WaveManager: no spawn points assigned.", this);
            return;
        }

        enemiesAlive = 0;
        waveActive = true;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < waveEnemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private void SpawnEnemy()
    {
        // Pick a random spawn point.
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // Register this manager on the enemy so it can call OnEnemyDied.
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
        Debug.Log("WaveManager: Wave complete!");
        // TODO: trigger next wave, show UI, unlock door, load next scene, etc.
    }
}
