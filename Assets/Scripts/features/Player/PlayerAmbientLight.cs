using UnityEngine;

/// <summary>
/// Provides a subtle ambient light around the player when the flashlight is off.
/// This allows the player to "feel" their immediate surroundings even in complete darkness.
/// </summary>
public class PlayerAmbientLight : MonoBehaviour
{
    [Header("Ambient Light Settings")]
    [Tooltip("The ambient light source (should be a Point Light with short range)")]
    [SerializeField] private Light ambientLight;

    [Tooltip("Light intensity when flashlight is OFF")]
    [SerializeField] private float ambientIntensity = 0.3f;

    [Tooltip("Light range - how far the ambient light reaches")]
    [SerializeField] private float ambientRange = 2f;

    [Tooltip("Light color for ambient glow")]
    [SerializeField] private Color ambientColor = new Color(0.7f, 0.8f, 1f, 1f); // Slight blue tint

    [Header("Flashlight Reference")]
    [Tooltip("Reference to the main flashlight to detect on/off state")]
    [SerializeField] private Light flashlight;

    [Header("Transition Settings")]
    [Tooltip("How fast the ambient light fades in/out")]
    [SerializeField] private float fadeSpeed = 5f;

    private float targetIntensity;
    private float currentIntensity;

    private void Awake()
    {
        if (ambientLight == null)
        {
            // Try to find or create ambient light
            var existingLight = transform.Find("AmbientLight");
            if (existingLight != null)
            {
                ambientLight = existingLight.GetComponent<Light>();
            }
        }

        if (ambientLight != null)
        {
            // Configure the ambient light
            ambientLight.type = LightType.Point;
            ambientLight.range = ambientRange;
            ambientLight.color = ambientColor;
            ambientLight.intensity = 0f;
            ambientLight.shadows = LightShadows.None; // No shadows for performance
            currentIntensity = 0f;
        }
    }

    private void Update()
    {
        if (ambientLight == null || flashlight == null)
            return;

        // Determine target intensity based on flashlight state
        // Ambient light is ON when flashlight is OFF
        targetIntensity = flashlight.enabled ? 0f : ambientIntensity;

        // Smoothly transition to target intensity
        if (!Mathf.Approximately(currentIntensity, targetIntensity))
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, fadeSpeed * Time.deltaTime);
            ambientLight.intensity = currentIntensity;
        }
    }

    /// <summary>
    /// Manually set the ambient light intensity multiplier (0-1)
    /// </summary>
    public void SetIntensityMultiplier(float multiplier)
    {
        ambientIntensity = Mathf.Clamp01(multiplier) * 0.5f; // Max 0.5 intensity
    }

    /// <summary>
    /// Force ambient light on/off (for cutscenes, etc.)
    /// </summary>
    public void ForceAmbientLight(bool enabled)
    {
        if (ambientLight != null)
        {
            targetIntensity = enabled ? ambientIntensity : 0f;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (ambientLight != null)
        {
            ambientLight.range = ambientRange;
            ambientLight.color = ambientColor;
        }
    }
#endif
}
