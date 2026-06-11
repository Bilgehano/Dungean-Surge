using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EnemyWaveEntry
{
    [Tooltip("Drag from the PROJECT window (Assets/Prefabs), NOT from the Hierarchy.")]
    public GameObject prefab;
    public int count = 4;
    public float timeBetweenSpawns = 0.4f;
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public EnemyWaveEntry[] enemyEntries;
}

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Points (shared)")]
    public Transform[] spawnPoints;

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();
    
    [Header("Settings")]
    public float timeBetweenWaves = 3f;
    public float countdownDuration = 3f;

    [Header("Events")]
    public UnityEvent onAllWavesComplete;

    [Header("State (Read Only)")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int enemiesAlive;
    [SerializeField] private bool waveActive;
    [SerializeField] private bool isWaitingForNextWave;
    [SerializeField] private float currentCountdown;
    [SerializeField] private int enemiesToSpawn;

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => waves.Count;
    public int EnemiesAlive => enemiesAlive;
    public bool WaveActive => waveActive;
    public bool IsWaiting => isWaitingForNextWave;
    public float Countdown => currentCountdown;

    private void Awake()
    {
    ResetWaveState();
    }

    private void ResetWaveState()
    {  
        currentWaveIndex = 0;
        enemiesAlive = 0;
        waveActive = false;
        isWaitingForNextWave = false;
        currentCountdown = 0f;
        enemiesToSpawn = 0;
    }

    void Start()
    {
        ResetWaveState();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
        Debug.LogError("WaveManager: No spawn points assigned!", this);
        return;
        }

        if (waves == null || waves.Count == 0)
        {
        Debug.LogError("WaveManager: No waves defined!", this);
        return;
        }

        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        // Initial Countdown for Wave 1
        isWaitingForNextWave = true;
        float initTimer = countdownDuration;
        while (initTimer > 0)
        {
            currentCountdown = initTimer;
            initTimer -= Time.deltaTime;
            yield return null;
        }
        currentCountdown = 0;
        isWaitingForNextWave = false;

        while (currentWaveIndex < waves.Count)
        {
            // Start Wave
            waveActive = true;
            isWaitingForNextWave = false;
            enemiesAlive = 0;
            enemiesToSpawn = 0;

            Wave currentWave = waves[currentWaveIndex];
            foreach (var entry in currentWave.enemyEntries)
            {
                enemiesToSpawn += entry.count;
            }

            foreach (var entry in currentWave.enemyEntries)
            {
                if (entry.prefab != null)
                {
                    StartCoroutine(SpawnEnemyType(entry));
                }
            }

            // Wait until all enemies are spawned AND all enemies are dead
            while (enemiesToSpawn > 0 || enemiesAlive > 0)
            {
                yield return null;
            }

            waveActive = false;
            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
            {
                // Wave Completed Message Period
                isWaitingForNextWave = true;
                currentCountdown = -1; // Flag for "Wave Completed"
                yield return new WaitForSeconds(timeBetweenWaves);

                // Countdown Period
                float timer = countdownDuration;
                while (timer > 0)
                {
                    currentCountdown = timer;
                    timer -= Time.deltaTime;
                    yield return null;
                }
                currentCountdown = 0;
            }
        }

        OnAllWavesComplete();
    }

    private IEnumerator SpawnEnemyType(EnemyWaveEntry entry)
    {
        if (entry.prefab == null)
        {
            enemiesToSpawn -= entry.count;
            yield break;
        }

        for (int i = 0; i < entry.count; i++)
        {
            bool spawned = SpawnEnemy(entry.prefab);

            if (!spawned)
            {
                Debug.LogWarning("WaveManager: Enemy could not be spawned.", this);
            }

        enemiesToSpawn--;
        yield return new WaitForSeconds(entry.timeBetweenSpawns);
        }
    }

    private bool SpawnEnemy(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("WaveManager: Tried to spawn a missing prefab.", this);
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("WaveManager: Cannot spawn enemy because no spawn points are assigned.", this);
            return false;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        Enemy_Health health = enemy.GetComponent<Enemy_Health>();
        if (health != null)
        {
            health.waveManager = this;
        }
        else
        {
            Debug.LogWarning("WaveManager: Spawned enemy has no Enemy_Health script.", enemy);
        }

        enemiesAlive++;
        return true;
    }

    public void OnEnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("WaveManager: All waves completed! Triggering end function.");
        onAllWavesComplete?.Invoke();
    }
    }
