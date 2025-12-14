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

  [Header("Blink Settings")]
  [Tooltip("Enable blinking when sanity is critically low")]
  [SerializeField] private bool blinkWhenLow = true;

  [Tooltip("Sanity level to start blinking (0-1)")]
  [SerializeField] private float criticalSanityThreshold = 0.15f;

  [Tooltip("Blink speed (times per second)")]
  [SerializeField] private float blinkSpeed = 2f;

  [Range(0f, 1f)]
  [Tooltip("Minimum alpha during blink (0=invisible, 1=fully visible)")]
  [SerializeField] private float blinkMinAlpha = 0.6f;

  [Header("Display Format")]
  [Tooltip("Format for sanity text. Use {0} for percentage")]
  [SerializeField] private string sanityTextFormat = "{0:0.0}%";

  [Tooltip("Text color for better contrast")]
  [SerializeField] private Color textColor = Color.white;

  [Tooltip("Enable text outline for better readability")]
  [SerializeField] private bool textOutline = true;

  [Tooltip("Outline color for text")]
  [SerializeField] private Color outlineColor = Color.black;

  private float targetFillAmount;
  private float currentFillAmount;
  private float blinkTimer;
  private bool isBlinkVisible = true;

  private void Awake()
  {
    ValidateComponents();
    // Ensure fill image is configured to respond to fillAmount changes
    if (sanityBarFill != null)
    {
      sanityBarFill.type = Image.Type.Filled;
      sanityBarFill.fillMethod = Image.FillMethod.Horizontal;
      sanityBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
      currentFillAmount = sanityBarFill.fillAmount;
      targetFillAmount = currentFillAmount;
    }
    else
    {
      // Initialize fill amount when no image assigned (avoid NaN in Update)
      currentFillAmount = 1f;
      targetFillAmount = 1f;
    }
  }

  private void Start()
  {
    // Initialize with full sanity immediately
    if (sanityBarFill != null)
    {
      sanityBarFill.fillAmount = 1f;
      sanityBarFill.color = highSanityColor;
    }

    // Apply text styling
    if (sanityText != null)
    {
      sanityText.text = string.Format(sanityTextFormat, 100f);
      sanityText.color = textColor;
      if (textOutline)
      {
        sanityText.outlineWidth = 0.2f;
        sanityText.outlineColor = outlineColor;
      }
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
    if (sanityBarFill != null)
    {
      if (smoothTransition)
      {
        // MoveTowards avoids asymptotic lerp and keeps animating even when timeScale is 0
        float step = transitionSpeed > 0f ? transitionSpeed * Time.unscaledDeltaTime : 1f;
        currentFillAmount = Mathf.MoveTowards(currentFillAmount, targetFillAmount, step);
      }
      else
      {
        currentFillAmount = targetFillAmount;
      }

      sanityBarFill.fillAmount = currentFillAmount;
    }

    // Handle blinking when sanity is critically low (but not empty)
    if (blinkWhenLow && currentFillAmount > 0f && currentFillAmount <= criticalSanityThreshold)
    {
      blinkTimer += Time.deltaTime * blinkSpeed;
      // Use sine wave to fade between blinkMinAlpha and 1.0
      float alpha = Mathf.Lerp(blinkMinAlpha, 1f, (Mathf.Sin(blinkTimer * Mathf.PI * 2f) + 1f) * 0.5f);

      if (sanityBarFill != null)
      {
        Color barCol = sanityBarFill.color;
        barCol.a = alpha;
        sanityBarFill.color = barCol;
      }
      if (sanityText != null)
      {
        Color textCol = sanityText.color;
        textCol.a = alpha;
        sanityText.color = textCol;
      }
    }
    else
    {
      // Ensure full visibility when not in critical state
      if (sanityBarFill != null)
      {
        Color barCol = sanityBarFill.color;
        barCol.a = 1f;
        sanityBarFill.color = barCol;
      }
      if (sanityText != null)
      {
        Color textCol = sanityText.color;
        textCol.a = 1f;
        sanityText.color = textCol;
      }
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
