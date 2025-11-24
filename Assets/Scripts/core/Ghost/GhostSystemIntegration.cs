using UnityEngine;

/// <summary>
/// Example integration script yang menghubungkan ghost system dengan game systems lain
/// Contoh: lighting, audio, UI, checkpoints, dll
/// </summary>
public class GhostSystemIntegration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSanity playerSanity;
    [SerializeField] private GhostAI ghostAI;

    [Header("UI Integration (Optional)")]
    [SerializeField] private UnityEngine.UI.Slider sanityBar;
    [SerializeField] private UnityEngine.UI.Text sanityText;
    [SerializeField] private UnityEngine.UI.Text ghostStateText;

    [Header("Audio Integration (Optional)")]
    [SerializeField] private AudioSource ghostAmbientSound;
    [SerializeField] private AudioSource chaseMusic;
    [SerializeField] private AudioClip detectSound;
    [SerializeField] private AudioClip attackSound;

    [Header("Visual Effects (Optional)")]
    [SerializeField] private Light environmentLight;
    [SerializeField] private float highSanityLightIntensity = 1f;
    [SerializeField] private float lowSanityLightIntensity = 0.3f;
    [SerializeField] private ParticleSystem sanityLowEffect;

    [Header("Camera Effects")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private bool enableScreenDistortion = true;

    private Material screenDistortionMaterial;
    private float currentVignetteIntensity = 0f;

    private void Start()
    {
        // Auto-find references jika tidak diset
        if (playerSanity == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerSanity = player.GetComponent<PlayerSanity>();
            }
        }

        if (ghostAI == null)
        {
            ghostAI = FindObjectOfType<GhostAI>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Subscribe to events
        if (playerSanity != null)
        {
            playerSanity.OnSanityChanged += OnSanityChanged;
            playerSanity.OnSanityLevelChanged += OnSanityLevelChanged;
            playerSanity.OnSanityDepleted += OnSanityDepleted;
        }

        if (ghostAI != null)
        {
            ghostAI.OnStateChanged += OnGhostStateChanged;
            ghostAI.OnPlayerDetected += OnGhostDetectedPlayer;
            ghostAI.OnPlayerLost += OnGhostLostPlayer;
        }
    }

    private void Update()
    {
        UpdateVisualEffects();
    }

    // ============================================
    // SANITY EVENT HANDLERS
    // ============================================

    private void OnSanityChanged(float newSanity)
    {
        // Update UI
        if (sanityBar != null)
        {
            sanityBar.value = newSanity / playerSanity.MaxSanity;
        }

        if (sanityText != null)
        {
            sanityText.text = $"Sanity: {newSanity:F0}/{playerSanity.MaxSanity}";
        }
    }

    private void OnSanityLevelChanged(PlayerSanity.SanityLevel newLevel)
    {
        Debug.Log($"[Integration] Sanity level changed to: {newLevel}");

        // Update lighting based on sanity level
        UpdateEnvironmentLighting(newLevel);

        // Play audio feedback
        PlaySanityLevelAudio(newLevel);

        // Show/hide effects
        if (sanityLowEffect != null)
        {
            if (newLevel == PlayerSanity.SanityLevel.Low ||
                newLevel == PlayerSanity.SanityLevel.Critical)
            {
                if (!sanityLowEffect.isPlaying)
                    sanityLowEffect.Play();
            }
            else
            {
                if (sanityLowEffect.isPlaying)
                    sanityLowEffect.Stop();
            }
        }
    }

    private void OnSanityDepleted()
    {
        Debug.Log("[Integration] Sanity depleted! Player in extreme danger!");

        // Trigger extreme visual effects
        if (playerCamera != null)
        {
            // Heavy screen shake, distortion, etc
            StartCoroutine(ScreenShakeCoroutine(0.5f, 0.3f));
        }

        // Play audio cue
        // ... play sound
    }

    // ============================================
    // GHOST EVENT HANDLERS
    // ============================================

    private void OnGhostStateChanged(GhostAI.GhostState newState)
    {
        Debug.Log($"[Integration] Ghost state: {newState}");

        // Update UI
        if (ghostStateText != null)
        {
            ghostStateText.text = $"Ghost: {newState}";
        }

        // Handle audio based on state
        switch (newState)
        {
            case GhostAI.GhostState.Idle:
            case GhostAI.GhostState.Patrol:
                // Play ambient sound
                if (ghostAmbientSound != null && !ghostAmbientSound.isPlaying)
                {
                    ghostAmbientSound.Play();
                }
                // Stop chase music
                if (chaseMusic != null && chaseMusic.isPlaying)
                {
                    chaseMusic.Stop();
                }
                break;

            case GhostAI.GhostState.Chase:
                // Play chase music
                if (chaseMusic != null && !chaseMusic.isPlaying)
                {
                    chaseMusic.Play();
                }
                break;

            case GhostAI.GhostState.Attack:
                // Play attack sound
                if (ghostAmbientSound != null && attackSound != null)
                {
                    ghostAmbientSound.PlayOneShot(attackSound);
                }
                break;
        }
    }

    private void OnGhostDetectedPlayer()
    {
        Debug.Log("[Integration] Ghost detected player!");

        // Play detection sound
        if (ghostAmbientSound != null && detectSound != null)
        {
            ghostAmbientSound.PlayOneShot(detectSound);
        }

        // Decrease sanity (scare effect)
        if (playerSanity != null)
        {
            playerSanity.OnGhostSeen(10f);
        }

        // Camera shake
        if (playerCamera != null)
        {
            StartCoroutine(ScreenShakeCoroutine(0.2f, 0.1f));
        }
    }

    private void OnGhostLostPlayer()
    {
        Debug.Log("[Integration] Ghost lost player");

        // Fade out chase music
        if (chaseMusic != null)
        {
            StartCoroutine(FadeOutAudioCoroutine(chaseMusic, 2f));
        }
    }

    // ============================================
    // VISUAL EFFECTS
    // ============================================

    private void UpdateVisualEffects()
    {
        if (playerSanity == null) return;

        // Update vignette/screen distortion based on sanity
        if (enableScreenDistortion)
        {
            float targetVignette = 1f - playerSanity.SanityPercentage;
            currentVignetteIntensity = Mathf.Lerp(
                currentVignetteIntensity,
                targetVignette,
                Time.deltaTime
            );

            // Apply post-processing effects here if available
            // Example: PostProcessVolume.weight = currentVignetteIntensity;
        }
    }

    private void UpdateEnvironmentLighting(PlayerSanity.SanityLevel level)
    {
        if (environmentLight == null) return;

        float targetIntensity = highSanityLightIntensity;

        switch (level)
        {
            case PlayerSanity.SanityLevel.High:
                targetIntensity = highSanityLightIntensity;
                break;
            case PlayerSanity.SanityLevel.Medium:
                targetIntensity = Mathf.Lerp(highSanityLightIntensity, lowSanityLightIntensity, 0.3f);
                break;
            case PlayerSanity.SanityLevel.Low:
                targetIntensity = Mathf.Lerp(highSanityLightIntensity, lowSanityLightIntensity, 0.7f);
                break;
            case PlayerSanity.SanityLevel.Critical:
                targetIntensity = lowSanityLightIntensity;
                break;
        }

        StartCoroutine(LerpLightIntensity(environmentLight, targetIntensity, 1f));
    }

    private void PlaySanityLevelAudio(PlayerSanity.SanityLevel level)
    {
        // Play audio cues based on sanity level
        // Implementation depends on your audio system
    }

    // ============================================
    // UTILITY COROUTINES
    // ============================================

    private System.Collections.IEnumerator ScreenShakeCoroutine(float duration, float magnitude)
    {
        if (playerCamera == null) yield break;

        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    private System.Collections.IEnumerator LerpLightIntensity(Light light, float target, float duration)
    {
        float startIntensity = light.intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            light.intensity = Mathf.Lerp(startIntensity, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        light.intensity = target;
    }

    private System.Collections.IEnumerator FadeOutAudioCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerSanity != null)
        {
            playerSanity.OnSanityChanged -= OnSanityChanged;
            playerSanity.OnSanityLevelChanged -= OnSanityLevelChanged;
            playerSanity.OnSanityDepleted -= OnSanityDepleted;
        }

        if (ghostAI != null)
        {
            ghostAI.OnStateChanged -= OnGhostStateChanged;
            ghostAI.OnPlayerDetected -= OnGhostDetectedPlayer;
            ghostAI.OnPlayerLost -= OnGhostLostPlayer;
        }
    }
}
