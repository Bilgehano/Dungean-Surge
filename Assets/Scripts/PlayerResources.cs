using TMPro;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Header("Gold")]
    public int gold;
    public TMP_Text goldText;

    private void Start()
    {
        RefreshGoldText();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gold += amount;
        RefreshGoldText();
    }

    private void RefreshGoldText()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + gold;
        }
    }
}
