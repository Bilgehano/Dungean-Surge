using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;


    public TMP_Text healthText;
    public Animator HealthBarAnimator;

    void Start()
    {
        currentHealth = maxHealth;
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        HealthBarAnimator.Play("Type");
        currentHealth += amount;
        healthText.text = "HP: " + currentHealth + "/" + maxHealth;
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
