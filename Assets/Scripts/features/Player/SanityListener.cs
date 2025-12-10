using System;
using UnityEngine;

/// <summary>
/// Defines a sanity threshold and the audio clips to play when sanity crosses that threshold.
/// Example: At 50% sanity, play heavy breathing. At 20% sanity, play heartbeat.
/// </summary>
[Serializable]
public class SanityThresholdAudioRecord
{
    [Tooltip("Sanity threshold (0-1). Audio plays when sanity goes BELOW this value")]
    [Range(0, 1)]
    public float sanityThreshold;

    [Tooltip("Audio clips to randomly choose from when threshold is crossed")]
    public SfxClipData[] sanitySfxs;

    [Tooltip("Should this audio loop continuously while below threshold?")]
    public bool shouldLoop = false;

    [Tooltip("Minimum time between audio triggers (prevents spam)")]
    public float cooldown = 5f;

    [HideInInspector]
    public float lastPlayedTime = -999f;

    [HideInInspector]
    public bool isCurrentlyActive = false;
}

/// <summary>
/// Listens to player sanity changes and plays audio based on sanity thresholds.
/// Lower sanity = more intense audio cues (breathing, heartbeat, etc.)
///
/// Setup:
/// - Add multiple SanityThresholdAudioRecord entries (e.g., 0.7, 0.5, 0.3, 0.2)
/// - Assign audio clips for each threshold
/// - Configure looping and cooldowns
///
/// Behavior:
/// - When sanity drops below a threshold, plays random audio from that threshold
/// - Supports looping audio (like continuous heavy breathing)
/// - Cooldown prevents audio spam
/// - Automatically stops looping when sanity recovers above threshold
/// </summary>
public class SanityListener : MonoBehaviour
{
    [Header("Audio Provider")]
    [SerializeField] private SpatialAudioProvider audioProvider;

    [Header("Sanity Thresholds (sorted from highest to lowest)")]
    [Tooltip("Define sanity thresholds and their corresponding audio. Sort from HIGH to LOW threshold.")]
    [SerializeField] private SanityThresholdAudioRecord[] sanityThresholds;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private SanityThresholdAudioRecord currentActiveThreshold = null;
    private float currentNormalizedSanity = 1f;

    private void OnEnable()
    {
        PlayerAttributes.onSanityUpdate += OnSanityUpdate;
    }

    private void OnDisable()
    {
        PlayerAttributes.onSanityUpdate -= OnSanityUpdate;

        // Stop any looping audio when disabled
        if (audioProvider != null)
        {
            audioProvider.AudioSource.Stop();
            audioProvider.AudioSource.loop = false;
        }
    }

    /// <summary>
    /// Called when player sanity changes (receives normalized value 0-1)
    /// </summary>
    private void OnSanityUpdate(float normalizedSanity)
    {
        currentNormalizedSanity = normalizedSanity;

        // Find the most severe threshold that has been crossed
        SanityThresholdAudioRecord newThreshold = GetActiveThreshold(normalizedSanity);

        // Check if threshold changed
        if (newThreshold != currentActiveThreshold)
        {
            // Stop previous threshold's audio if it was looping
            if (currentActiveThreshold != null && currentActiveThreshold.shouldLoop)
            {
                StopLoopingAudio();
                Log($"Stopped looping audio for threshold: {currentActiveThreshold.sanityThreshold:F2}");
            }

            // Update current threshold
            currentActiveThreshold = newThreshold;

            // Play new threshold's audio
            if (newThreshold != null)
            {
                PlayThresholdAudio(newThreshold);
            }
        }
        else if (currentActiveThreshold != null)
        {
            // Same threshold, check if we should trigger audio again (for non-looping with cooldown)
            if (!currentActiveThreshold.shouldLoop)
            {
                TryPlayThresholdAudio(currentActiveThreshold);
            }
        }
    }

    /// <summary>
    /// Get the most severe threshold that is currently active (sanity is below threshold)
    /// </summary>
    private SanityThresholdAudioRecord GetActiveThreshold(float normalizedSanity)
    {
        if (sanityThresholds == null || sanityThresholds.Length == 0)
            return null;

        // Find the LOWEST threshold that sanity is below (most severe)
        SanityThresholdAudioRecord lowestActiveThreshold = null;

        foreach (var threshold in sanityThresholds)
        {
            // Check if sanity is below this threshold
            if (normalizedSanity < threshold.sanityThreshold)
            {
                // If this is the first active threshold, or it's lower than current lowest
                if (lowestActiveThreshold == null || threshold.sanityThreshold < lowestActiveThreshold.sanityThreshold)
                {
                    lowestActiveThreshold = threshold;
                }
            }
        }

        return lowestActiveThreshold;
    }

    /// <summary>
    /// Play audio for a threshold, respecting cooldown
    /// </summary>
    private void TryPlayThresholdAudio(SanityThresholdAudioRecord threshold)
    {
        if (threshold == null || threshold.sanitySfxs == null || threshold.sanitySfxs.Length == 0)
            return;

        // Check cooldown
        if (Time.time - threshold.lastPlayedTime < threshold.cooldown)
            return;

        PlayThresholdAudio(threshold);
    }

    /// <summary>
    /// Play audio for a threshold (ignores cooldown, used when threshold first activated)
    /// </summary>
    private void PlayThresholdAudio(SanityThresholdAudioRecord threshold)
    {
        if (threshold == null || threshold.sanitySfxs == null || threshold.sanitySfxs.Length == 0)
            return;

        if (audioProvider == null || audioProvider.AudioSource == null)
        {
            Debug.LogWarning("[SanityListener] Audio provider not assigned or missing AudioSource!");
            return;
        }

        // Randomly select audio from threshold
        SfxClipData selectedAudio = threshold.sanitySfxs[UnityEngine.Random.Range(0, threshold.sanitySfxs.Length)];

        if (selectedAudio == null)
            return;

        // Update last played time
        threshold.lastPlayedTime = Time.time;

        // Play audio based on loop setting
        if (threshold.shouldLoop)
        {
            // Play looping audio
            audioProvider.AudioSource.loop = true;
            audioProvider.PlaySfx(selectedAudio);
            Log($"Playing LOOPING audio for threshold {threshold.sanityThreshold:F2} (Sanity: {currentNormalizedSanity:F2})");
        }
        else
        {
            // Play one-shot audio
            audioProvider.AudioSource.loop = false;
            audioProvider.PlaySfx(selectedAudio);
            Log($"Playing ONE-SHOT audio for threshold {threshold.sanityThreshold:F2} (Sanity: {currentNormalizedSanity:F2})");
        }

        threshold.isCurrentlyActive = true;
    }

    /// <summary>
    /// Stop any looping audio
    /// </summary>
    private void StopLoopingAudio()
    {
        if (audioProvider != null && audioProvider.AudioSource != null)
        {
            audioProvider.AudioSource.Stop();
            audioProvider.AudioSource.loop = false;
        }

        if (currentActiveThreshold != null)
        {
            currentActiveThreshold.isCurrentlyActive = false;
        }
    }

    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SanityListener] {message}", this);
        }
    }

    private void OnValidate()
    {
        // Validate thresholds are in descending order (optional warning)
        if (sanityThresholds != null && sanityThresholds.Length > 1)
        {
            for (int i = 0; i < sanityThresholds.Length - 1; i++)
            {
                if (sanityThresholds[i].sanityThreshold < sanityThresholds[i + 1].sanityThreshold)
                {
                    Debug.LogWarning($"[SanityListener] Consider sorting thresholds from highest to lowest for clarity. " +
                        $"Threshold[{i}] ({sanityThresholds[i].sanityThreshold:F2}) is lower than Threshold[{i + 1}] ({sanityThresholds[i + 1].sanityThreshold:F2})", this);
                    break;
                }
            }
        }
    }
}