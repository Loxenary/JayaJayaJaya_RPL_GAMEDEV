using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;


public class SettingsUI : FadeShowHideProceduralWithEventBus<SettingsUI>
{
    [Header("Settings UI Components")]
    [SerializeField] private Slider musicSlider;

    [SerializeField] private Slider sfxSlider;


    private void OnValidate()
    {
        Assert.IsNotNull(musicSlider, "Volume slider is missing");
        Assert.IsNotNull(sfxSlider, "SFX slider is missing");
    }

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener(OnMusicValueChange);
        sfxSlider.onValueChanged.AddListener(OnSFXValueChange);
    }

    private void OnMusicValueChange(float newValue)
    {
        ServiceLocator.Get<AudioManager>().SetMusicVolume(newValue);
    }

    private void OnSFXValueChange(float newValue)
    {
        ServiceLocator.Get<AudioManager>().SetSFXVolume(newValue);
    }

}