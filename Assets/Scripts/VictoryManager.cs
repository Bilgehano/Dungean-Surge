using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private float displayDuration = 10f;

    public void ShowVictoryAndExit()
    {
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            victoryPanel.transform.SetAsLastSibling();
        }

        yield return new WaitForSecondsRealtime(displayDuration);

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}