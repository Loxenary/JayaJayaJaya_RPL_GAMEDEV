using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optional listener for hint events.
/// Useful for coordinating additional logic when hints are shown (e.g., sound effects, analytics).
/// Note: HintUI already subscribes to HintShown events directly, so this listener is optional.
/// </summary>
public class HintListener : MonoBehaviour
{
    [Header("Events")]
    [Tooltip("Invoked when a hint is shown")]
    [SerializeField] private UnityEvent<string> onHintShown;

    [Header("Optional: Sound Effects")]
    [SerializeField] private SfxClipData hintShownSfx;

    [Header("Debug")]
    [SerializeField] private bool logHintEvents = false;

    private void OnEnable()
    {
        EventBus.Subscribe<HintShown>(OnHintShown);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<HintShown>(OnHintShown);
    }

    private void OnHintShown(HintShown evt)
    {
        // Log for debugging
        if (logHintEvents)
        {
            Debug.Log($"[HintListener] Hint shown: '{evt.hintText}' (duration: {evt.displayDuration}s)");
        }

        // Invoke Unity event
        onHintShown?.Invoke(evt.hintText);

        // Play sound effect if configured
        if (hintShownSfx != null)
        {
            var audioManager = ServiceLocator.Get<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySfx(hintShownSfx.SFXId);
            }
        }
    }
}
