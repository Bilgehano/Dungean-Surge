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

    [Header("Events")]
    public UnityEvent<int> onLevelUp;
    public System.Action<float> onXPChanged; // progress 0 to 1

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
        playerResources = GetComponent<PlayerResources>();
        if (playerResources != null)
        {
            playerResources.OnGoldAdded += HandleGoldAdded;
        }
        else
        {
            // Try finding it in the scene if not on the same object
            playerResources = Object.FindAnyObjectByType<PlayerResources>();
            if (playerResources != null)
            {
                playerResources.OnGoldAdded += HandleGoldAdded;
            }
        }
        // Initial progress
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
        
        Debug.Log("upgrade cart system");
        onLevelUp?.Invoke(currentLevel);
    }

    private void UpdateLevelThreshold()
    {
        // 25, 50, 75... 
        xpToNextLevel = currentLevel * 25;
    }
}