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

    private BossManager bossManager;
    private BossHealthBar bossHealthBar;
    private Animator animator;
    private BossController bossController;

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
}