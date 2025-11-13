// File: IAudioPlaybackService.cs
using UnityEngine;

/// <summary>
/// Interface for audio playback services.
/// Allows different implementations for 2D (global) and 3D (spatial) audio.
/// </summary>
public interface IAudioPlaybackService
{
    /// <summary>
    /// Plays an SFX sound effect.
    /// </summary>
    void PlaySfx(SFXIdentifier sfxId);

    /// <summary>
    /// Plays an SFX with custom pitch.
    /// </summary>
    void PlaySfx(SFXIdentifier sfxId, float pitch);

    /// <summary>
    /// Plays an SFX with increasing pitch on repeated calls.
    /// </summary>
    void PlayIncreasingPitchSFX(SFXIdentifier sfxId);

    /// <summary>
    /// Plays a random SFX from the provided choices.
    /// </summary>
    void PlayRandomSfx(params SFXIdentifier[] choices);

    /// <summary>
    /// Plays background music (looping).
    /// </summary>
    void PlayMusic(MusicIdentifier musicId);

    /// <summary>
    /// Plays a one-shot music clip (non-looping).
    /// </summary>
    void PlayMusicOneShot(MusicIdentifier musicId, float pitch = 1f);

    /// <summary>
    /// Plays music with an intro that transitions to a loop.
    /// </summary>
    void PlayMusicWithIntro(MusicIdentifier introId, MusicIdentifier loopId);

    /// <summary>
    /// Stops all currently playing SFX.
    /// </summary>
    void StopAllSfx();

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    void StopMusic();

    /// <summary>
    /// Stops the currently playing one-shot music.
    /// </summary>
    void StopOneShotMusic();
}
