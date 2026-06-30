using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DebugManager : MonoBehaviour
{
    [Header("Debug Scene Routing")]
    [SerializeField] private string level1SceneName = "Level1";
    [SerializeField] private string level2SceneName = "Level2";

    private bool showMenu = false;
    private bool godMode = false;
    private static bool pendingBossTriggerAfterSceneLoad;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Press Backquote/Tilde (`) to toggle debug menu
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            showMenu = !showMenu;
        }

        if (godMode)
        {
            var ph = Object.FindAnyObjectByType<PlayerHealth>();
            if (ph != null && ph.currentHealth < ph.maxHealth) ph.ChangeHealth(10);
        }
    }

    private void OnGUI()
    {
        int menuWidth = 500;
        int menuHeight = 940;

        int x = (Screen.width - menuWidth) / 2;
        int y = (Screen.height - menuHeight) / 2;

        // Bigger text styles
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 32;
        boxStyle.alignment = TextAnchor.UpperCenter;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 28;

        GUIStyle toggleStyle = new GUIStyle(GUI.skin.toggle);
        toggleStyle.fontSize = 26;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 24;

        if (!showMenu)
        {
            GUI.Label(
                new Rect(20, 20, 500, 40),
                "Press Backspace for Debug Menu",
                labelStyle
            );
            return;
        }

        GUI.Box(
            new Rect(x, y, menuWidth, menuHeight),
            "DEBUG MENU",
            boxStyle
        );

        if (GUI.Button(new Rect(x + 50, y + 90, 400, 70), "Kill All Enemies", buttonStyle))
        {
            KillAllEnemies();
        }

        if (GUI.Button(new Rect(x + 50, y + 180, 400, 70), "Trigger Boss Fight", buttonStyle))
        {
            TriggerBoss();
        }

        if (GUI.Button(new Rect(x + 50, y + 270, 400, 70), "Level Up (+50 Gold)", buttonStyle))
        {
            LevelUp();
        }

        if (GUI.Button(new Rect(x + 50, y + 360, 400, 70), "Go To Level 1", buttonStyle))
        {
            GoToLevel1();
        }

        if (GUI.Button(new Rect(x + 50, y + 450, 400, 70), "Go To Level 2", buttonStyle))
        {
            GoToLevel2();
        }

        if (GUI.Button(new Rect(x + 50, y + 540, 400, 70), "Full Heal Player", buttonStyle))
        {
            HealPlayer();
        }

        if (GUI.Button(new Rect(x + 50, y + 630, 400, 70), "Kill Player", buttonStyle))
        {
            KillPlayer();
        }

        if (GUI.Button(new Rect(x + 50, y + 720, 400, 70), "Kill Boss", buttonStyle))
        {
            KillBoss();
        }

        godMode = GUI.Toggle(
            new Rect(x + 50, y + 810, 400, 50),
            godMode,
            " God Mode",
            toggleStyle
        );

        if (GUI.Button(new Rect(x + 50, y + 870, 400, 50), "Close Menu", buttonStyle))
        {
            showMenu = false;
        }
    }
    private void KillAllEnemies()
    {
        // Finds all active enemies and kills them to progress wave
        Enemy_Health[] enemies = Object.FindObjectsByType<Enemy_Health>();
        foreach (var enemy in enemies)
        {
            enemy.ChangeHealth(-9999);
        }
    }

    private void TriggerBoss()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        bool isBossLevelScene = IsBossLevelScene(activeScene);

        if (!isBossLevelScene)
        {
            pendingBossTriggerAfterSceneLoad = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(level1SceneName);
            return;
        }

        StartCoroutine(StopWavesAndStartBossRoutine());
    }

    private void GoToLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(level1SceneName);
    }

    private void GoToLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(level2SceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!pendingBossTriggerAfterSceneLoad)
        {
            return;
        }

        if (!IsBossLevelScene(scene.name))
        {
            return;
        }

        pendingBossTriggerAfterSceneLoad = false;
        StartCoroutine(StopWavesAndStartBossRoutine());
    }

    private IEnumerator StopWavesAndStartBossRoutine()
    {
        yield return null;

        WaveManager waveManager = Object.FindAnyObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.StopWaveSpawning(true);
        }

        yield return null;

        BossManager bossManager = Object.FindAnyObjectByType<BossManager>();
        if (bossManager != null)
        {
            bossManager.StartBossFight();
        }
        else
        {
            Debug.LogWarning("DebugManager: BossManager not found in scene.");
        }
    }

    private bool IsBossLevelScene(string sceneName)
    {
        return string.Equals(sceneName, level1SceneName, System.StringComparison.Ordinal) ||
               string.Equals(sceneName, level2SceneName, System.StringComparison.Ordinal);
    }

    private void LevelUp()
    {
        PlayerResources pr = Object.FindAnyObjectByType<PlayerResources>();
        if (pr != null)
        {
            pr.AddGold(50); // This will trigger your level up logic
        }
    }

    private void HealPlayer()
    {
        PlayerHealth ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.ChangeHealth(999);
        }
    }

    private void KillPlayer()
    {
        PlayerHealth ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.Die();
        }
        else
        {
            Debug.LogWarning("DebugManager: PlayerHealth not found.");
        }
    }

    private void KillBoss()
    {
        BossHealth bossHealth = Object.FindAnyObjectByType<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.ChangeHealth(-999999);
        }
        else
        {
            Debug.LogWarning("DebugManager: BossHealth not found.");
        }
    }
}