using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generates a procedural ring/circle crosshair texture at runtime.
/// No external assets needed for the default crosshair.
/// </summary>
[ExecuteAlways] // Allow preview in Editor without Play mode
[RequireComponent(typeof(Image))]
public class ProceduralRingCrosshair : MonoBehaviour
{
  [Header("Ring Settings")]
  [Tooltip("Outer radius of the ring in pixels")]
  [SerializeField] private int outerRadius = 12;

  [Tooltip("Inner radius (hole) in pixels")]
  [SerializeField] private int innerRadius = 6;

  [Tooltip("Texture resolution (power of 2 recommended)")]
  [SerializeField] private int textureSize = 64;

  [Tooltip("Anti-aliasing amount (softness of edges)")]
  [Range(0f, 4f)]
  [SerializeField] private float antiAliasing = 1.5f;

  [Header("Visual")]
  [Tooltip("Ring color")]
  [SerializeField] private Color ringColor = new Color(1f, 1f, 1f, 0.5f);

  private Image image;
  private Texture2D generatedTexture;

  private void Awake()
  {
    image = GetComponent<Image>();
    GenerateRingTexture();
  }

  private void OnValidate()
  {
    // Regenerate in editor when values change (preview without play mode)
    if (image == null)
      image = GetComponent<Image>();

    GenerateRingTexture();
  }

  private void OnDestroy()
  {
    // Clean up generated texture
    if (generatedTexture != null)
    {
      Destroy(generatedTexture);
    }
  }

  /// <summary>
  /// Generate a ring texture with anti-aliasing
  /// </summary>
  public void GenerateRingTexture()
  {
    // Clean up old texture
    if (generatedTexture != null)
    {
      if (Application.isPlaying)
        Destroy(generatedTexture);
      else
        DestroyImmediate(generatedTexture);
    }

    generatedTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
    generatedTexture.filterMode = FilterMode.Bilinear;

    float center = textureSize / 2f;
    float outerRadiusNorm = (float)outerRadius / textureSize * textureSize;
    float innerRadiusNorm = (float)innerRadius / textureSize * textureSize;

    Color[] pixels = new Color[textureSize * textureSize];

    for (int y = 0; y < textureSize; y++)
    {
      for (int x = 0; x < textureSize; x++)
      {
        float dx = x - center;
        float dy = y - center;
        float distance = Mathf.Sqrt(dx * dx + dy * dy);

        // Calculate alpha based on distance with anti-aliasing
        float outerAlpha = Mathf.Clamp01((outerRadiusNorm - distance) / antiAliasing);
        float innerAlpha = Mathf.Clamp01((distance - innerRadiusNorm) / antiAliasing);
        float alpha = outerAlpha * innerAlpha;

        Color pixelColor = ringColor;
        pixelColor.a = ringColor.a * alpha;

        pixels[y * textureSize + x] = pixelColor;
      }
    }

    generatedTexture.SetPixels(pixels);
    generatedTexture.Apply();

    // Create sprite from texture
    Sprite ringSprite = Sprite.Create(
        generatedTexture,
        new Rect(0, 0, textureSize, textureSize),
        new Vector2(0.5f, 0.5f),
        100f
    );

    image.sprite = ringSprite;
    image.color = Color.white; // Color is baked into the texture
  }

  /// <summary>
  /// Update ring parameters at runtime
  /// </summary>
  public void SetRingParameters(int outer, int inner, Color color)
  {
    outerRadius = outer;
    innerRadius = inner;
    ringColor = color;
    GenerateRingTexture();
  }
}
