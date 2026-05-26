using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;

    private UpgradeCardData cardData;
    private UpgradeSelectionManager selectionManager;

    public void Setup(UpgradeCardData data, UpgradeSelectionManager manager)
    {
        cardData = data;
        selectionManager = manager;

        if (titleText != null)
        {
            titleText.text = data.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SelectCard);
        }
    }

    private void SelectCard()
    {
        if (selectionManager != null && cardData != null)
        {
            selectionManager.SelectUpgrade(cardData);
        }
    }
}