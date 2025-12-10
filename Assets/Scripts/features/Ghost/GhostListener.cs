using UnityEngine;

public class GhostListener : MonoBehaviour
{
    [SerializeField] private SfxClipData ghostSpawnSFX;

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

    

    }
}