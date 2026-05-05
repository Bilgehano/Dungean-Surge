using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;


    public TMP_Text healthText;
    public Animator HealthBarAnimator;
    public AudioClip hurtSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        currentHealth = maxHealth;
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        HealthBarAnimator.Play("Type");
        currentHealth += amount;
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;

        if (amount < 0 && hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Handle player death (e.g., play animation, disable controls, etc.)
        Debug.Log("Player has died.");
        gameObject.SetActive(false);
    }



}
