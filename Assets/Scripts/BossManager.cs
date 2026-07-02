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
    [SerializeField] private float bossDefeatScreenTime = 2f;
    [SerializeField] private Color bossDefeatOverlayColor = new Color(0.35f, 0f, 0f, 0.8f);

    [Header("Boss Spawn Effects")]
    [SerializeField] private AudioClip bossSpawnSfx;
    [SerializeField] private float bossSpawnBlinkInterval = 0.08f;
    [SerializeField, Range(0f, 1f)] private float bossSpawnDelayRatio = 0.3f;

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
    private Rigidbody2D playerRb;

    private BossController spawnedBossController;
    private Coroutine bossSpawnBlinkRoutine;
    private bool bossIntroInProgress;
    private bool bossDefeatSequenceInProgress;

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
            playerRb = player.GetComponent<Rigidbody2D>();

            Debug.Log("BossManager: Player found: " + player.name);
        }
        else
        {
            Debug.LogWarning("BossManager: Player was not found. Make sure your player has the tag 'Player'.");
        }

        if (cameraFollow == null && Camera.main != null)
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
            Debug.Log("BossManager: Boss fight already active or boss already defeated.");
            return;
        }

        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogError("BossManager: Boss Prefab or Boss Spawn Point is missing!");
            return;
        }

        StartCoroutine(BossSpawnCutscene());
    }

    private IEnumerator BossSpawnCutscene()
    {
        bossFightActive = true;
        bossIntroInProgress = true;

        Debug.Log("BossManager: Boss fight started. Trying to start boss music.");

        // Boss music starts directly through AudioManager.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
            Debug.Log("BossManager: AudioManager.PlayBossMusic() was called.");
        }
        else
        {
            Debug.LogError("BossManager: AudioManager.Instance is NULL. Boss music cannot play.");
        }

        // Keep this event for other things if you need it.
        // But boss music does not depend on this anymore.
        onBossFightStarted?.Invoke();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        Transform originalTarget = null;

        if (cameraFollow != null)
        {
            originalTarget = cameraFollow.target;
            cameraFollow.target = bossSpawnPoint;
        }

        if (cutsceneOverlay != null)
        {
            cutsceneOverlay.gameObject.SetActive(true);

            float spawnDelayTime = bossIntroTime * Mathf.Clamp01(bossSpawnDelayRatio);
            float blinkTimer = 0f;

            while (blinkTimer < bossIntroTime)
            {
                cutsceneOverlay.color =
                    Mathf.FloorToInt(Time.time * 10) % 2 == 0
                        ? blinkColor
                        : Color.clear;

                if (currentBoss == null && blinkTimer >= spawnDelayTime)
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
            float spawnDelayTime = bossIntroTime * Mathf.Clamp01(bossSpawnDelayRatio);

            if (spawnDelayTime > 0f)
            {
                yield return new WaitForSeconds(spawnDelayTime);
            }

            SpawnBoss();

            float remainingIntroTime = Mathf.Max(0f, bossIntroTime - spawnDelayTime);

            if (remainingIntroTime > 0f)
            {
                yield return new WaitForSeconds(remainingIntroTime);
            }
        }

        if (currentBoss == null)
        {
            Debug.LogError("BossManager: Boss could not be spawned. Ending cutscene and resetting state.");

            if (cameraFollow != null)
            {
                cameraFollow.target = player != null ? player.transform : null;
            }

            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }

            if (playerCombat != null)
            {
                playerCombat.enabled = true;
            }

            bossIntroInProgress = false;
            bossFightActive = false;

            yield break;
        }

        if (cameraFollow != null)
        {
            cameraFollow.target = originalTarget;
        }

        yield return new WaitForSeconds(cameraTravelTime);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = true;
        }

        bossIntroInProgress = false;

        if (spawnedBossController != null)
        {
            spawnedBossController.ActivateBoss();
        }

        Debug.Log("BossManager: Boss intro cutscene finished.");
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
        else
        {
            Debug.LogWarning("BossManager: Spawned boss has no BossHealth component.");
        }

        BossController bossController = currentBoss.GetComponent<BossController>();

        if (bossController != null)
        {
            spawnedBossController = bossController;

            // Boss stays frozen during intro.
            // It becomes active after the cutscene.
            bossController.DeactivateBoss();
        }
        else
        {
            Debug.LogWarning("BossManager: Spawned boss has no BossController component.");
        }

        PlayBossSpawnSfx();
        StartBossSpawnBlink();

        Debug.Log("BossManager: Boss spawned at " + bossSpawnPoint.position);
    }

    private void PlayBossSpawnSfx()
    {
        if (bossSpawnSfx == null)
        {
            Debug.Log("BossManager: No boss spawn SFX assigned.");
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(bossSpawnSfx);
            Debug.Log("BossManager: Boss spawn SFX played.");
        }
        else
        {
            Debug.LogWarning("BossManager: AudioManager.Instance is null, boss spawn SFX could not be played.");
        }
    }

    private void StartBossSpawnBlink()
    {
        if (bossSpawnBlinkRoutine != null)
        {
            StopCoroutine(bossSpawnBlinkRoutine);
        }

        if (currentBoss == null)
        {
            return;
        }

        bossSpawnBlinkRoutine = StartCoroutine(BossSpawnBlinkRoutine(currentBoss));
    }

    private IEnumerator BossSpawnBlinkRoutine(GameObject boss)
    {
        if (boss == null)
        {
            yield break;
        }

        SpriteRenderer bossRenderer = boss.GetComponent<SpriteRenderer>();

        if (bossRenderer == null)
        {
            yield break;
        }

        float interval = Mathf.Max(0.02f, bossSpawnBlinkInterval);
        WaitForSeconds wait = new WaitForSeconds(interval);
        bool visible = true;

        while (bossIntroInProgress && boss != null)
        {
            visible = !visible;
            bossRenderer.enabled = visible;

            yield return wait;
        }

        if (bossRenderer != null)
        {
            bossRenderer.enabled = true;
        }

        bossSpawnBlinkRoutine = null;
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

    public void OnBossDied(GameObject defeatedBoss = null)
    {
        if (bossDefeated || bossDefeatSequenceInProgress)
        {
            return;
        }

        StartCoroutine(BossDefeatCutscene(defeatedBoss));
    }

    private IEnumerator BossDefeatCutscene(GameObject defeatedBoss)
    {
        bossFightActive = false;
        bossIntroInProgress = false;
        bossDefeated = true;
        bossDefeatSequenceInProgress = true;

        GameObject bossObject = defeatedBoss != null
            ? defeatedBoss
            : currentBoss;

        Transform originalTarget = null;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        if (cameraFollow != null)
        {
            originalTarget = cameraFollow.target;

            if (bossObject != null)
            {
                cameraFollow.target = bossObject.transform;
            }
        }

        float cameraFocusTime = Mathf.Max(0f, cameraTravelTime);
        if (cameraFocusTime > 0f)
        {
            yield return new WaitForSeconds(cameraFocusTime);
        }

        if (cutsceneOverlay != null)
        {
            cutsceneOverlay.gameObject.SetActive(true);
            cutsceneOverlay.color = bossDefeatOverlayColor;
        }

        float deathScreenTime = Mathf.Max(0f, bossDefeatScreenTime);
        if (deathScreenTime > 0f)
        {
            yield return new WaitForSeconds(deathScreenTime);
        }

        if (cutsceneOverlay != null)
        {
            cutsceneOverlay.gameObject.SetActive(false);
        }

        currentBoss = null;
        spawnedBossController = null;
        bossDefeatSequenceInProgress = false;

        Debug.Log("BossManager: Boss defeated.");

        onBossDefeated?.Invoke();

        if (cameraFollow != null && cameraFollow.target == null)
        {
            cameraFollow.target = originalTarget;
        }

        if (bossObject != null)
        {
            Destroy(bossObject);
        }
    }
}