
using UnityEngine;

/// <summary>
/// Handles 3D spatial audio playback.
/// Sounds are positioned in 3D space and attenuate based on distance.
/// This service works with AudioSource components that have spatial blend enabled.
/// </summary>
public class SpatialAudioPlayback : IAudioPlaybackService
{
    private readonly AudioVolumeController _volumeController;
    private readonly AudioDataRegistry _dataRegistry;

    public SpatialAudioPlayback(AudioVolumeController volumeController, AudioDataRegistry dataRegistry)
    {
        _volumeController = volumeController;
        _dataRegistry = dataRegistry;
    }

    /// <summary>
    /// Plays spatial SFX at a specific position in 3D space.
    /// </summary>
    public void PlaySfxAtPosition(SFXIdentifier sfxId, Vector3 position)
    {
        var clipData = _dataRegistry.GetSFXData(sfxId);
        if (clipData == null || clipData.Clip == null)
        {
            Debug.LogWarning($"[SpatialAudioPlayback] SFX not found: {sfxId}");
            return;
        }

        float volume = _volumeController.CalculateSFXVolume(clipData.Volume);
        AudioSource.PlayClipAtPoint(clipData.Clip, position, volume);
    }

    /// <summary>
    /// Plays SFX on a specific AudioSource (should be configured for spatial audio).
    /// </summary>
    public void PlaySfxOnSource(SFXIdentifier sfxId, AudioSource audioSource)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[SpatialAudioPlayback] AudioSource is null");
            return;
        }

        var clipData = _dataRegistry.GetSFXData(sfxId);
        if (clipData == null || clipData.Clip == null)
        {
            Debug.LogWarning($"[SpatialAudioPlayback] SFX not found: {sfxId}");
            return;
        }

        float volume = _volumeController.CalculateSFXVolume(clipData.Volume);
        audioSource.pitch = clipData.Pitch;
        audioSource.PlayOneShot(clipData.Clip, volume);

        if (clipData.Delay > 0)
        {
            audioSource.PlayDelayed(clipData.Delay);
        }
    }

    /// <summary>
    /// Plays SFX on a specific AudioSource with custom pitch.
    /// </summary>
    public void PlaySfxOnSource(SFXIdentifier sfxId, AudioSource audioSource, float pitch)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[SpatialAudioPlayback] AudioSource is null");
            return;
        }

        var clipData = _dataRegistry.GetSFXData(sfxId);
        if (clipData == null || clipData.Clip == null)
        {
            Debug.LogWarning($"[SpatialAudioPlayback] SFX not found: {sfxId}");
            return;
        }

        float volume = _volumeController.CalculateSFXVolume(clipData.Volume);
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clipData.Clip, volume);
    }

    // IAudioPlaybackService implementation (delegates to global for now, can be extended)
    public void PlaySfx(SFXIdentifier sfxId)
    {
        Debug.LogWarning("[SpatialAudioPlayback] PlaySfx without position/source not supported for spatial audio. Use PlaySfxAtPosition or PlaySfxOnSource instead.");
    }

    public void PlaySfx(SFXIdentifier sfxId, float pitch)
    {
        Debug.LogWarning("[SpatialAudioPlayback] PlaySfx without position/source not supported for spatial audio. Use PlaySfxOnSource with pitch instead.");
    }

    public void PlayIncreasingPitchSFX(SFXIdentifier sfxId)
    {
        Debug.LogWarning("[SpatialAudioPlayback] Increasing pitch SFX not supported for spatial audio.");
    }

    public void PlayRandomSfx(params SFXIdentifier[] choices)
    {
        Debug.LogWarning("[SpatialAudioPlayback] Random SFX without position/source not supported for spatial audio.");
    }

    public void PlayMusic(MusicIdentifier musicId)
    {
        Debug.LogWarning("[SpatialAudioPlayback] Music playback not typically used with spatial audio. Use GlobalAudioPlayback instead.");
    }

    public void PlayMusicOneShot(MusicIdentifier musicId, float pitch = 1f)
    {
        Debug.LogWarning("[SpatialAudioPlayback] Music one-shot not typically used with spatial audio. Use GlobalAudioPlayback instead.");
    }

    public void PlayMusicWithIntro(MusicIdentifier introId, MusicIdentifier loopId)
    {
        Debug.LogWarning("[SpatialAudioPlayback] Music with intro not supported for spatial audio. Use GlobalAudioPlayback instead.");
    }

    public void StopAllSfx()
    {
        Debug.LogWarning("[SpatialAudioPlayback] StopAllSfx not implemented for spatial audio (sources are independent).");
    }

    public void StopMusic()
    {
        // No-op for spatial audio
    }

    public void StopOneShotMusic()
    {
        // No-op for spatial audio
    }
}
