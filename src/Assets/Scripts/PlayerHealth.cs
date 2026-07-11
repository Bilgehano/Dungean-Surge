using TMPro;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public TMP_Text healthText;
    public Animator HealthBarAnimator;
    [SerializeField] private PlayerStats playerStats;
    public AudioClip hurtSound;

    [Header("Damage Feedback")]
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.25f, 0.25f, 0.6f);
    [SerializeField] private float damageFlashDuration = 0.15f;

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel;

    private SpriteRenderer[] spriteRenderers;
    private Color[] defaultColors;
    private Coroutine damageFlashRoutine;

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

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        defaultColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            defaultColors[i] = spriteRenderers[i] != null
                ? spriteRenderers[i].color
                : Color.white;
        }
    }

    void Start()
    {
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

        if (amount < 0 && hurtSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hurtSound);
        }

        if (amount < 0)
        {
            TriggerDamageFlash();
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

    private void TriggerDamageFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            return;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color flashColor = damageFlashColor;
                flashColor.a = defaultColors[i].a * damageFlashColor.a;
                spriteRenderers[i].color = flashColor;
            }
        }

        yield return new WaitForSeconds(Mathf.Max(0.02f, damageFlashDuration));

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = defaultColors[i];
            }
        }

        damageFlashRoutine = null;
    }

    public void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayGameOverSFX();
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