using UnityEngine;

public static class PlayerSessionData
{
    public static bool HasData { get; private set; }

    public static int CurrentLevel { get; private set; }
    public static int CurrentXP { get; private set; }

    public static int MaxHealth { get; private set; }
    public static int CurrentHealth { get; private set; }

    public static int AttackDamage { get; private set; }
    public static float MoveSpeed { get; private set; }

    public static float DefensePercent { get; private set; }
    public static float HealthRegenAmount { get; private set; }

    // Wird einmal beim Start eines neuen Play-Durchlaufs aufgerufen.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlayStart()
    {
        Reset();
    }

    public static void Save()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "PlayerSessionData: No GameObject with the Player tag was found."
            );
            return;
        }

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        PlayerMovement playerMovement =
            player.GetComponent<PlayerMovement>();

        Player_Combat playerCombat =
            player.GetComponent<Player_Combat>();

        PlayerStats playerStats =
            player.GetComponent<PlayerStats>();

        LevelManager levelManager =
            player.GetComponent<LevelManager>();

        if (playerHealth == null ||
            playerMovement == null ||
            playerCombat == null ||
            playerStats == null ||
            levelManager == null)
        {
            Debug.LogWarning(
                "PlayerSessionData: The Player is missing one or more required components."
            );
            return;
        }

        CurrentLevel = levelManager.currentLevel;
        CurrentXP = levelManager.currentXP;

        MaxHealth = playerHealth.maxHealth;
        CurrentHealth = playerHealth.currentHealth;

        AttackDamage = playerCombat.damageAmount;
        MoveSpeed = playerMovement.moveSpeed;

        DefensePercent = playerStats.DefensePercent;
        HealthRegenAmount = playerStats.HealthRegenAmount;

        HasData = true;

        Debug.Log(
            "Player session saved. " +
            "Level: " + CurrentLevel +
            ", XP: " + CurrentXP +
            ", HP: " + CurrentHealth + "/" + MaxHealth +
            ", Damage: " + AttackDamage +
            ", Speed: " + MoveSpeed +
            ", Defense: " + DefensePercent + "%" +
            ", Regeneration: " + HealthRegenAmount
        );
    }

    public static void Reset()
    {
        HasData = false;

        CurrentLevel = 1;
        CurrentXP = 0;

        MaxHealth = 100;
        CurrentHealth = 100;

        AttackDamage = -1;
        MoveSpeed = 5f;

        DefensePercent = 0f;
        HealthRegenAmount = 0f;
    }
}