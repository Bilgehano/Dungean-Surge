using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("References")]
    public WaveManager waveManager;
    public TMP_Text waveInfoText;    // e.g. "Wave 1/2 - 10 Enemies Left"
    public TMP_Text statusText;      // e.g. "Wave Completed" or "3"
    public GameObject statusContainer;

    [Header("Announcement (Center)")]
    public TMP_Text announcementText;
    public GameObject announcementContainer;

    private void Start()
    {
        if (waveManager == null)
            waveManager = Object.FindAnyObjectByType<WaveManager>();
        
        if (statusContainer != null) statusContainer.SetActive(false);
        if (announcementContainer != null) announcementContainer.SetActive(false);
    }

    private void Update()
    {
        if (waveManager == null) return;

        bool isPaused = Time.timeScale <= 0f;

        // Update Wave Info (Bottom Left)
        if (waveInfoText != null)
        {
            if (waveManager.WaveActive || waveManager.EnemiesAlive > 0)
            {
                waveInfoText.text = $"WAVE {waveManager.CurrentWaveNumber}/{waveManager.TotalWaves} - {waveManager.EnemiesAlive} ENEMIES LEFT";
            }
            else
            {
                waveInfoText.text = $"WAVE {waveManager.CurrentWaveNumber}/{waveManager.TotalWaves} - ALL CLEAR";
            }
        }

        // Update Status/Announcements
    bool isWaiting = waveManager.IsWaiting && !isPaused;
        
        if (statusContainer != null) statusContainer.SetActive(isWaiting);
        if (announcementContainer != null) announcementContainer.SetActive(isWaiting);

        if (isWaiting)
        {
            string msg = "";
            if (waveManager.Countdown < 0)
            {
                msg = "WAVE COMPLETED";
            }
            else
            {
                // Show "WAVE X STARTING" or just the number
                msg = $"WAVE {waveManager.CurrentWaveNumber} STARTING IN\n{Mathf.CeilToInt(waveManager.Countdown)}";
            }

            if (statusText != null) statusText.text = msg;
            if (announcementText != null) announcementText.text = msg;
        }
}
}
