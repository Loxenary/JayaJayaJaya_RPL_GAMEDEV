using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightingHandler : MonoBehaviour, IRestartable
{
    [SerializeField] private SfxClipData lightingSwitchSfx;

    [SerializeField] private List<GameObject> turnedOnLights = new List<GameObject>();

    private void OnEnable()
    {
        EventBus.Subscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
        RestartManager.Register(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
        RestartManager.Unregister(this);
    }


    private void OnFirstPuzzleEvent(FirstPuzzleEvent eventData)
    {
        var audioManager = ServiceLocator.Get<AudioManager>();

        audioManager.PlaySfx(lightingSwitchSfx.SFXId);

        foreach (var light in turnedOnLights)
        {
            light.SetActive(false);
        }
    }

    public void TurnOnSound()
    {
        var audioManager = ServiceLocator.Get<AudioManager>();
        audioManager.PlaySfx(lightingSwitchSfx.SFXId);
    }

    public void Restart()
    {
        foreach (var light in turnedOnLights)
        {
            light.SetActive(true);
        }
    }
}
