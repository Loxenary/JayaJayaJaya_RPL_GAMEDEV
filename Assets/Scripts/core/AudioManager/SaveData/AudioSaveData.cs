using UnityEngine;

public class AudioSaveData : BaseSaveData
{
    public float SoundMasterVolume = 1f;
    public float MusicMasterVolume = 1f;
    public float SFXMasterVolume = 1f;

    public AudioSaveData() : base("AudioSaveData")
    {

    }
}