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

    public event System.Action<int> OnGoldAdded;

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gold += amount;
        OnGoldAdded?.Invoke(amount);
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
