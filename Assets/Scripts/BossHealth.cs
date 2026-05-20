using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int hurtStep = 25;
    [SerializeField] private float deathAnimationDuration = 1.2f;

    [Header("State (Read Only)")]
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead;
    [SerializeField] private int nextHurtThreshold;

    private BossManager bossManager;
    private BossHealthBar bossHealthBar;
    private Animator animator;
    private BossController bossController;

    private void Awake()
    {
        currentHealth = maxHealth;
        nextHurtThreshold = maxHealth - hurtStep;

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
    }

    public void ChangeHealth(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (bossHealthBar != null)
        {
            bossHealthBar.SetHealth(currentHealth);
        }

        Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth);

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
                nextHurtThreshold -= hurtStep;
            }
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