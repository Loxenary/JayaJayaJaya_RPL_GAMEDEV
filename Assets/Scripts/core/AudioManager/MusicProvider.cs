using UnityEngine;

public class MusicProvider : MonoBehaviour
{

    public AudioSource AudioSource { get; private set; }

    [Header("Audio Settings")]
    private AudioManager _audioManager;

    [Header("Music Settings")]
    [SerializeField] private MusicClipData _musicClipData;

    [SerializeField] private bool _playOnAwake = false;


    private void Awake()
    {
        AudioSource ??= GetComponent<AudioSource>();
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
        AudioSource.playOnAwake = _playOnAwake;
    }

    private void UpdateVolume()
    {
        if (AudioSource != null && _audioManager != null)
        {
            // Use the volume calculation from AudioManager
            AudioSource.volume = _audioManager.CalculateMusicVolume(_musicClipData ? _musicClipData.Volume : 1);
        }
    }

    /// <summary>
    /// Plays an SFX on this spatial audio source.
    /// </summary>
    public void PlayMusic(MusicClipData musicClipData)
    {
        if (_audioManager != null)
        {
            AudioSource.clip = musicClipData.Clip;
            AudioSource.Play();
        }
    }
}