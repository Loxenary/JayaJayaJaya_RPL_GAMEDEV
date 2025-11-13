using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SFXClip", menuName ="Audio/SFX")]
public class SfxClipData : AudioClipData 
{
    [SerializeField] private SFXIdentifier sfxNameEnum;
    [Range(0,5)]
    [SerializeField] private float delay = 0;
    [Range(0,3)]
    [SerializeField] private float pitch = 1;

    public SFXIdentifier SFXId{
        get{
            return sfxNameEnum;
        }
    }
    public float Delay => delay;
    public float Pitch => pitch;


    #region Static Methods

    private static float _s_sfx_masterVolume = 1;
    public static float SFXMasterVolumeProperty{
        get{
            return _s_sfx_masterVolume;
        }set{
            if(value <= 0){
                Debug.LogWarning("Music master set to under negative number. Setting volume to 0 instead");
                _s_sfx_masterVolume = 0;
                return;
            }
            if(value > 1){
                value /= 100;
            }

            _s_sfx_masterVolume = value;
        }
    }

    private static Dictionary<SFXIdentifier, SfxClipData> _sfxRegistry  = new();

    public static Dictionary<SFXIdentifier, SfxClipData> SFXRegistry{
        get{
            return _sfxRegistry;
    }
        set{
            foreach(var item in value){
                if(_sfxRegistry.ContainsKey(item.Key)){
                    Debug.LogWarning("SFXClipData with id " + item.Key + " is Already Registered. Overwriting it.");
                    _sfxRegistry[item.Key] = item.Value;
                }

                else{
                    _sfxRegistry.Add(item.Key, item.Value);
                }
            }
        }
    } 

    public static SfxClipData GetSFXClipDataById(SFXIdentifier id){
        if(_sfxRegistry.TryGetValue(id, out SfxClipData data)){
            return data;
        }
        Debug.LogWarning("No SFXClipData found for id: " + id);
        return null;
    }

    public static float CalculateSFXVolume(float clipVolume){
        float calculatedVolume = MasterVolumeProperty * _s_sfx_masterVolume * clipVolume;
        if(calculatedVolume == 0){
            Debug.LogWarning("Music Volume Player is 0");
        }
        return calculatedVolume;
    }

    #endregion


}