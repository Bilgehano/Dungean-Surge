using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private float displayDuration = 5f;

    [Header("UI Elements to Hide")]
    [SerializeField] private GameObject healthBarUI;
    [SerializeField] private GameObject waveInfoUI;
    [SerializeField] private GameObject xpLevelBarUI;

    public void ShowVictoryAndExit()
    {
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }

        if (waveInfoUI != null)
        {
            waveInfoUI.SetActive(false);
        }

        if (xpLevelBarUI != null)
        {
            xpLevelBarUI.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            victoryPanel.transform.SetAsLastSibling();
        }

        yield return new WaitForSecondsRealtime(displayDuration);

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}