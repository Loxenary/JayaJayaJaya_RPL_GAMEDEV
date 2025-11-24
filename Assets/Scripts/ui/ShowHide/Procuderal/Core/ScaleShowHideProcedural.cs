using UnityEngine;

/// <summary>
/// ShowHide implementation using scale-based procedural animation.
/// Smoothly scales the UI element between hidden and shown states.
/// </summary>
public class ScaleShowHideProcedural : BaseShowHideProcedural
{
    [Header("Scale Animation Settings")]
    [Tooltip("Scale when hidden")]
    [SerializeField] private Vector3 hiddenScale = Vector3.zero;

    [Tooltip("Scale when shown")]
    [SerializeField] private Vector3 shownScale = Vector3.one;

    protected override void ApplyAnimation(float t, bool isShowing)
    {
        transform.localScale = Vector3.Lerp(hiddenScale, shownScale, t);
    }
}
