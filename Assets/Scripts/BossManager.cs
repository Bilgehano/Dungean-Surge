using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BossManager : MonoBehaviour
{
    [Header("Boss Spawn")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Cutscene Settings")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private UnityEngine.UI.Image cutsceneOverlay;
    [SerializeField] private float cameraTravelTime = 1.5f;
    [SerializeField] private float bossIntroTime = 2f;
    [SerializeField] private Color blinkColor = new Color(0, 0, 0, 0.8f);

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

    private GameObject player;
    private PlayerMovement playerMovement;
    private Player_Combat playerCombat;

    private void Awake()
    {
        ClearInvalidCurrentBossReference();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerCombat = player.GetComponent<Player_Combat>();
        }

        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }

        if (cutsceneOverlay != null)
        {
            cutsceneOverlay.gameObject.SetActive(false);
        }
    }

    public void StartBossFight()
    {
        ClearInvalidCurrentBossReference();

        if (bossFightActive || bossDefeated)
        {
            return;
        }

        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogError("BossManager: Prefab or Spawn Point missing!");
            return;
        }

        StartCoroutine(BossSpawnCutscene());
    }

    private IEnumerator BossSpawnCutscene()
    {
        bossFightActive = true;

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        Transform originalTarget = null;
        if (cameraFollow != null)
        {
            originalTarget = cameraFollow.target;
            cameraFollow.target = bossSpawnPoint;
        }

        yield return new WaitForSeconds(cameraTravelTime);

        if (cutsceneOverlay != null)
        {
            cutsceneOverlay.gameObject.SetActive(true);
            float blinkTimer = 0;
            while (blinkTimer < bossIntroTime)
            {
                cutsceneOverlay.color = (Mathf.FloorToInt(Time.time * 10) % 2 == 0) ? blinkColor : Color.clear;
                
                if (currentBoss == null && blinkTimer > bossIntroTime * 0.3f)
                {
                    SpawnBoss();
                }

                blinkTimer += Time.deltaTime;
                yield return null;
            }

            if (currentBoss == null)
            {
                Debug.LogWarning("BossManager: Mid-cutscene spawn was missed, forcing spawn now.");
                SpawnBoss();
            }

            cutsceneOverlay.gameObject.SetActive(false);
        }
        else
        {
            SpawnBoss();
            yield return new WaitForSeconds(bossIntroTime);
        }

        if (currentBoss == null)
        {
            Debug.LogError("BossManager: Boss could not be spawned. Ending cutscene and resetting state.");

            if (cameraFollow != null)
            {
                cameraFollow.target = player != null ? player.transform : null;
            }

            if (playerMovement != null) playerMovement.enabled = true;
            if (playerCombat != null) playerCombat.enabled = true;

            bossFightActive = false;
            yield break;
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = originalTarget;
        }

        yield return new WaitForSeconds(cameraTravelTime);

        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;

        Debug.Log("BossManager: Boss fight started.");
        onBossFightStarted?.Invoke();
    }

    private void SpawnBoss()
    {
        ClearInvalidCurrentBossReference();

        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogError("BossManager: Cannot spawn boss because prefab or spawn point is missing.");
            return;
        }

        if (currentBoss != null)
        {
            return;
        }

        currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

        if (currentBoss == null)
        {
            Debug.LogError("BossManager: Instantiate returned null.");
            return;
        }

        if (!currentBoss.activeSelf)
        {
            currentBoss.SetActive(true);
        }

        BossHealth bossHealth = currentBoss.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.SetBossManager(this);
            bossHealth.SetBossHealthBar(bossHealthBar);
        }

        BossController bossController = currentBoss.GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.ActivateBoss();
        }

        Debug.Log("BossManager: Boss spawned at " + bossSpawnPoint.position);
    }

    private void ClearInvalidCurrentBossReference()
    {
        if (currentBoss == null)
        {
            return;
        }

        bool isSceneObject = currentBoss.scene.IsValid();

        if (!isSceneObject)
        {
            Debug.LogWarning("BossManager: currentBoss had a prefab/asset reference. Clearing it so boss can spawn.");
            currentBoss = null;
            return;
        }

        if (currentBoss == bossPrefab)
        {
            Debug.LogWarning("BossManager: currentBoss referenced bossPrefab directly. Clearing invalid runtime state.");
            currentBoss = null;
        }
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

        GameObject bgMusic = GameObject.Find("Background Music Ingame");
        if (bgMusic != null)
        {
            Destroy(bgMusic);
        }

        Debug.Log("BossManager: Boss defeated.");

        onBossDefeated?.Invoke();
    }
}