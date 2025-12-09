using UnityEngine;

/// <summary>
/// Component that provides spatial (3D) audio capabilities.
/// Attach this to GameObjects that need to play positioned sounds in 3D space.
/// Automatically configures the AudioSource for spatial audio and synchronizes volume with AudioManager.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SpatialAudioProvider : MonoBehaviour
{
    [Header("Spatial Audio Settings")]
    [Tooltip("Spatial blend: 0 = 2D, 1 = 3D")]
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [Tooltip("Minimum distance for audio attenuation")]
    [SerializeField] private float minDistance = 1f;

    [Tooltip("Maximum distance for audio attenuation")]
    [SerializeField] private float maxDistance = 50f;

    public AudioSource AudioSource { get; private set; }
    private AudioManager _audioManager;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        ConfigureSpatialAudio();
    }

    private void OnEnable()
    {
        // Get AudioManager from ServiceLocator
        if (_audioManager == null)
        {
            _audioManager = ServiceLocator.Get<AudioManager>();
        }

        if (_audioManager != null)
        {
            UpdateVolume();
            _audioManager.OnVolumeChanged += UpdateVolume;
        }
        else
        {
            Debug.LogWarning("[SpatialAudioProvider] AudioManager not found in ServiceLocator");
        }
    }

    private void OnDisable()
    {
        if (_audioManager != null)
        {
            _audioManager.OnVolumeChanged -= UpdateVolume;
        }
    }

    private void ConfigureSpatialAudio()
    {
        if (AudioSource == null) return;

        // Configure for 3D spatial audio
        AudioSource.spatialBlend = spatialBlend;
        AudioSource.minDistance = minDistance;
        AudioSource.maxDistance = maxDistance;
        AudioSource.rolloffMode = AudioRolloffMode.Linear;
        AudioSource.playOnAwake = false;

    }

    private void UpdateVolume()
    {
        if (AudioSource != null && _audioManager != null)
        {
            // Use the volume calculation from AudioManager
            AudioSource.volume = _audioManager.CalculateSFXVolume(1f);
        }
    }

    /// <summary>
    /// Plays an SFX on this spatial audio source.
    /// </summary>
    public void PlaySfx(SfxClipData sfxId)
    {
        if (_audioManager != null)
        {
            _audioManager.PlaySfx(sfxId.SFXId, AudioSource);
        }
    }

    /// <summary>
    /// Plays an SFX with custom pitch on this spatial audio source.
    /// </summary>
    public void PlaySfx(SFXIdentifier sfxId, float pitch)
    {
        if (_audioManager != null)
        {
            _audioManager.PlaySfx(sfxId, AudioSource, pitch);
        }
    }

    private void OnValidate()
    {
        // Update settings when changed in inspector
        if (AudioSource != null)
        {
            ConfigureSpatialAudio();
        }
    }
}