// File: GlobalAudioPlayback.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles 2D/Global audio playback (non-spatial).
/// All sounds play at the same volume regardless of position.
/// </summary>
public class GlobalAudioPlayback : IAudioPlaybackService
{
    private readonly AudioSource _musicSource;
    private readonly AudioSource _introSource;
    private readonly AudioSource _sfxSource;
    private AudioSource _musicOneShotSource;
    private readonly AudioVolumeController _volumeController;
    private readonly AudioDataRegistry _dataRegistry;
    private readonly MonoBehaviour _coroutineRunner;

    // Pitch chain tracking
    private Coroutine _pitchChainTimer;
    private int _pitchChainCount;
    private readonly float _pitchChainDelay = 1.0f;
    private readonly float _pitchStep = 0.3f;
    private readonly float _pitchCap = 3.0f;

    public GlobalAudioPlayback(
        AudioSource musicSource,
        AudioSource introSource,
        AudioSource sfxSource,
        AudioSource musicOneShotSource,
        AudioVolumeController volumeController,
        AudioDataRegistry dataRegistry,
        MonoBehaviour coroutineRunner)
    {
        _musicSource = musicSource;
        _introSource = introSource;
        _sfxSource = sfxSource;
        _musicOneShotSource = musicOneShotSource;
        _volumeController = volumeController;
        _dataRegistry = dataRegistry;
        _coroutineRunner = coroutineRunner;

        // Subscribe to volume changes
        _volumeController.OnVolumeChanged += OnVolumeChanged;
    }

    private void OnVolumeChanged()
    {
        // Update all audio source volumes
        if (_musicSource != null && _musicSource.clip != null)
        {
            var musicData = _dataRegistry.GetMusicData(GetMusicIdentifier(_musicSource.clip));
            if (musicData != null)
            {
                _musicSource.volume = _volumeController.CalculateMusicVolume(musicData.Volume);
            }
        }

        if (_musicOneShotSource != null)
        {
            _musicOneShotSource.volume = _volumeController.CalculateMusicVolume(1f);
        }
    }

    public void PlaySfx(SFXIdentifier sfxId)
    {
        var clipData = _dataRegistry.GetSFXData(sfxId);
        if (clipData == null || clipData.Clip == null)
        {
            Debug.LogWarning($"[GlobalAudioPlayback] SFX not found: {sfxId}");
            return;
        }

        float volume = _volumeController.CalculateSFXVolume(clipData.Volume);

        if (clipData.Delay > 0)
        {
            _sfxSource.clip = clipData.Clip;
            _sfxSource.volume = volume;
            _sfxSource.PlayDelayed(clipData.Delay);
        }
        else
        {
            _sfxSource.pitch = clipData.Pitch;
            _sfxSource.PlayOneShot(clipData.Clip, volume);
            _sfxSource.pitch = 1f;
        }
    }

    public void PlaySfx(SFXIdentifier sfxId, float pitch)
    {
        var clipData = _dataRegistry.GetSFXData(sfxId);
        if (clipData == null || clipData.Clip == null)
        {
            Debug.LogWarning($"[GlobalAudioPlayback] SFX not found: {sfxId}");
            return;
        }

        float volume = _volumeController.CalculateSFXVolume(clipData.Volume);
        _sfxSource.pitch = pitch;
        _sfxSource.PlayOneShot(clipData.Clip, volume);
        _sfxSource.pitch = 1f;
    }

    public void PlayIncreasingPitchSFX(SFXIdentifier sfxId)
    {
        var clipData = _dataRegistry.GetSFXData(sfxId);
        if (clipData == null || clipData.Clip == null)
        {
            Debug.LogWarning($"[GlobalAudioPlayback] SFX not found: {sfxId}");
            return;
        }

        if (_pitchChainTimer != null)
        {
            _coroutineRunner.StopCoroutine(_pitchChainTimer);
        }

        _pitchChainTimer = _coroutineRunner.StartCoroutine(PitchChainCountdown());
        _pitchChainCount++;

        float finalPitch = Mathf.Min(1f + _pitchStep * _pitchChainCount, _pitchCap);
        PlaySfx(sfxId, finalPitch);
    }

    private IEnumerator PitchChainCountdown()
    {
        yield return new WaitForSeconds(_pitchChainDelay);
        _pitchChainCount = 0;
        _pitchChainTimer = null;
    }

    public void PlayRandomSfx(params SFXIdentifier[] choices)
    {
        if (choices == null || choices.Length == 0)
        {
            return;
        }

        int idx = Random.Range(0, choices.Length);
        PlaySfx(choices[idx]);
    }

    public void PlayMusic(MusicIdentifier musicId)
    {
        var musicData = _dataRegistry.GetMusicData(musicId);
        if (musicData == null || musicData.Clip == null)
        {
            Debug.LogWarning($"[GlobalAudioPlayback] Music not found: {musicId}");
            return;
        }

        // Don't restart if already playing
        if (_musicSource.clip == musicData.Clip && _musicSource.isPlaying)
        {
            return;
        }

        _musicSource.clip = musicData.Clip;
        _musicSource.volume = _volumeController.CalculateMusicVolume(musicData.Volume);
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void PlayMusicOneShot(MusicIdentifier musicId, float pitch = 1f)
    {
        var musicData = _dataRegistry.GetMusicData(musicId);
        if (musicData == null || musicData.Clip == null)
        {
            Debug.LogWarning($"[GlobalAudioPlayback] Music not found: {musicId}");
            return;
        }

        _musicOneShotSource.pitch = pitch;
        _musicOneShotSource.PlayOneShot(musicData.Clip, _volumeController.CalculateMusicVolume(musicData.Volume));
        _musicOneShotSource.pitch = 1f;
    }

    public void PlayMusicWithIntro(MusicIdentifier introId, MusicIdentifier loopId)
    {
        _musicSource.Stop();
        PlayScheduled(introId, loopId, gapAfterIntro: 0.2f, leadTime: 0.12f);
    }

    private void PlayScheduled(MusicIdentifier introId, MusicIdentifier loopId, float gapAfterIntro, float leadTime)
    {
        var introData = _dataRegistry.GetMusicData(introId);
        var loopData = _dataRegistry.GetMusicData(loopId);

        if (introData == null || loopData == null)
        {
            Debug.LogWarning("[GlobalAudioPlayback] Intro or loop music not found");
            return;
        }

        _introSource.Stop();
        _musicSource.Stop();

        if (introData.Clip.loadState != AudioDataLoadState.Loaded) introData.Clip.LoadAudioData();
        if (loopData.Clip.loadState != AudioDataLoadState.Loaded) loopData.Clip.LoadAudioData();

        _musicSource.outputAudioMixerGroup = _introSource.outputAudioMixerGroup;
        _introSource.loop = false;
        _introSource.pitch = 1f;
        _introSource.volume = _volumeController.CalculateMusicVolume(introData.Volume);
        _introSource.clip = introData.Clip;

        _musicSource.loop = true;
        _musicSource.pitch = 1f;
        _musicSource.volume = _volumeController.CalculateMusicVolume(loopData.Volume);
        _musicSource.clip = loopData.Clip;

        double introDelay = Mathf.Max(0f, introData.Delay);
        double loopDelay = Mathf.Max(0f, loopData.Delay);
        float gap = Mathf.Max(0.0f, gapAfterIntro);

        double now = AudioSettings.dspTime;
        double startTime = now + Mathf.Max(0.05f, leadTime);

        double introStart = startTime + introDelay;
        double introEnd = introStart + _introSource.clip.length;
        double loopStart = introEnd + gap + loopDelay;

        _introSource.PlayScheduled(introStart);
        _musicSource.PlayScheduled(loopStart);
        _introSource.SetScheduledEndTime(loopStart);
    }

    public void StopAllSfx()
    {
        _sfxSource.Stop();
        _sfxSource.pitch = 1f;
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _introSource.Stop();
    }

    public void StopOneShotMusic()
    {
        _musicOneShotSource.Stop();
    }

    // Helper to get MusicIdentifier from AudioClip (would need reverse lookup in real implementation)
    private MusicIdentifier GetMusicIdentifier(AudioClip clip)
    {
        // This is a simplified version - you'd need proper reverse lookup
        return default;
    }

    /// <summary>
    /// Sets the music one-shot source (called after initialization).
    /// </summary>
    public void SetMusicOneShotSource(AudioSource musicOneShotSource)
    {
        _musicOneShotSource = musicOneShotSource;
    }
}
