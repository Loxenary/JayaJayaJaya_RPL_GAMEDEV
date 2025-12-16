using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays battery as discrete segments (bars) with 10% increments.
/// Each segment represents 10% of battery. Blinks when critically low.
/// </summary>
[ExecuteAlways] // Allows preview in Editor without Play mode
public class DiscreteBatteryDisplay : MonoBehaviour
{
  [Header("Editor Preview")]
  [Range(0f, 100f)]
  [Tooltip("Preview battery value in Editor (drag to see changes)")]
  [SerializeField] private float previewBatteryValue = 100f;

  [Header("UI References")]
  [Tooltip("Container for battery segments (will auto-create if not assigned)")]
  [SerializeField] private Transform segmentContainer;

  [Tooltip("Prefab for a single battery segment")]
  [SerializeField] private Image segmentPrefab;

  [Tooltip("Optional text to display battery percentage")]
  [SerializeField] private TextMeshProUGUI batteryText;

  [Header("Segment Settings")]
  [Tooltip("Number of segments (default 10 for 10% increments)")]
  [SerializeField] private int segmentCount = 10;

  [Tooltip("Spacing between segments")]
  [SerializeField] private float segmentSpacing = 5f;

  [Tooltip("Width of each segment")]
  [SerializeField] private float segmentWidth = 15f;

  [Tooltip("Height of each segment")]
  [SerializeField] private float segmentHeight = 30f;

  [Header("Visual Settings")]
  [Tooltip("Color when battery is full/high")]
  [SerializeField] private Color fullBatteryColor = Color.green;

  [Tooltip("Color when battery is medium")]
  [SerializeField] private Color mediumBatteryColor = Color.yellow;

  [Tooltip("Color when battery is low")]
  [SerializeField] private Color lowBatteryColor = Color.red;

  [Tooltip("Color for empty/inactive segments")]
  [SerializeField] private Color emptySegmentColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

  [Range(0f, 1f)]
  [Tooltip("Battery percentage threshold for medium color (0-1)")]
  [SerializeField] private float mediumBatteryThreshold = 0.5f;

  [Range(0f, 1f)]
  [Tooltip("Battery percentage threshold for low color (0-1)")]
  [SerializeField] private float lowBatteryThreshold = 0.25f;

  [Header("Blink Settings")]
  [Tooltip("Enable blinking when battery is critically low")]
  [SerializeField] private bool blinkWhenLow = true;

  [Tooltip("Battery level to start blinking (0-1)")]
  [SerializeField] private float criticalBatteryThreshold = 0.15f;

  [Tooltip("Blink speed (times per second)")]
  [SerializeField] private float blinkSpeed = 2f;

  [Range(0f, 1f)]
  [Tooltip("Minimum alpha during blink (0=invisible, 1=fully visible)")]
  [SerializeField] private float blinkMinAlpha = 0.6f;

  [Header("Segment Shape")]
  [Tooltip("Enable rounded corners for segments (requires UI shader support)")]
  [SerializeField] private bool roundedCorners = false;

  [Range(0f, 20f)]
  [Tooltip("Border radius for rounded corners (in pixels)")]
  [SerializeField] private float cornerRadius = 3f;

  [Header("Display Format")]
  [Tooltip("Format for battery text. Use {0} for percentage")]
  [SerializeField] private string batteryTextFormat = "{0:0}%";

  [Tooltip("Text color for better contrast")]
  [SerializeField] private Color textColor = Color.white;

  [Tooltip("Enable text outline for better readability")]
  [SerializeField] private bool textOutline = true;

  [Tooltip("Outline color for text")]
  [SerializeField] private Color outlineColor = Color.black;

  private List<Image> segments = new List<Image>();
  private List<Color> segmentBaseColors = new List<Color>(); // Store base colors without alpha modification
  private float currentBattery = 100f;
  private float blinkTimer;
  private float currentAlpha = 1f; // Current alpha for blink effect
  private Sprite roundedRectSprite; // Cached rounded rect sprite

  private void Awake()
  {
    InitializeDisplay();
  }

  private void Start()
  {
    ApplyTextStyling();

    // In play mode, start with full battery; in editor, use preview value
    if (Application.isPlaying)
    {
      UpdateBattery(100f);
    }
    else
    {
      UpdateBattery(previewBatteryValue);
    }
  }

  /// <summary>
  /// Called when values change in Inspector. Enables live preview without Play mode.
  /// </summary>
  private void OnValidate()
  {
    // Delay execution to avoid issues during serialization
    if (!Application.isPlaying)
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.delayCall += () =>
      {
        if (this == null) return; // Object might be destroyed
        RefreshDisplay();
      };
#endif
    }
  }

  /// <summary>
  /// Initialize or refresh the display (safe for both Editor and Play mode).
  /// </summary>
  private void InitializeDisplay()
  {
    // Skip when editing prefab asset to avoid dirty overrides
#if UNITY_EDITOR
    if (!Application.isPlaying && UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
    {
      return;
    }
#endif

    // Generate rounded rect sprite if needed
    if (roundedCorners)
    {
      roundedRectSprite = CreateRoundedRectSprite((int)segmentWidth, (int)segmentHeight, (int)cornerRadius);
    }
    CreateSegments();
  }

  /// <summary>
  /// Refresh display in Editor mode.
  /// </summary>
  private void RefreshDisplay()
  {
    if (this == null) return;

#if UNITY_EDITOR
    // Avoid touching prefab asset itself
    if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;
#endif

    // Regenerate rounded sprite if settings changed
    if (roundedCorners)
    {
      roundedRectSprite = CreateRoundedRectSprite((int)segmentWidth, (int)segmentHeight, (int)cornerRadius);
    }
    else
    {
      roundedRectSprite = null;
    }

    // Recreate segments with new settings
    CreateSegmentsEditor();
    ApplyTextStyling();
    UpdateBattery(previewBatteryValue);
  }

  /// <summary>
  /// Apply text styling.
  /// </summary>
  private void ApplyTextStyling()
  {
    if (batteryText != null)
    {
      batteryText.color = textColor;
      if (textOutline)
      {
        batteryText.outlineWidth = 0.2f;
        batteryText.outlineColor = outlineColor;
      }
    }
  }

  private void OnEnable()
  {
    // Only subscribe to events in Play mode
    if (Application.isPlaying)
    {
      // Clean any editor preview children before runtime to avoid duplicates
      CleanupContainer(false);
      CreateSegmentsInternal();
      PlayerAttributes.onBatteryUpdate += OnBatteryUpdated;
    }
  }

  private void OnDisable()
  {
    if (Application.isPlaying)
    {
      PlayerAttributes.onBatteryUpdate -= OnBatteryUpdated;
    }
  }

  private void OnBatteryUpdated(float batteryValue)
  {
    UpdateBattery(batteryValue);
  }

  /// <summary>
  /// Create segments for Editor mode (uses DestroyImmediate).
  /// </summary>
  private void CreateSegmentsEditor()
  {
#if UNITY_EDITOR
    CleanupContainer(true);
    CreateSegmentsInternal();
#endif
  }

  private void CreateSegments()
  {
    CleanupContainer(false);
    CreateSegmentsInternal();
  }

  /// <summary>
  /// Internal method to create segments (shared between Editor and Play mode).
  /// </summary>
  private void CreateSegmentsInternal()
  {
    // Ensure container exists
    if (segmentContainer == null)
    {
      GameObject container = new GameObject("BatterySegments");
      container.transform.SetParent(transform, false);
      segmentContainer = container.transform;

      // Add RectTransform for UI
      RectTransform containerRT = container.GetComponent<RectTransform>();
      if (containerRT == null) containerRT = container.AddComponent<RectTransform>();
      containerRT.anchoredPosition = Vector2.zero;

      // Setup layout
      HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
      layout.spacing = segmentSpacing;
      layout.childAlignment = TextAnchor.MiddleLeft;
      layout.childControlWidth = false;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = false;
      layout.childForceExpandHeight = false;
    }
    else
    {
      // Update spacing if container already exists
      HorizontalLayoutGroup layout = segmentContainer.GetComponent<HorizontalLayoutGroup>();
      if (layout != null)
      {
        layout.spacing = segmentSpacing;
      }
    }

    // Create segments
    for (int i = 0; i < segmentCount; i++)
    {
      Image segment;

      if (segmentPrefab != null)
      {
        segment = Instantiate(segmentPrefab, segmentContainer);
      }
      else
      {
        // Create default segment
        GameObject segmentObj = new GameObject($"Segment_{i}");
        segmentObj.transform.SetParent(segmentContainer, false);
        if (!Application.isPlaying)
        {
          // Prevent prefab overrides & scene dirtying in editor preview
          segmentObj.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontSave;
        }
        segment = segmentObj.AddComponent<Image>();
        segment.color = emptySegmentColor;

        // Add rounded corners if enabled
        if (roundedCorners && roundedRectSprite != null)
        {
          segment.sprite = roundedRectSprite;
          segment.type = Image.Type.Simple;
          segment.preserveAspect = false;
        }

        RectTransform rt = segment.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(segmentWidth, segmentHeight);
      }

      if (!Application.isPlaying)
      {
        segment.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontSave;
      }

      segments.Add(segment);
      segmentBaseColors.Add(emptySegmentColor); // Track base color
    }
  }

  /// <summary>
  /// Clears all generated children/segments to avoid prefab overrides and double instances.
  /// </summary>
  private void CleanupContainer(bool immediate)
  {
    // Destroy tracked segments
    for (int i = segments.Count - 1; i >= 0; i--)
    {
      if (segments[i] != null)
      {
        if (immediate)
          DestroyImmediate(segments[i].gameObject);
        else
          Destroy(segments[i].gameObject);
      }
    }
    segments.Clear();
    segmentBaseColors.Clear();

    // Destroy any leftover children under container
    if (segmentContainer != null)
    {
      for (int i = segmentContainer.childCount - 1; i >= 0; i--)
      {
        Transform child = segmentContainer.GetChild(i);
        if (immediate)
          DestroyImmediate(child.gameObject);
        else
          Destroy(child.gameObject);
      }
    }
  }

  private void Update()
  {
    // Handle blinking when battery is critically low (but not empty)
    float normalizedBattery = currentBattery / 100f;

    if (blinkWhenLow && normalizedBattery > 0f && normalizedBattery <= criticalBatteryThreshold)
    {
      blinkTimer += Time.deltaTime * blinkSpeed;
      // Use sine wave to fade between blinkMinAlpha and 1.0
      float alpha = Mathf.Lerp(blinkMinAlpha, 1f, (Mathf.Sin(blinkTimer * Mathf.PI * 2f) + 1f) * 0.5f);
      UpdateSegmentAlpha(alpha);
    }
    else
    {
      // Ensure full visibility when not in critical state
      UpdateSegmentAlpha(1f);
    }
  }

  private void UpdateBattery(float batteryValue)
  {
    currentBattery = Mathf.Clamp(batteryValue, 0f, 100f);
    float normalizedBattery = currentBattery / 100f;

    // Calculate how many segments should be filled
    float filledSegments = normalizedBattery * segmentCount;
    int fullSegments = Mathf.FloorToInt(filledSegments);

    // Ensure base colors list is sized correctly
    while (segmentBaseColors.Count < segments.Count)
    {
      segmentBaseColors.Add(emptySegmentColor);
    }

    // Update each segment's base color
    for (int i = 0; i < segments.Count; i++)
    {
      Color baseColor;
      if (i < fullSegments)
      {
        // Full segment
        baseColor = GetBatteryColor(normalizedBattery);
      }
      else if (i == fullSegments && filledSegments % 1f > 0f)
      {
        // Partial segment
        float partial = filledSegments % 1f;
        baseColor = partial > 0.5f ? GetBatteryColor(normalizedBattery) : emptySegmentColor;
      }
      else
      {
        // Empty segment
        baseColor = emptySegmentColor;
      }

      // Store base color
      segmentBaseColors[i] = baseColor;

      // Apply with current alpha
      Color finalColor = baseColor;
      finalColor.a *= currentAlpha;
      segments[i].color = finalColor;
    }

    // Update text
    if (batteryText != null)
    {
      batteryText.text = string.Format(batteryTextFormat, currentBattery);
    }
  }

  private void UpdateSegmentAlpha(float alpha)
  {
    currentAlpha = alpha;

    for (int i = 0; i < segments.Count; i++)
    {
      if (segments[i] != null && i < segmentBaseColors.Count)
      {
        Color col = segmentBaseColors[i];
        col.a *= alpha; // Multiply base alpha with blink alpha
        segments[i].color = col;
      }
    }

    if (batteryText != null)
    {
      Color textCol = textColor;
      textCol.a = alpha;
      batteryText.color = textCol;
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

  /// <summary>
  /// Shows or hides the battery display.
  /// </summary>
  public void SetVisible(bool visible)
  {
    gameObject.SetActive(visible);
  }

  /// <summary>
  /// Creates a procedural rounded rectangle sprite.
  /// </summary>
  private Sprite CreateRoundedRectSprite(int width, int height, int radius)
  {
    // Ensure minimum size
    width = Mathf.Max(width, radius * 2 + 2);
    height = Mathf.Max(height, radius * 2 + 2);
    radius = Mathf.Min(radius, Mathf.Min(width, height) / 2);

    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[width * height];

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float alpha = GetRoundedRectAlpha(x, y, width, height, radius);
        pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();

    return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
  }

  /// <summary>
  /// Calculate alpha for rounded rect at given pixel position.
  /// </summary>
  private float GetRoundedRectAlpha(int x, int y, int width, int height, int radius)
  {
    // Check if we're in a corner region
    int cornerX = -1, cornerY = -1;

    // Bottom-left corner
    if (x < radius && y < radius)
    {
      cornerX = radius;
      cornerY = radius;
    }
    // Bottom-right corner
    else if (x >= width - radius && y < radius)
    {
      cornerX = width - radius - 1;
      cornerY = radius;
    }
    // Top-left corner
    else if (x < radius && y >= height - radius)
    {
      cornerX = radius;
      cornerY = height - radius - 1;
    }
    // Top-right corner
    else if (x >= width - radius && y >= height - radius)
    {
      cornerX = width - radius - 1;
      cornerY = height - radius - 1;
    }

    // If in a corner, check distance from corner center
    if (cornerX >= 0)
    {
      float dist = Mathf.Sqrt((x - cornerX) * (x - cornerX) + (y - cornerY) * (y - cornerY));
      if (dist > radius)
        return 0f;
      else if (dist > radius - 1.5f)
        return 1f - (dist - (radius - 1.5f)) / 1.5f; // Anti-aliasing
      else
        return 1f;
    }

    // Not in corner - fully opaque
    return 1f;
  }
}
