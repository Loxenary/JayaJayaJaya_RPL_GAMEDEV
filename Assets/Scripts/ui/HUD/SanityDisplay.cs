using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's sanity with a visual bar and optional text.
/// Sanity: 100% = full/healthy, 0% = dead
/// </summary>
public class SanityDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The fill image for the sanity bar (Image type should be set to Filled)")]
    [SerializeField] private Image sanityBarFill;

    [Tooltip("Optional text to display sanity value")]
    [SerializeField] private TextMeshProUGUI sanityText;

    [Tooltip("Optional icon/image for sanity")]
    [SerializeField] private Image sanityIcon;

    [Header("Visual Settings")]
    [Tooltip("Color when sanity is full/high")]
    [SerializeField] private Color highSanityColor = Color.green;

    [Tooltip("Color when sanity is medium")]
    [SerializeField] private Color mediumSanityColor = Color.yellow;

    [Tooltip("Color when sanity is low")]
    [SerializeField] private Color lowSanityColor = Color.red;

    [Range(0f, 1f)]
    [Tooltip("Sanity percentage threshold for medium color (0-1)")]
    [SerializeField] private float mediumSanityThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Sanity percentage threshold for low color (0-1)")]
    [SerializeField] private float lowSanityThreshold = 0.25f;

    [Header("Animation Settings")]
    [Tooltip("Enable smooth transition when sanity changes")]
    [SerializeField] private bool smoothTransition = true;

    [Tooltip("Speed of the smooth transition")]
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Display Format")]
    [Tooltip("Format for sanity text. Use {0} for percentage")]
    [SerializeField] private string sanityTextFormat = "{0:0}%";

    private float targetFillAmount;
    private float currentFillAmount;

    private void Awake()
    {
        ValidateComponents();
        // Initialize fill amount
        currentFillAmount = 1f;
        targetFillAmount = 1f;
    }

    private void Start()
    {
        // Initialize with full sanity immediately
        if (sanityBarFill != null)
        {
            sanityBarFill.fillAmount = 1f;
            sanityBarFill.color = highSanityColor;
        }

        // Update text to show 100%
        if (sanityText != null)
        {
            sanityText.text = string.Format(sanityTextFormat, 100f);
        }
    }

    private void OnEnable()
    {
        // Subscribe to player sanity updates
        PlayerAttributes.onSanityUpdate += OnSanityUpdated;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        PlayerAttributes.onSanityUpdate -= OnSanityUpdated;
    }

    /// <summary>
    /// Called when player's sanity value changes
    /// </summary>
    private void OnSanityUpdated(float normalizedSanity)
    {
        UpdateSanityNormalized(normalizedSanity);
    }

    private void ValidateComponents()
    {
        if (sanityBarFill == null)
        {
            Debug.LogError("[SanityDisplay] Sanity bar fill image is not assigned!", this);
        }
    }

    private void Update()
    {
        if (smoothTransition && sanityBarFill != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            sanityBarFill.fillAmount = currentFillAmount;
        }
    }

    /// <summary>
    /// Updates the sanity display with current and max values.
    /// </summary>
    public void UpdateSanity(float currentSanity, float maxSanity)
    {
        float normalized = maxSanity > 0 ? currentSanity / maxSanity : 0f;
        UpdateSanityNormalized(normalized);
    }

    /// <summary>
    /// Updates the sanity display with a normalized value (0-1).
    /// </summary>
    public void UpdateSanityNormalized(float normalizedSanity)
    {
        normalizedSanity = Mathf.Clamp01(normalizedSanity);
        targetFillAmount = normalizedSanity;

        if (sanityBarFill != null)
        {
            if (!smoothTransition)
            {
                sanityBarFill.fillAmount = targetFillAmount;
                currentFillAmount = targetFillAmount;
            }

            // Update color based on sanity percentage
            sanityBarFill.color = GetSanityColor(normalizedSanity);
        }

        UpdateSanityText(normalizedSanity);
    }

    private void UpdateSanityText(float normalizedSanity)
    {
        if (sanityText != null)
        {
            float percentage = normalizedSanity * 100f;
            sanityText.text = string.Format(sanityTextFormat, percentage);
        }
    }

    private Color GetSanityColor(float normalizedSanity)
    {
        if (normalizedSanity <= lowSanityThreshold)
        {
            return lowSanityColor;
        }
        else if (normalizedSanity <= mediumSanityThreshold)
        {
            // Interpolate between low and medium
            float t = (normalizedSanity - lowSanityThreshold) / (mediumSanityThreshold - lowSanityThreshold);
            return Color.Lerp(lowSanityColor, mediumSanityColor, t);
        }
        else
        {
            // Interpolate between medium and high
            float t = (normalizedSanity - mediumSanityThreshold) / (1f - mediumSanityThreshold);
            return Color.Lerp(mediumSanityColor, highSanityColor, t);
        }
    }

    /// <summary>
    /// Sets the sanity icon sprite.
    /// </summary>
    public void SetSanityIcon(Sprite icon)
    {
        if (sanityIcon != null)
        {
            sanityIcon.sprite = icon;
        }
    }

    /// <summary>
    /// Shows or hides the sanity display.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
