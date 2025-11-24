// File: AudioManager.cs
using System;
using System.Threading.Tasks;
using CustomLogger;
using UnityEngine;

/// <summary>
/// Orchestrates all audio systems with proper separation of concerns.
/// Provides a clean facade for audio playback while delegating to specialized services.
/// Supports both 2D (global) and 3D (spatial) audio.
/// </summary>
public class AudioManager : InitializableServiceBase<AudioManager>
{
    [Header("Audio Sources - 2D/Global")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [SerializeField] private float saveDebounceTime = 3f;

    // Core systems (dependency injection)
    private AudioVolumeController _volumeController;
    private AudioDataRegistry _dataRegistry;
    private AudioSettingsManager _settingsManager;
    private GlobalAudioPlayback _globalPlayback;
    private SpatialAudioPlayback _spatialPlayback;
    private AudioSource _musicOneShotSource;

    // Public events 
    public event Action OnVolumeChangeEvent;
    public event Action OnVolumeChanged
    {
        add => OnVolumeChangeEvent += value;
        remove => OnVolumeChangeEvent -= value;
    }

    public override ServicePriority InitializationPriority => ServicePriority.PRIMARY;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        // Ensure required audio sources exist
        EnsureAudioSources();

        // Initialize core systems with dependency injection
        InitializeSystems();
    }

    private void Start()
    {
        InitOneShotSource();
    }

    #endregion

    #region Initialization

    private void EnsureAudioSources()
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (introSource == null) introSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        introSource.loop = false;
        introSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
    }

    private void InitOneShotSource()
    {
        var go = new GameObject("MusicOneShotSource");
        go.transform.SetParent(transform, worldPositionStays: false);

        _musicOneShotSource = go.AddComponent<AudioSource>();
        _musicOneShotSource.playOnAwake = false;
        _musicOneShotSource.loop = false;
        _musicOneShotSource.outputAudioMixerGroup = musicSource.outputAudioMixerGroup;

        // Update playback service with the one-shot source
        if (_globalPlayback != null)
        {
            _globalPlayback.SetMusicOneShotSource(_musicOneShotSource);
        }
    }

    private void InitializeSystems()
    {
        //  Initialize volume controller
        _volumeController = new AudioVolumeController();
        _volumeController.OnVolumeChanged += HandleVolumeChanged;

        //  Initialize data registry and load audio data
        _dataRegistry = new AudioDataRegistry();
        _dataRegistry.LoadAllAudioData();

        //  Initialize settings manager
        _settingsManager = new AudioSettingsManager(_volumeController, saveDebounceTime);
        _settingsManager.SetCoroutineRunner(this);

        //  Initialize playback services (dependency injection)
        _globalPlayback = new GlobalAudioPlayback(
            musicSource,
            introSource,
            sfxSource,
            null, // Will be set in Start()
            _volumeController,
            _dataRegistry,
            this
        );

        _spatialPlayback = new SpatialAudioPlayback(_volumeController, _dataRegistry);
    }

    public override async Task Initialize()
    {
        try
        {
            // Load saved audio settings
            await _settingsManager.LoadSettingsAsync();
            BetterLogger.Log("[AudioManager] Initialization complete", BetterLogger.LogCategory.System);
        }
        catch (Exception e)
        {
            BetterLogger.LogError($"[AudioManager] Initialization failed: {e.Message}", BetterLogger.LogCategory.System);
        }
    }

    private void HandleVolumeChanged()
    {
        OnVolumeChangeEvent?.Invoke();
        _settingsManager.ScheduleSave();
    }

    #endregion

    #region Volume Control - Public API

    public float GetMasterVolume() => _volumeController.MasterVolume;
    public float GetMusicVolume() => _volumeController.MusicVolume;
    public float GetSFXVolume() => _volumeController.SFXVolume;

    // Legacy methods for backward compatibility
    public float GetMusicMasterVolume() => GetMusicVolume();
    public float GetSFXMasterVolume() => GetSFXVolume();

    public void SetMasterVolume(float volume) => _volumeController.MasterVolume = volume;
    public void SetMusicVolume(float volume) => _volumeController.MusicVolume = volume;
    public void SetSFXVolume(float volume) => _volumeController.SFXVolume = volume;

    /// <summary>
    /// Calculates final SFX volume with all multipliers (for backward compatibility).
    /// </summary>
    public float GetSFXVolume(float clipVolume) => _volumeController.CalculateSFXVolume(clipVolume);

    /// <summary>
    /// Calculates final music volume with all multipliers (for backward compatibility).
    /// </summary>
    public float GetMusicVolume(float clipVolume) => _volumeController.CalculateMusicVolume(clipVolume);

    /// <summary>
    /// Calculates final SFX volume with all multipliers.
    /// </summary>
    public float CalculateSFXVolume(float clipVolume) => _volumeController.CalculateSFXVolume(clipVolume);

    /// <summary>
    /// Calculates final music volume with all multipliers.
    /// </summary>
    public float CalculateMusicVolume(float clipVolume) => _volumeController.CalculateMusicVolume(clipVolume);

    #endregion

    #region 2D/Global Audio Playback - Public API

    public void PlaySfx(SFXIdentifier sfxId) => _globalPlayback.PlaySfx(sfxId);
    public void PlaySfx(SFXIdentifier sfxId, float pitch) => _globalPlayback.PlaySfx(sfxId, pitch);
    public void PlayIncreasingPitchSFX(SFXIdentifier sfxId) => _globalPlayback.PlayIncreasingPitchSFX(sfxId);
    public void PlayRandomSfx(params SFXIdentifier[] choices) => _globalPlayback.PlayRandomSfx(choices);

    public void PlayMusic(MusicIdentifier musicId) => _globalPlayback.PlayMusic(musicId);
    public void PlayMusicOneShot(MusicIdentifier musicId, float pitch = 1f) => _globalPlayback.PlayMusicOneShot(musicId, pitch);
    public void PlayMusicWithIntro(MusicIdentifier introId, MusicIdentifier loopId) => _globalPlayback.PlayMusicWithIntro(introId, loopId);

    // Legacy method name
    public void PlayScheduled(MusicIdentifier introId, MusicIdentifier loopId, float gapAfterIntro = 0.0f, float leadTime = 0.12f)
    {
        _globalPlayback.PlayMusicWithIntro(introId, loopId);
    }

    public void StopAllSfx() => _globalPlayback.StopAllSfx();
    public void StopMusic() => _globalPlayback.StopMusic();
    public void StopOneShotMusic() => _globalPlayback.StopOneShotMusic();

    public AudioClip GetCurrentMusic() => musicSource != null ? musicSource.clip : null;

    #endregion

    #region 3D/Spatial Audio Playback - Public API

    /// <summary>
    /// Plays SFX at a specific 3D position (spatial audio).
    /// </summary>
    public void PlaySfxAtPosition(SFXIdentifier sfxId, Vector3 position)
    {
        _spatialPlayback.PlaySfxAtPosition(sfxId, position);
    }

    /// <summary>
    /// Plays SFX on a specific AudioSource (for spatial audio).
    /// The AudioSource should have spatial blend enabled.
    /// </summary>
    public void PlaySfx(SFXIdentifier sfxId, AudioSource audioSource)
    {
        _spatialPlayback.PlaySfxOnSource(sfxId, audioSource);
    }

    /// <summary>
    /// Plays SFX on a specific AudioSource with custom pitch (for spatial audio).
    /// </summary>
    public void PlaySfx(SFXIdentifier sfxId, AudioSource audioSource, float pitch)
    {
        _spatialPlayback.PlaySfxOnSource(sfxId, audioSource, pitch);
    }

    #endregion

    #region Advanced Features

    /// <summary>
    /// Manually saves audio settings immediately.
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        await _settingsManager.SaveSettingsAsync();
    }

    /// <summary>
    /// Reloads all audio clip data from Resources.
    /// </summary>
    public void ReloadAudioData()
    {
        _dataRegistry.LoadAllAudioData();
    }

    #endregion

    #region Dependency Injection Helpers (for testing)

    /// <summary>
    /// Allows injecting dependencies for testing. Call before Awake().
    /// </summary>
    public void InjectDependencies(
        AudioVolumeController volumeController,
        AudioDataRegistry dataRegistry,
        AudioSettingsManager settingsManager)
    {
        _volumeController = volumeController;
        _dataRegistry = dataRegistry;
        _settingsManager = settingsManager;
    }



    #endregion
}
