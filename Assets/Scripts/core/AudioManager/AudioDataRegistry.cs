// File: AudioDataRegistry.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for loading and managing audio clip data registries.
/// Separates data loading concerns from playback logic.
/// </summary>
public class AudioDataRegistry
{
    private const string SFX_RESOURCE_PATH = "Audio/SFX";
    private const string MUSIC_RESOURCE_PATH = "Audio/Music";

    /// <summary>
    /// Loads all SFX and Music clip data from Resources folders.
    /// Should be called once during initialization.
    /// </summary>
    public void LoadAllAudioData()
    {
        LoadSFXData();
        LoadMusicData();
    }

    /// <summary>
    /// Loads SFX clip data from Resources.
    /// </summary>
    private void LoadSFXData()
    {
        SfxClipData[] sfxArray = Resources.LoadAll<SfxClipData>(SFX_RESOURCE_PATH);

        if (sfxArray == null || sfxArray.Length == 0)
        {
            Debug.LogWarning($"[AudioDataRegistry] No SFX data found at {SFX_RESOURCE_PATH}");
            return;
        }

        var sfxDict = new Dictionary<SFXIdentifier, SfxClipData>();
        foreach (var clip in sfxArray)
        {
            if (clip != null)
            {
                sfxDict[clip.SFXId] = clip;
            }
        }

        SfxClipData.SFXRegistry = sfxDict;
        Debug.Log($"[AudioDataRegistry] Loaded {sfxDict.Count} SFX clips");
    }

    /// <summary>
    /// Loads Music clip data from Resources.
    /// </summary>
    private void LoadMusicData()
    {
        MusicClipData[] musicArray = Resources.LoadAll<MusicClipData>(MUSIC_RESOURCE_PATH);

        if (musicArray == null || musicArray.Length == 0)
        {
            Debug.LogWarning($"[AudioDataRegistry] No music data found at {MUSIC_RESOURCE_PATH}");
            return;
        }

        var musicDict = new Dictionary<MusicIdentifier, MusicClipData>();
        foreach (var clip in musicArray)
        {
            if (clip != null)
            {
                musicDict[clip.MusicId] = clip;
            }
        }

        MusicClipData.MusicRegistry = musicDict;
        Debug.Log($"[AudioDataRegistry] Loaded {musicDict.Count} music clips");
    }

    /// <summary>
    /// Retrieves SFX clip data by identifier.
    /// </summary>
    public SfxClipData GetSFXData(SFXIdentifier sfxId)
    {
        return SfxClipData.GetSFXClipDataById(sfxId);
    }

    /// <summary>
    /// Retrieves Music clip data by identifier.
    /// </summary>
    public MusicClipData GetMusicData(MusicIdentifier musicId)
    {
        return MusicClipData.GetMusicClipDataById(musicId);
    }

    /// <summary>
    /// Clears all loaded audio data (useful for cleanup or reloading).
    /// </summary>
    public void ClearAllData()
    {
        SfxClipData.SFXRegistry = new Dictionary<SFXIdentifier, SfxClipData>();
        MusicClipData.MusicRegistry = new Dictionary<MusicIdentifier, MusicClipData>();
    }
}
