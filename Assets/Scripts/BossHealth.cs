using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int healthSegments = 4;
    [SerializeField] private float deathAnimationDuration = 1.2f;

    [Header("State (Read Only)")]
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead;
    [SerializeField] private int nextHurtThreshold;
    [SerializeField] private int currentPhase = 1;

    [Header("Damage Feedback")]
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.25f, 0.25f, 0.6f);
    [SerializeField] private float damageFlashDuration = 0.15f;

    private BossManager bossManager;
    private BossHealthBar bossHealthBar;
    private Animator animator;
    private BossController bossController;
    private SpriteRenderer[] spriteRenderers;
    private Color[] defaultColors;
    private Coroutine damageFlashRoutine;

    public int CurrentPhase => currentPhase;

    private int HurtStep
    {
        get
        {
            return Mathf.Max(1, maxHealth / healthSegments);
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        nextHurtThreshold = maxHealth - HurtStep;
        currentPhase = 1;

        animator = GetComponent<Animator>();
        bossController = GetComponent<BossController>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        defaultColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            defaultColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }
    }

    public void SetBossManager(BossManager manager)
    {
        bossManager = manager;
    }

    public void SetBossHealthBar(BossHealthBar healthBar)
    {
        bossHealthBar = healthBar;

        if (bossHealthBar != null)
        {
            bossHealthBar.SetMaxHealth(maxHealth);
            bossHealthBar.SetHealth(currentHealth);
            bossHealthBar.Show();
        }
        else
        {
            Debug.LogWarning("BossHealth: BossHealthBar is missing.");
        }
    }

    public void ChangeHealth(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdatePhase();

        if (bossHealthBar != null)
        {
            bossHealthBar.SetHealth(currentHealth);
        }

        Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth + " | Phase: " + currentPhase);

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        if (amount < 0)
        {
            TriggerDamageFlash();
        }

        if (amount < 0 && currentHealth <= nextHurtThreshold)
        {
            PlayHurtAnimation();

            while (currentHealth <= nextHurtThreshold && nextHurtThreshold > 0)
            {
                nextHurtThreshold -= HurtStep;
            }
        }
    }

    private void UpdatePhase()
    {
        int oldPhase = currentPhase;

        if (currentHealth <= maxHealth * 0.25f)
        {
            currentPhase = 4;
        }
        else if (currentHealth <= maxHealth * 0.5f)
        {
            currentPhase = 3;
        }
        else if (currentHealth <= maxHealth * 0.75f)
        {
            currentPhase = 2;
        }
        else
        {
            currentPhase = 1;
        }

        if (oldPhase != currentPhase)
        {
            Debug.Log("Boss changed to phase " + currentPhase);
        }
    }

    private void PlayHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    private IEnumerator DieRoutine()
    {
        if (isDead)
        {
            yield break;
        }

        isDead = true;

        if (bossController != null)
        {
            bossController.DeactivateBoss();
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        Debug.Log("Boss died.");

        yield return new WaitForSeconds(deathAnimationDuration);

        if (bossHealthBar != null)
        {
            bossHealthBar.Hide();
        }

        if (bossManager != null)
        {
            bossManager.OnBossDied();
        }

        Destroy(gameObject);
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
}