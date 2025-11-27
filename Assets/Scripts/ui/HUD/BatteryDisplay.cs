using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the battery level with a visual indicator and optional text.
/// </summary>
public class BatteryDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The fill image for the battery bar (Image type should be set to Filled)")]
    [SerializeField] private Image batteryBarFill;

    [Tooltip("Optional background/frame for the battery")]
    [SerializeField] private Image batteryFrame;

    [Tooltip("Optional text to display battery percentage")]
    [SerializeField] private TextMeshProUGUI batteryText;

    [Tooltip("Optional icon for battery")]
    [SerializeField] private Image batteryIcon;

    [Header("Visual Settings")]
    [Tooltip("Color when battery is full/high")]
    [SerializeField] private Color fullBatteryColor = Color.green;

    [Tooltip("Color when battery is medium")]
    [SerializeField] private Color mediumBatteryColor = Color.yellow;

    [Tooltip("Color when battery is low")]
    [SerializeField] private Color lowBatteryColor = Color.red;

    [Range(0f, 1f)]
    [Tooltip("Battery percentage threshold for medium color (0-1)")]
    [SerializeField] private float mediumBatteryThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Battery percentage threshold for low color (0-1)")]
    [SerializeField] private float lowBatteryThreshold = 0.25f;

    [Header("Animation Settings")]
    [Tooltip("Enable smooth transition when battery changes")]
    [SerializeField] private bool smoothTransition = true;

    [Tooltip("Speed of the smooth transition")]
    [SerializeField] private float transitionSpeed = 5f;

    [Tooltip("Enable blinking when battery is critically low")]
    [SerializeField] private bool blinkWhenLow = true;

    [Tooltip("Battery level to start blinking (0-1)")]
    [SerializeField] private float criticalBatteryThreshold = 0.15f;

    [Tooltip("Blink speed (times per second)")]
    [SerializeField] private float blinkSpeed = 2f;

    [Header("Display Format")]
    [Tooltip("Format for battery text. Use {0} for current, {1} for max, {2} for percentage")]
    [SerializeField] private string batteryTextFormat = "{2:0}%";

    [Tooltip("Show battery icon sprites based on level")]
    [SerializeField] private bool useBatteryLevelIcons = false;

    [Tooltip("Sprite for full battery (75-100%)")]
    [SerializeField] private Sprite fullBatterySprite;

    [Tooltip("Sprite for high battery (50-75%)")]
    [SerializeField] private Sprite highBatterySprite;

    [Tooltip("Sprite for medium battery (25-50%)")]
    [SerializeField] private Sprite mediumBatterySprite;

    [Tooltip("Sprite for low battery (0-25%)")]
    [SerializeField] private Sprite lowBatterySprite;

    private float currentBattery;
    private float maxBattery;
    private float targetFillAmount;
    private float currentFillAmount;
    private float blinkTimer;
    private bool isBlinkVisible = true;

    private void Awake()
    {
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (batteryBarFill == null)
        {
            Debug.LogError("[BatteryDisplay] Battery bar fill image is not assigned!", this);
        }
    }

    private void Update()
    {
        // Smooth transition
        if (smoothTransition && batteryBarFill != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            batteryBarFill.fillAmount = currentFillAmount;
        }

        // Blinking effect when battery is critically low
        if (blinkWhenLow && targetFillAmount <= criticalBatteryThreshold)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            bool shouldBeVisible = Mathf.Sin(blinkTimer * Mathf.PI) > 0;

            if (shouldBeVisible != isBlinkVisible)
            {
                isBlinkVisible = shouldBeVisible;
                UpdateBlinkVisibility();
            }
        }
        else if (!isBlinkVisible)
        {
            isBlinkVisible = true;
            UpdateBlinkVisibility();
        }
    }

    private void UpdateBlinkVisibility()
    {
        if (batteryBarFill != null)
        {
            Color color = batteryBarFill.color;
            color.a = isBlinkVisible ? 1f : 0.3f;
            batteryBarFill.color = color;
        }

        if (batteryText != null)
        {
            Color textColor = batteryText.color;
            textColor.a = isBlinkVisible ? 1f : 0.3f;
            batteryText.color = textColor;
        }
    }

    /// <summary>
    /// Updates the battery display with current and max values.
    /// </summary>
    public void UpdateBattery(float current, float max)
    {
        currentBattery = Mathf.Clamp(current, 0, max);
        maxBattery = max;

        float normalizedBattery = maxBattery > 0 ? currentBattery / maxBattery : 0;
        UpdateBatteryNormalized(normalizedBattery);

        UpdateBatteryText();
    }

    /// <summary>
    /// Updates the battery display with a normalized value (0-1).
    /// </summary>
    public void UpdateBatteryNormalized(float normalizedBattery)
    {
        normalizedBattery = Mathf.Clamp01(normalizedBattery);
        targetFillAmount = normalizedBattery;

        if (batteryBarFill != null)
        {
            if (!smoothTransition)
            {
                batteryBarFill.fillAmount = targetFillAmount;
                currentFillAmount = targetFillAmount;
            }

            // Update color based on battery percentage
            Color batteryColor = GetBatteryColor(normalizedBattery);
            batteryColor.a = batteryBarFill.color.a; // Preserve alpha for blinking
            batteryBarFill.color = batteryColor;
        }

        // Update battery icon based on level
        if (useBatteryLevelIcons && batteryIcon != null)
        {
            UpdateBatteryIcon(normalizedBattery);
        }
    }

    private void UpdateBatteryText()
    {
        if (batteryText != null)
        {
            float percentage = maxBattery > 0 ? (currentBattery / maxBattery) * 100f : 0f;
            batteryText.text = string.Format(batteryTextFormat, currentBattery, maxBattery, percentage);
        }
    }

    private Color GetBatteryColor(float normalizedBattery)
    {
        if (normalizedBattery <= lowBatteryThreshold)
        {
            return lowBatteryColor;
        }
        else if (normalizedBattery <= mediumBatteryThreshold)
        {
            // Interpolate between low and medium
            float t = (normalizedBattery - lowBatteryThreshold) / (mediumBatteryThreshold - lowBatteryThreshold);
            return Color.Lerp(lowBatteryColor, mediumBatteryColor, t);
        }
        else
        {
            // Interpolate between medium and full
            float t = (normalizedBattery - mediumBatteryThreshold) / (1f - mediumBatteryThreshold);
            return Color.Lerp(mediumBatteryColor, fullBatteryColor, t);
        }
    }

    private void UpdateBatteryIcon(float normalizedBattery)
    {
        Sprite iconToUse = null;

        if (normalizedBattery >= 0.75f)
            iconToUse = fullBatterySprite;
        else if (normalizedBattery >= 0.5f)
            iconToUse = highBatterySprite;
        else if (normalizedBattery >= 0.25f)
            iconToUse = mediumBatterySprite;
        else
            iconToUse = lowBatterySprite;

        if (iconToUse != null)
        {
            batteryIcon.sprite = iconToUse;
        }
    }

    /// <summary>
    /// Sets a custom battery icon sprite.
    /// </summary>
    public void SetBatteryIcon(Sprite icon)
    {
        if (batteryIcon != null)
        {
            batteryIcon.sprite = icon;
        }
    }

    /// <summary>
    /// Shows or hides the battery display.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
