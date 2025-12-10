using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightingHandler : MonoBehaviour
{
    [SerializeField] private SfxClipData lightingSwitchSfx;

    [SerializeField] private List<GameObject> turnedOnLights = new List<GameObject>();

    private void OnEnable()
    {
        EventBus.Subscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
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
}
