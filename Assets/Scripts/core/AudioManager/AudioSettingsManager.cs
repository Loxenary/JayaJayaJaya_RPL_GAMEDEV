// File: AudioSettingsManager.cs
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages saving and loading of audio settings.
/// Handles debounced saving to avoid excessive file writes.
/// </summary>
public class AudioSettingsManager
{
    private readonly AudioVolumeController _volumeController;
    private readonly float _saveDebounceTime;
    private MonoBehaviour _coroutineRunner;

    private Coroutine _saveDebounceCoroutine;
    private bool _isSaveScheduled = false;

    public AudioSettingsManager(AudioVolumeController volumeController, float saveDebounceTime = 3f)
    {
        _volumeController = volumeController ?? throw new ArgumentNullException(nameof(volumeController));
        _saveDebounceTime = saveDebounceTime;
    }

    /// <summary>
    /// Sets the MonoBehaviour used for running coroutines.
    /// Must be called before using save debouncing.
    /// </summary>
    public void SetCoroutineRunner(MonoBehaviour runner)
    {
        _coroutineRunner = runner;
    }

    /// <summary>
    /// Loads audio settings from persistent storage.
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        try
        {
            AudioSaveData saveData = await SaveLoadManager.LoadAsync<AudioSaveData>();

            if (saveData != null)
            {
                _volumeController.SetAllVolumes(
                    saveData.SoundMasterVolume,
                    saveData.MusicMasterVolume,
                    saveData.SFXMasterVolume,
                    invokeEvent: true
                );

                Debug.Log("[AudioSettingsManager] Settings loaded successfully");
            }
            else
            {
                Debug.LogWarning("[AudioSettingsManager] No saved audio data found. Using defaults.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AudioSettingsManager] Failed to load settings: {e.Message}");
        }
    }

    /// <summary>
    /// Saves audio settings immediately.
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        var (master, music, sfx) = _volumeController.GetAllVolumes();

        var saveData = new AudioSaveData
        {
            SoundMasterVolume = master,
            MusicMasterVolume = music,
            SFXMasterVolume = sfx
        };

        try
        {
            await SaveLoadManager.SaveAsync(saveData);
            Debug.Log("[AudioSettingsManager] Settings saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AudioSettingsManager] Failed to save settings: {e.Message}");
        }
    }

    /// <summary>
    /// Schedules a save with debouncing to avoid excessive writes.
    /// Multiple calls within the debounce time will be collapsed into one save.
    /// </summary>
    public void ScheduleSave()
    {
        if (_coroutineRunner == null)
        {
            Debug.LogWarning("[AudioSettingsManager] Coroutine runner not set. Saving immediately.");
            _ = SaveSettingsAsync();
            return;
        }

        if (_saveDebounceCoroutine != null)
        {
            _coroutineRunner.StopCoroutine(_saveDebounceCoroutine);
        }

        _saveDebounceCoroutine = _coroutineRunner.StartCoroutine(SaveDebounceCoroutine());
    }

    private IEnumerator SaveDebounceCoroutine()
    {
        _isSaveScheduled = true;
        yield return new WaitForSeconds(_saveDebounceTime);

        _isSaveScheduled = false;
        _saveDebounceCoroutine = null;

        _ = SaveSettingsAsync();
    }

    /// <summary>
    /// Returns true if a save is currently scheduled but not yet executed.
    /// </summary>
    public bool IsSaveScheduled => _isSaveScheduled;
}
