using System.Collections.Generic;
using UnityEngine;

public class UpgradeSelectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PlayerStats playerStats;

    [Header("UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private UpgradeCardUI[] cardSlots;

    [Header("Available Upgrades")]
    [SerializeField] private List<UpgradeCardData> availableUpgrades = new List<UpgradeCardData>();

    [Header("Settings")]
    [SerializeField] private int cardsToShow = 3;
    [SerializeField] private bool pauseGameDuringSelection = true;

    private void Start()
    {
        if (levelManager == null)
        {
            levelManager = Object.FindAnyObjectByType<LevelManager>();
        }

        if (playerStats == null)
        {
            playerStats = Object.FindAnyObjectByType<PlayerStats>();
        }

        if (levelManager != null)
        {
            levelManager.onLevelUp.AddListener(OpenUpgradeSelection);
        }

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.onLevelUp.RemoveListener(OpenUpgradeSelection);
        }
    }

    public void OpenUpgradeSelection(int level)
    {
        if (upgradePanel == null || cardSlots == null || cardSlots.Length == 0)
        {
            Debug.LogWarning("UpgradeSelectionManager: UI is not assigned.");
            return;
        }

        upgradePanel.SetActive(true);

        if (pauseGameDuringSelection)
        {
            Time.timeScale = 0f;
        }

        List<UpgradeCardData> selectedCards = GetRandomUpgradeCards();

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < selectedCards.Count)
            {
                cardSlots[i].gameObject.SetActive(true);
                cardSlots[i].Setup(selectedCards[i], this);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectUpgrade(UpgradeCardData upgrade)
    {
        ApplyUpgrade(upgrade);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUpgradeChooseSFX();
        }

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        if (pauseGameDuringSelection)
        {
            Time.timeScale = 1f;
        }
    }

    private List<UpgradeCardData> GetRandomUpgradeCards()
    {
        List<UpgradeCardData> pool = new List<UpgradeCardData>(availableUpgrades);
        List<UpgradeCardData> result = new List<UpgradeCardData>();

        int amount = Mathf.Min(cardsToShow, cardSlots.Length, pool.Count);

        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            result.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }

    private void ApplyUpgrade(UpgradeCardData upgrade)
    {
        if (playerStats == null || upgrade == null)
        {
            return;
        }

        switch (upgrade.upgradeType)
        {
            case UpgradeType.Attack:
                playerStats.AddAttackDamage(Mathf.RoundToInt(upgrade.value));
                break;

            case UpgradeType.Defense:
                playerStats.AddDefense(upgrade.value);
                break;

            case UpgradeType.MaxHealth:
                playerStats.AddMaxHealth(Mathf.RoundToInt(upgrade.value));
                break;

            case UpgradeType.MoveSpeed:
                playerStats.AddMoveSpeed(upgrade.value);
                break;

            case UpgradeType.HealthRegen:
                playerStats.AddHealthRegen(upgrade.value);
                break;
        }
    }
}