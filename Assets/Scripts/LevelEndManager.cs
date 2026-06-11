using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Level2";

    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;

    [Tooltip("Level complete ekranı açılınca kapatılacak UI objeleri.")]
    [SerializeField] private GameObject[] uiObjectsToHide;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnLevelComplete = true;
    [SerializeField] private bool autoLoadNextLevel = true;
    [SerializeField] private float autoLoadDelay = 5f;

    private bool levelCompleted = false;

    private void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void CompleteLevel()
    {
        if (levelCompleted)
        {
            return;
        }

        levelCompleted = true;

        HideOtherUI();

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompletePanel.transform.SetAsLastSibling();
        }

        if (pauseGameOnLevelComplete)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Level completed.");

        if (autoLoadNextLevel)
        {
            StartCoroutine(AutoLoadNextLevelRoutine());
        }
    }

    private void HideOtherUI()
    {
        if (uiObjectsToHide == null)
        {
            return;
        }

        for (int i = 0; i < uiObjectsToHide.Length; i++)
        {
            if (uiObjectsToHide[i] != null)
            {
                uiObjectsToHide[i].SetActive(false);
            }
        }
    }

    private IEnumerator AutoLoadNextLevelRoutine()
    {
        yield return new WaitForSecondsRealtime(autoLoadDelay);

        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("LevelEndManager: No next scene name assigned.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}