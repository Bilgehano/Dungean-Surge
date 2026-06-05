using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Level2";

    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnLevelComplete = true;

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

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        if (pauseGameOnLevelComplete)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Level completed.");
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