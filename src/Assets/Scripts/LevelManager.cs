using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [Header("Settings")]
    public int baseXPRequired = 10;
    public int xpIncreasePerLevel = 10;

    [Header("State")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    [Header("Level Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Events")]
    public UnityEvent<int> onLevelUp;
    public System.Action<float> onXPChanged;

    private PlayerResources playerResources;

    private void Awake()
    {
        if (PlayerSessionData.HasData)
        {
            currentLevel = Mathf.Max(
                1,
                PlayerSessionData.CurrentLevel
            );

            currentXP = Mathf.Max(
                0,
                PlayerSessionData.CurrentXP
            );
        }

        UpdateLevelThreshold();
    }

    private void Start()
    {
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusic, 0.25f);
        }

        playerResources = GetComponent<PlayerResources>();
        if (playerResources != null)
        {
            playerResources.OnGoldAdded += HandleGoldAdded;
        }
        else
        {
            playerResources = Object.FindAnyObjectByType<PlayerResources>();
            if (playerResources != null)
            {
                playerResources.OnGoldAdded += HandleGoldAdded;
            }
        }
        onXPChanged?.Invoke((float)currentXP / xpToNextLevel);
    }

    private void OnDestroy()
    {
        if (playerResources != null)
        {
            playerResources.OnGoldAdded -= HandleGoldAdded;
        }
    }

    private void HandleGoldAdded(int amount)
    {
        currentXP += amount;
        
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        onXPChanged?.Invoke((float)currentXP / xpToNextLevel);
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        UpdateLevelThreshold();
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelUpSFX();
        }

        Debug.Log("upgrade cart system");
        onLevelUp?.Invoke(currentLevel);
    }

    private void UpdateLevelThreshold()
    {
        xpToNextLevel = currentLevel * 25;
    }
}