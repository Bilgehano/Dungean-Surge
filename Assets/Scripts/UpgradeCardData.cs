using UnityEngine;

public enum UpgradeType
{
    Attack,
    Defense,
    MaxHealth,
    MoveSpeed,
    HealthRegen
}

[System.Serializable]
public class UpgradeCardData
{
    public string title;
    [TextArea] public string description;
    public UpgradeType upgradeType;
    public float value;
}