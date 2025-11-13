// File: AudioVolumeController.cs
using System;
using UnityEngine;

/// <summary>
/// Manages all audio volume calculations and settings.
/// Handles master, music, and SFX volumes independently.
/// </summary>
public class AudioVolumeController
{
    public event Action OnVolumeChanged;

    private float _masterVolume = 1f;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            OnVolumeChanged?.Invoke();
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            OnVolumeChanged?.Invoke();
        }
    }

    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            OnVolumeChanged?.Invoke();
        }
    }

    /// <summary>
    /// Calculates the final SFX volume with all multipliers applied.
    /// </summary>
    /// <param name="clipVolume">Volume of the individual audio clip (0-1)</param>
    /// <returns>Final calculated volume (0-1)</returns>
    public float CalculateSFXVolume(float clipVolume)
    {
        return clipVolume * _sfxVolume * _masterVolume;
    }

    /// <summary>
    /// Calculates the final music volume with all multipliers applied.
    /// </summary>
    /// <param name="clipVolume">Volume of the individual music clip (0-1)</param>
    /// <returns>Final calculated volume (0-1)</returns>
    public float CalculateMusicVolume(float clipVolume)
    {
        return clipVolume * _musicVolume * _masterVolume;
    }

    /// <summary>
    /// Sets all volumes at once (useful when loading saved settings).
    /// </summary>
    public void SetAllVolumes(float master, float music, float sfx, bool invokeEvent = true)
    {
        _masterVolume = Mathf.Clamp01(master);
        _musicVolume = Mathf.Clamp01(music);
        _sfxVolume = Mathf.Clamp01(sfx);

        if (invokeEvent)
        {
            OnVolumeChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets all current volume settings.
    /// </summary>
    public (float master, float music, float sfx) GetAllVolumes()
    {
        return (_masterVolume, _musicVolume, _sfxVolume);
    }
}
