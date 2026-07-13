using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    public GameObject soundContainer;
    [SerializeField] private GameObject waveStatusContainer;
    [SerializeField] private GameObject waveAnnouncementContainer;

    private bool isPaused;

    void Start()
    {
        container.SetActive(false);
        soundContainer.SetActive(false);
        isPaused = false;
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeButton();
                return;
            }

            PauseGame();
        }
    }

    private void PauseGame()
    {
        container.SetActive(true);
        soundContainer.SetActive(false);
        SetWaveUiVisible(false);
        isPaused = true;
        Time.timeScale = 0;
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        soundContainer.SetActive(false);
        SetWaveUiVisible(true);
        isPaused = false;
        Time.timeScale = 1;
    }

    public void MainMenuButton()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void SoundButton()
    {
        soundContainer.SetActive(true);
    }

    public void SoundBackButton()
    {
        soundContainer.SetActive(false);
    }

    private void SetWaveUiVisible(bool isVisible)
    {
        if (waveStatusContainer != null)
        {
            waveStatusContainer.SetActive(isVisible);
        }

        if (waveAnnouncementContainer != null)
        {
            waveAnnouncementContainer.SetActive(isVisible);
        }
    }
}