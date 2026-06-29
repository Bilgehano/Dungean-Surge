using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public TMP_Text healthText;
    public Animator HealthBarAnimator;
    [SerializeField] private PlayerStats playerStats;
    public AudioClip hurtSound;
    private AudioSource audioSource;

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        if (PlayerSessionData.HasData)
        {
            maxHealth = Mathf.Max(1, PlayerSessionData.MaxHealth);
            currentHealth = Mathf.Clamp(PlayerSessionData.CurrentHealth, 0, maxHealth);
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        RefreshHealthText();
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0 && playerStats != null)
        {
            int incomingDamage = Mathf.Abs(amount);
            float reductionMultiplier = 1f - (playerStats.DefensePercent / 100f);
            int reducedDamage = Mathf.CeilToInt(incomingDamage * reductionMultiplier);
            reducedDamage = Mathf.Max(1, reducedDamage);
            amount = -reducedDamage;
        }

        if (HealthBarAnimator != null)
        {
            HealthBarAnimator.Play("Type");
        }
    
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        RefreshHealthText();

        if (amount < 0 && hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        RefreshHealthText();
    }

    private void RefreshHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        }
    }  

    public void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.endingSound);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
            Time.timeScale = 0f;
        }

        Debug.Log("Player has died.");
        gameObject.SetActive(false);
    }
}