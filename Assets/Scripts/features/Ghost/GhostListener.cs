using UnityEngine;

public class GhostListener : MonoBehaviour
{
    [SerializeField] private SfxClipData ghostSpawnSFX;

    [SerializeField] private SfxClipData trapSetup;

    private void OnEnable()
    {
        EventBus.Subscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
    }

    private void OnFirstPuzzleEvent(FirstPuzzleEvent evt)
    {
        var audioManager = ServiceLocator.Get<AudioManager>();

        audioManager.PlaySfx(ghostSpawnSFX.SFXId);
        Invoke(nameof(PlayTrap), 0.5f);
    }

    private void PlayTrap()
    {
        var audioManager = ServiceLocator.Get<AudioManager>();
        audioManager.PlaySfx(trapSetup.SFXId);
    }
}