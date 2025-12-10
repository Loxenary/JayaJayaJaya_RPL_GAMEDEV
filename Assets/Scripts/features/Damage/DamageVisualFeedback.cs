using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Provides visual feedback based on player sanity level using URP post-processing.
/// Features:
/// - Persistent vignette that intensifies as sanity decreases
/// - Chromatic aberration at low sanity
/// - Film grain for stress effect
/// - Event-triggered vignette pulse (for damage/events)
///
/// Similar to games like Amnesia, Outlast, etc.
/// </summary>
[RequireComponent(typeof(Volume))]
public class DamageVisualFeedback : MonoBehaviour
{
    public struct TriggerPulseEvent
    {

    }

    public struct TriggerPulseEventWithParam
    {
        public float intensity;
        public float duration;
    }


    [Header("Volume Setup")]
    [Tooltip("The URP Volume component (auto-assigned if not set)")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Sanity-Based Effects Configuration")]
    [Tooltip("Sanity threshold ranges and their corresponding visual intensity")]
    [SerializeField]
    private SanityVisualThreshold[] sanityThresholds = new SanityVisualThreshold[]
    {
        new SanityVisualThreshold { sanityThreshold = 1.0f, vignetteIntensity = 0.0f, chromaticAberrationIntensity = 0.0f, filmGrainIntensity = 0.0f },
        new SanityVisualThreshold { sanityThreshold = 0.7f, vignetteIntensity = 0.2f, chromaticAberrationIntensity = 0.0f, filmGrainIntensity = 0.1f },
        new SanityVisualThreshold { sanityThreshold = 0.5f, vignetteIntensity = 0.35f, chromaticAberrationIntensity = 0.2f, filmGrainIntensity = 0.3f },
        new SanityVisualThreshold { sanityThreshold = 0.3f, vignetteIntensity = 0.5f, chromaticAberrationIntensity = 0.4f, filmGrainIntensity = 0.5f },
        new SanityVisualThreshold { sanityThreshold = 0.2f, vignetteIntensity = 0.65f, chromaticAberrationIntensity = 0.6f, filmGrainIntensity = 0.7f },
        new SanityVisualThreshold { sanityThreshold = 0.0f, vignetteIntensity = 0.8f, chromaticAberrationIntensity = 0.8f, filmGrainIntensity = 1.0f }
    };

    [Header("Smooth Transition")]
    [Tooltip("How fast effects transition when sanity changes")]
    [SerializeField] private float transitionSpeed = 2f;

    [Header("Event-Based Pulse (for damage/events)")]
    [Tooltip("Vignette intensity spike when damage taken")]
    [SerializeField] private float pulseSpikeIntensity = 0.4f;

    [Tooltip("Duration of the damage pulse effect")]
    [SerializeField] private float pulseDuration = 0.5f;

    [Tooltip("How fast the pulse fades out")]
    [SerializeField] private float pulseFadeSpeed = 3f;

    [Header("Vignette Color")]
    [Tooltip("Color of the vignette at low sanity")]
    [SerializeField] private Color vignetteColor = new Color(0.1f, 0.0f, 0.0f, 1f); // Dark red

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    // Post-processing effects
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;

    // Runtime state
    private float currentNormalizedSanity = 1f;
    private float targetVignetteIntensity = 0f;
    private float targetChromaticAberrationIntensity = 0f;
    private float targetFilmGrainIntensity = 0f;
    private float currentPulseIntensity = 0f;
    private bool isPulsing = false;

    private void Awake()
    {
        // Get or add Volume component
        if (postProcessVolume == null)
        {
            postProcessVolume = GetComponent<Volume>();
        }

        if (postProcessVolume == null)
        {
            Debug.LogError("[DamageVisualFeedback] No Volume component found!", this);
            enabled = false;
            return;
        }

        InitializePostProcessingEffects();
    }

    private void OnEnable()
    {
        // Subscribe to player sanity changes
        PlayerAttributes.onSanityUpdate += OnSanityChanged;
        EventBus.Subscribe<TriggerPulseEvent>(OnPulseEventTrigger);
        EventBus.Subscribe<TriggerPulseEventWithParam>(OnSpecialEvent);
    }

    private void OnDisable()
    {
        PlayerAttributes.onSanityUpdate -= OnSanityChanged;
        EventBus.Unsubscribe<TriggerPulseEvent>(OnPulseEventTrigger);
        EventBus.Unsubscribe<TriggerPulseEventWithParam>(OnSpecialEvent);
    }

    private void Update()
    {
        // Smoothly interpolate effects to target values
        UpdateEffectIntensities();


    }

    /// <summary>
    /// Initialize post-processing effects from the volume profile
    /// </summary>
    private void InitializePostProcessingEffects()
    {
        if (postProcessVolume.profile == null)
        {
            Debug.LogError("[DamageVisualFeedback] Volume profile is null! Please assign a URP profile.", this);
            enabled = false;
            return;
        }

        // Try to get existing effects or add them
        if (!postProcessVolume.profile.TryGet(out vignette))
        {
            vignette = postProcessVolume.profile.Add<Vignette>(false);
            Log("Added Vignette effect to volume profile");
        }

        if (!postProcessVolume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration = postProcessVolume.profile.Add<ChromaticAberration>(false);
            Log("Added ChromaticAberration effect to volume profile");
        }

        if (!postProcessVolume.profile.TryGet(out filmGrain))
        {
            filmGrain = postProcessVolume.profile.Add<FilmGrain>(false);
            Log("Added FilmGrain effect to volume profile");
        }

        // Enable effects
        vignette.active = true;
        chromaticAberration.active = true;
        filmGrain.active = true;

        // Set vignette color
        vignette.color.overrideState = true;
        vignette.color.value = vignetteColor;

        // Initialize intensities
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0f;

        chromaticAberration.intensity.overrideState = true;
        chromaticAberration.intensity.value = 0f;

        filmGrain.intensity.overrideState = true;
        filmGrain.intensity.value = 0f;

        Log("Post-processing effects initialized");
    }

    /// <summary>
    /// Called when player sanity changes (receives normalized value 0-1)
    /// </summary>
    private void OnSanityChanged(float normalizedSanity)
    {
        currentNormalizedSanity = normalizedSanity;

        // Find the appropriate threshold for current sanity
        UpdateTargetEffectValues();

        Log($"Sanity changed: {normalizedSanity:F2} -> Vignette target: {targetVignetteIntensity:F2}");
    }

    /// <summary>
    /// Calculate target effect values based on current sanity
    /// </summary>
    private void UpdateTargetEffectValues()
    {
        // Find the two thresholds to interpolate between
        SanityVisualThreshold lowerThreshold = sanityThresholds[sanityThresholds.Length - 1];
        SanityVisualThreshold upperThreshold = sanityThresholds[0];

        for (int i = 0; i < sanityThresholds.Length - 1; i++)
        {
            if (currentNormalizedSanity <= sanityThresholds[i].sanityThreshold &&
                currentNormalizedSanity > sanityThresholds[i + 1].sanityThreshold)
            {
                upperThreshold = sanityThresholds[i];
                lowerThreshold = sanityThresholds[i + 1];
                break;
            }
        }

        // Interpolate between thresholds
        float t = Mathf.InverseLerp(lowerThreshold.sanityThreshold, upperThreshold.sanityThreshold, currentNormalizedSanity);

        targetVignetteIntensity = Mathf.Lerp(lowerThreshold.vignetteIntensity, upperThreshold.vignetteIntensity, t);
        targetChromaticAberrationIntensity = Mathf.Lerp(lowerThreshold.chromaticAberrationIntensity, upperThreshold.chromaticAberrationIntensity, t);
        targetFilmGrainIntensity = Mathf.Lerp(lowerThreshold.filmGrainIntensity, upperThreshold.filmGrainIntensity, t);
    }

    /// <summary>
    /// Smoothly update effect intensities to target values
    /// </summary>
    private void UpdateEffectIntensities()
    {
        // Combine base intensity with pulse
        float finalVignetteIntensity = targetVignetteIntensity + currentPulseIntensity;

        // Smoothly lerp to target
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, finalVignetteIntensity, Time.deltaTime * transitionSpeed);
        chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, targetChromaticAberrationIntensity, Time.deltaTime * transitionSpeed);
        filmGrain.intensity.value = Mathf.Lerp(filmGrain.intensity.value, targetFilmGrainIntensity, Time.deltaTime * transitionSpeed);
    }

    /// <summary>
    /// Trigger a vignette pulse (for damage or special events)
    /// </summary>
    public void TriggerPulse()
    {
        TriggerPulse(pulseSpikeIntensity, pulseDuration);
    }

    /// <summary>
    /// Trigger a vignette pulse with custom intensity and duration
    /// </summary>
    public void TriggerPulse(float intensity, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(PulseCoroutine(intensity, duration));
        Log($"Triggered pulse: intensity={intensity:F2}, duration={duration:F2}");
    }

    /// <summary>
    /// Coroutine to handle pulse effect
    /// </summary>
    private IEnumerator PulseCoroutine(float intensity, float duration)
    {
        isPulsing = true;
        currentPulseIntensity = intensity;

        // Hold the pulse for the duration
        yield return new WaitForSeconds(duration);

        // Fade out the pulse
        while (currentPulseIntensity > 0.01f)
        {
            currentPulseIntensity = Mathf.Lerp(currentPulseIntensity, 0f, Time.deltaTime * pulseFadeSpeed);
            yield return null;
        }

        currentPulseIntensity = 0f;
        isPulsing = false;
    }

    /// <summary>
    /// Public method to trigger pulse via event
    /// </summary>
    private void OnPulseEventTrigger(TriggerPulseEvent evt)
    {
        TriggerPulse();
    }

    /// <summary>
    /// Trigger a custom pulse for special events
    /// </summary>
    public void OnSpecialEvent(TriggerPulseEventWithParam param)
    {
        TriggerPulse(param.intensity, param.duration);
    }

    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[DamageVisualFeedback] {message}", this);
        }
    }

    private void OnValidate()
    {
        // Ensure thresholds are sorted in descending order
        if (sanityThresholds != null && sanityThresholds.Length > 1)
        {
            for (int i = 0; i < sanityThresholds.Length - 1; i++)
            {
                if (sanityThresholds[i].sanityThreshold < sanityThresholds[i + 1].sanityThreshold)
                {
                    Debug.LogWarning($"[DamageVisualFeedback] Sanity thresholds should be sorted from highest (1.0) to lowest (0.0). " +
                        $"Threshold[{i}] ({sanityThresholds[i].sanityThreshold:F2}) is lower than Threshold[{i + 1}] ({sanityThresholds[i + 1].sanityThreshold:F2})", this);
                    break;
                }
            }
        }
    }
}

/// <summary>
/// Defines visual effect intensities for a specific sanity threshold
/// </summary>
[System.Serializable]
public class SanityVisualThreshold
{
    [Tooltip("Sanity level (0-1). Effects apply when sanity is BELOW this value")]
    [Range(0f, 1f)]
    public float sanityThreshold = 1f;

    [Tooltip("Vignette intensity at this sanity level (0-1)")]
    [Range(0f, 1f)]
    public float vignetteIntensity = 0f;

    [Tooltip("Chromatic aberration intensity at this sanity level (0-1)")]
    [Range(0f, 1f)]
    public float chromaticAberrationIntensity = 0f;

    [Tooltip("Film grain intensity at this sanity level (0-1)")]
    [Range(0f, 1f)]
    public float filmGrainIntensity = 0f;
}