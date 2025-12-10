using Unity.VisualScripting;
using UnityEngine;

public class LightingHandler : MonoBehaviour
{
    [SerializeField] private SfxClipData lightingSwitchSfx;

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

        gameObject.SetActive(false);
    }
}
