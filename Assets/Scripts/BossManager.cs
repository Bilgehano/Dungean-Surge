using UnityEngine;
using UnityEngine.Events;

public class BossManager : MonoBehaviour
{
    [Header("Boss Spawn")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Events")]
    public UnityEvent onBossFightStarted;
    public UnityEvent onBossDefeated;

    [Header("State (Read Only)")]
    [SerializeField] private GameObject currentBoss;
    [SerializeField] private bool bossFightActive;
    [SerializeField] private bool bossDefeated;

    [Header("Boss UI")]
    [SerializeField] private BossHealthBar bossHealthBar;

    public bool BossFightActive => bossFightActive;
    public bool BossDefeated => bossDefeated;

    public void StartBossFight()
    {
        if (bossFightActive || bossDefeated)
        {
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("BossManager: No boss prefab assigned!", this);
            return;
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError("BossManager: No boss spawn point assigned!", this);
            return;
        }

        currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

        BossHealth bossHealth = currentBoss.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.SetBossManager(this);
            bossHealth.SetBossHealthBar(bossHealthBar);
        }
        else
        {
            Debug.LogWarning("BossManager: Spawned boss has no BossHealth script.", currentBoss);
        }

        BossController bossController = currentBoss.GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.ActivateBoss();
        }

        bossFightActive = true;

        Debug.Log("BossManager: Boss fight started.");

        onBossFightStarted?.Invoke();
    }

    public void OnBossDied()
    {
        if (bossDefeated)
        {
            return;
        }

        bossFightActive = false;
        bossDefeated = true;
        currentBoss = null;

        Debug.Log("BossManager: Boss defeated.");

        onBossDefeated?.Invoke();
    }
}