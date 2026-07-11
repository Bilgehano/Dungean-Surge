using UnityEngine;
using System.Collections;

public class Enemy_gethit : MonoBehaviour
{
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float stunDuration = 0.5f;
    public AudioClip hurtSound;

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private bool hasOriginalColor = false;
    private Enemy_Movement enemyMovement;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && !hasOriginalColor)
            {
                originalColor = spriteRenderer.color;
                // If original color is too red (maybe captured during a flash), default to white
                if (originalColor.r > 0.8f && originalColor.g < 0.2f && originalColor.b < 0.2f)
                {
                    originalColor = Color.white;
                }
                hasOriginalColor = true;
            }
        }
        if (enemyMovement == null)
        {
            enemyMovement = GetComponent<Enemy_Movement>();
        }
    }

    public void TriggerHit()
    {
        Initialize();

        if (spriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRed());
        }

        if (hurtSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hurtSound);
        }

        if (enemyMovement != null)
        {
            enemyMovement.Stun(stunDuration);
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void OnDisable()
    {
        // Ensure color is restored if disabled during flash
        if (spriteRenderer != null && hasOriginalColor)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
