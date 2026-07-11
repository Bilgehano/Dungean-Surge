using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [Header("References")]
    public LevelManager levelManager;
    public Image xpFillImage;
    public TMP_Text levelText;

    [Header("Animation Settings")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    public float scaleUpOnCollect = 1.1f;
    public float scaleReturnSpeed = 5f;

    private Vector3 originalScale;
    private Color originalFillColor;

    private void Start()
    {
        originalScale = transform.localScale;
        if (xpFillImage != null) originalFillColor = xpFillImage.color;

        if (levelManager == null) 
            levelManager = Object.FindAnyObjectByType<LevelManager>();
        
        if (levelManager != null)
        {
            levelManager.onXPChanged += UpdateXPBar;
            levelManager.onLevelUp.AddListener(OnLevelUp);
            
            // Initial update
            UpdateXPBar((float)levelManager.currentXP / Mathf.Max(1, levelManager.xpToNextLevel));
            UpdateLevelText(levelManager.currentLevel);
        }
    }

    private void Update()
    {
        // Continuous pixel pulse animation
        if (xpFillImage != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            xpFillImage.color = originalFillColor * pulse;
        }

        // Return to original scale
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleReturnSpeed);
    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.onXPChanged -= UpdateXPBar;
            levelManager.onLevelUp.RemoveListener(OnLevelUp);
        }
    }

    private void UpdateXPBar(float progress)
    {
        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = progress;
            // Bump scale on change
            transform.localScale = originalScale * scaleUpOnCollect;
        }
    }

    private void OnLevelUp(int level)
    {
        UpdateLevelText(level);
        // Extra bump on level up
        transform.localScale = originalScale * (scaleUpOnCollect + 0.2f);
    }

    private void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "LEVEL " + level;
        }
    }
}