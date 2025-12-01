using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's health with a visual bar and optional text.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The fill image for the health bar (Image type should be set to Filled)")]
    [SerializeField] private Image healthBarFill;

    [Tooltip("Optional text to display health value (e.g., '75/100')")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Tooltip("Optional icon/image for health")]
    [SerializeField] private Image healthIcon;

    [Header("Visual Settings")]
    [Tooltip("Color when health is full/high")]
    [SerializeField] private Color highHealthColor = Color.green;

    [Tooltip("Color when health is medium")]
    [SerializeField] private Color mediumHealthColor = Color.yellow;

    [Tooltip("Color when health is low")]
    [SerializeField] private Color lowHealthColor = Color.red;

    [Range(0f, 1f)]
    [Tooltip("Health percentage threshold for medium color (0-1)")]
    [SerializeField] private float mediumHealthThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Health percentage threshold for low color (0-1)")]
    [SerializeField] private float lowHealthThreshold = 0.25f;

    [Header("Animation Settings")]
    [Tooltip("Enable smooth transition when health changes")]
    [SerializeField] private bool smoothTransition = true;

    [Tooltip("Speed of the smooth transition")]
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Display Format")]
    [Tooltip("Format for health text. Use {0} for current health, {1} for max health, {2} for percentage")]
    [SerializeField] private string healthTextFormat = "{0:0}/{1:0}";

    [Tooltip("Show percentage in text")]
    [SerializeField] private bool showPercentage = false;

    private float currentHealth;
    private float maxHealth = 100f;
    private float targetFillAmount;
    private float currentFillAmount;

    private void Awake()
    {
        ValidateComponents();
    }

    private void Start()
    {
        // Initialize with full health (0 fear)
        UpdateHealth(maxHealth, maxHealth);
    }

    private void OnEnable()
    {
        // Subscribe to player fear updates (fear = health in this game)
        PlayerAttributes.onFearUpdate += OnFearUpdated;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        PlayerAttributes.onFearUpdate -= OnFearUpdated;
    }

    /// <summary>
    /// Called when player's fear value changes
    /// </summary>
    private void OnFearUpdated(float fearValue)
    {
        // Fear acts as health - 0 fear = full health, 100 fear = dead
        // Invert the value so UI shows health correctly
        float healthValue = maxHealth - fearValue;
        UpdateHealth(healthValue, maxHealth);
    }

    private void ValidateComponents()
    {
        if (healthBarFill == null)
        {
            Debug.LogError("[HealthDisplay] Health bar fill image is not assigned!", this);
        }
    }

    private void Update()
    {
        if (smoothTransition && healthBarFill != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            healthBarFill.fillAmount = currentFillAmount;
        }
    }

    /// <summary>
    /// Updates the health display with current and max values.
    /// </summary>
    public void UpdateHealth(float current, float max)
    {
        currentHealth = Mathf.Clamp(current, 0, max);
        maxHealth = max;

        float normalizedHealth = maxHealth > 0 ? currentHealth / maxHealth : 0;
        UpdateHealthNormalized(normalizedHealth);

        UpdateHealthText();
    }

    /// <summary>
    /// Updates the health display with a normalized value (0-1).
    /// </summary>
    public void UpdateHealthNormalized(float normalizedHealth)
    {
        normalizedHealth = Mathf.Clamp01(normalizedHealth);
        targetFillAmount = normalizedHealth;

        if (healthBarFill != null)
        {
            if (!smoothTransition)
            {
                healthBarFill.fillAmount = targetFillAmount;
                currentFillAmount = targetFillAmount;
            }

            // Update color based on health percentage
            healthBarFill.color = GetHealthColor(normalizedHealth);
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            if (showPercentage)
            {
                float percentage = maxHealth > 0 ? (currentHealth / maxHealth) * 100f : 0f;
                healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth, percentage);
            }
            else
            {
                healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth);
            }
        }
    }

    private Color GetHealthColor(float normalizedHealth)
    {
        if (normalizedHealth <= lowHealthThreshold)
        {
            return lowHealthColor;
        }
        else if (normalizedHealth <= mediumHealthThreshold)
        {
            // Interpolate between low and medium
            float t = (normalizedHealth - lowHealthThreshold) / (mediumHealthThreshold - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, mediumHealthColor, t);
        }
        else
        {
            // Interpolate between medium and high
            float t = (normalizedHealth - mediumHealthThreshold) / (1f - mediumHealthThreshold);
            return Color.Lerp(mediumHealthColor, highHealthColor, t);
        }
    }

    /// <summary>
    /// Sets the health icon sprite.
    /// </summary>
    public void SetHealthIcon(Sprite icon)
    {
        if (healthIcon != null)
        {
            healthIcon.sprite = icon;
        }
    }

    /// <summary>
    /// Shows or hides the health display.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
