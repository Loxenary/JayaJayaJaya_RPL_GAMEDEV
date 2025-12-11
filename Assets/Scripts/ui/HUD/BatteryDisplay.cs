using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the battery level with a visual indicator and optional text.
/// </summary>
public class BatteryDisplay : MonoBehaviour
{
    [Header("Battery Display")]
    [Tooltip("Image component to display battery sprite")]
    [SerializeField] private Image batteryIcon;

    [Tooltip("Array of battery sprites from empty to full. Index 0 = empty, last index = full")]
    [SerializeField] private Sprite[] batterySprites = new Sprite[0];

    [Header("Optional Text Display")]
    [Tooltip("Optional text to display battery percentage")]
    [SerializeField] private TextMeshProUGUI batteryText;

    [Tooltip("Format for battery text. Use {0} for current, {1} for max, {2} for percentage")]
    [SerializeField] private string batteryTextFormat = "{2:0}%";

    [Header("Blinking Effect")]
    [Tooltip("Enable blinking when battery is critically low")]
    [SerializeField] private bool blinkWhenLow = true;

    [Tooltip("Battery level to start blinking (0-1)")]
    [SerializeField] private float criticalBatteryThreshold = 0.15f;

    [Tooltip("Blink speed (times per second)")]
    [SerializeField] private float blinkSpeed = 2f;

    private float currentBattery;
    private float maxBattery = 100f;
    private float blinkTimer;
    private bool isBlinkVisible = true;

    private void Awake()
    {
        ValidateComponents();
    }

    private void Start()
    {
        // Initialize with full battery
        UpdateBattery(maxBattery, maxBattery);
    }

    private void OnEnable()
    {
        // Subscribe to player battery updates
        PlayerAttributes.onBatteryUpdate += OnBatteryUpdated;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        PlayerAttributes.onBatteryUpdate -= OnBatteryUpdated;
    }

    /// <summary>
    /// Called when player's battery value changes
    /// </summary>
    private void OnBatteryUpdated(float batteryValue)
    {
        UpdateBattery(batteryValue, maxBattery);
    }

    private void ValidateComponents()
    {
        if (batteryIcon == null)
        {
            Debug.LogError("[BatteryDisplay] batteryIcon is not assigned!", this);
        }

        if (batterySprites == null || batterySprites.Length == 0)
        {
            Debug.LogWarning("[BatteryDisplay] batterySprites array is empty!", this);
        }
    }

    private void Update()
    {
        // Blinking effect when battery is critically low
        float normalizedBattery = maxBattery > 0 ? currentBattery / maxBattery : 0;

        if (blinkWhenLow && normalizedBattery <= criticalBatteryThreshold)
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
        if (batteryIcon != null)
        {
            Color iconColor = batteryIcon.color;
            iconColor.a = isBlinkVisible ? 1f : 0.3f;
            batteryIcon.color = iconColor;
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

        // Update battery sprite
        if (batteryIcon != null)
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

    private void UpdateBatteryIcon(float normalizedBattery)
    {
        if (batterySprites == null || batterySprites.Length == 0 || batteryIcon == null)
            return;

        // Calculate which sprite to use based on battery level
        // normalizedBattery is 0-1, map it to sprite array indices
        int spriteIndex = Mathf.RoundToInt(normalizedBattery * (batterySprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, batterySprites.Length - 1);

        batteryIcon.sprite = batterySprites[spriteIndex];
    }

    /// <summary>
    /// Sets the battery sprites array at runtime.
    /// </summary>
    public void SetBatterySprites(Sprite[] sprites)
    {
        batterySprites = sprites;
    }
}
