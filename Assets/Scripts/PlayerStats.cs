using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Player_Combat playerCombat;

    [Header("Defense")]
    [SerializeField] private float defensePercent = 0f;
    [SerializeField] private float maxDefensePercent = 50f;

    [Header("Health Regeneration")]
    [SerializeField] private float healthRegenAmount = 0f;
    [SerializeField] private float healthRegenInterval = 3f;

    private Coroutine regenRoutine;

    public float DefensePercent => defensePercent;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerCombat == null)
        {
            playerCombat = GetComponent<Player_Combat>();
        }
    }

    private void Start()
    {
        StartHealthRegenIfNeeded();
    }

    public void AddAttackDamage(int amount)
    {
        if (playerCombat != null)
        {
            playerCombat.damageAmount -= amount;
            Debug.Log("Attack upgraded. Damage is now: " + playerCombat.damageAmount);
        }
    }

    public void AddDefense(float amount)
    {
        defensePercent += amount;
        defensePercent = Mathf.Clamp(defensePercent, 0f, maxDefensePercent);

        Debug.Log("Defense upgraded to " + defensePercent + "%");
    }

    public void AddMaxHealth(int amount)
    {
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(amount);
        }
    }

    public void AddMoveSpeed(float amount)
    {
        if (playerMovement != null)
        {
            playerMovement.moveSpeed += amount;
            Debug.Log("Move speed upgraded to " + playerMovement.moveSpeed);
        }
    }

    public void AddHealthRegen(float amount)
    {
        healthRegenAmount += amount;
        StartHealthRegenIfNeeded();

        Debug.Log("Health regeneration upgraded to " + healthRegenAmount + " HP every " + healthRegenInterval + " seconds.");
    }

    private void StartHealthRegenIfNeeded()
    {
        if (regenRoutine == null && healthRegenAmount > 0f)
        {
            regenRoutine = StartCoroutine(HealthRegenRoutine());
        }
    }

    private IEnumerator HealthRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(healthRegenInterval);

            if (playerHealth != null && healthRegenAmount > 0f)
            {
                playerHealth.ChangeHealth(Mathf.RoundToInt(healthRegenAmount));
            }
        }
    }
}