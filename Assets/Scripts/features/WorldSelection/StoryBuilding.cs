using DG.Tweening;
using UnityEngine;

/// <summary>
/// Attached to each building in the world selection diorama.
/// Holds the story it represents, the camera anchor used for the zoom-in,
/// and the visuals for hover/locked/completed states.
/// The collider(s) under this object must be on the layer raycast by
/// WorldSelectionController.
/// </summary>
public class StoryBuilding : MonoBehaviour
{
    [SerializeField] private StoryDefinition story;

    [Tooltip("Camera pose the world camera tweens to when entering this building (place in front of the entrance, looking at the door).")]
    [SerializeField] private Transform zoomCameraAnchor;

    [Header("Visuals")]
    [SerializeField] private HiglightObject highlight;
    [Tooltip("Shown while the story is locked or not yet playable (e.g. fog, dark overlay, lock icon).")]
    [SerializeField] private GameObject lockedVisual;
    [Tooltip("Shown once the story has been completed (e.g. lit windows).")]
    [SerializeField] private GameObject completedMarker;
    [Tooltip("Scaled by the locked-click feedback. Defaults to this transform.")]
    [SerializeField] private Transform punchTarget;
    [SerializeField] private SfxClipData lockedSfx;

    public StoryDefinition Story => story;
    public Transform ZoomAnchor => zoomCameraAnchor != null ? zoomCameraAnchor : transform;
    public bool IsUnlocked { get; private set; }

    /// <summary>True when the story is unlocked AND has a playable scene group.</summary>
    public bool IsEnterable { get; private set; }

    private Tween _punchTween;

    public void RefreshState(bool unlocked, StoryProgressEntry progress)
    {
        IsUnlocked = unlocked;
        IsEnterable = unlocked && story != null && story.IsPlayable;

        if (lockedVisual != null)
        {
            lockedVisual.SetActive(!IsEnterable);
        }
        if (completedMarker != null)
        {
            completedMarker.SetActive(progress != null && progress.completed);
        }
    }

    public void SetHovered(bool hovered)
    {
        if (highlight == null)
        {
            return;
        }

        if (hovered)
        {
            highlight.Highlight();
        }
        else
        {
            highlight.UnHighlight();
        }
    }

    /// <summary>Small shake + sfx when the player clicks a building they cannot enter.</summary>
    public void PlayLockedFeedback()
    {
        var target = punchTarget != null ? punchTarget : transform;
        if (_punchTween == null || !_punchTween.IsActive())
        {
            _punchTween = target.DOPunchScale(Vector3.one * 0.05f, 0.3f, vibrato: 8)
                .OnComplete(() => _punchTween = null);
        }

        if (lockedSfx != null)
        {
            ServiceLocator.Get<AudioManager>()?.PlaySfx(lockedSfx.SFXId);
        }
    }

    private void OnDestroy()
    {
        _punchTween?.Kill();
    }
}
