using UnityEngine;

/// <summary>
/// Trigger volume placed just OUTSIDE the exit door threshold.
/// While the exit door is still open (before FirstPuzzleEvent locks it),
/// walking out returns the player to the world selection map.
/// Once FirstPuzzleEvent fires, the trigger disarms permanently for this run.
/// </summary>
[RequireComponent(typeof(Collider))]
public class StoryExitTrigger : MonoBehaviour
{
    [Tooltip("Optional double-guard: when set, the trigger also refuses to fire once this door reports locked.")]
    [SerializeField] private InteractableEndGameDoor exitDoor;

    private bool _armed = true;
    private bool _fired;

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
        // The story has started: the exit door locks, leaving is no longer possible.
        _armed = false;
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (_fired || !_armed)
        {
            return;
        }
        if (exitDoor != null && exitDoor.IsLocked)
        {
            return;
        }
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _fired = true;

        // Freeze movement so the player cannot wander during the fade-out.
        other.GetComponentInParent<PlayerController>()?.SetFrozen(true);

        await ServiceLocator.Get<FlowManager>().ReturnToSelection();
    }
}
