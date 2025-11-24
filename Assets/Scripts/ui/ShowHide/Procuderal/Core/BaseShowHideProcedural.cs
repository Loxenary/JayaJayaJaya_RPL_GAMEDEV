using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for procedural show/hide animations using coroutines.
/// Provides common animation timing and curve evaluation logic.
/// Derived classes implement specific animation types (scale, fade, etc.).
/// </summary>
public abstract class BaseShowHideProcedural : ShowHideBase
{
    [Header("Procedural Animation Settings")]
    [Tooltip("Duration of the show/hide animation in seconds")]
    [SerializeField] protected float animationDuration = 0.3f;

    [Tooltip("Animation curve for the transition")]
    [SerializeField] protected AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected Coroutine _currentAnimation;

    protected override void ShowInternal()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(AnimateShow());
    }

    protected override void HideInternal()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(AnimateHide());
    }

    private IEnumerator AnimateShow()
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);

            ApplyAnimation(t, true);

            yield return null;
        }

        // Ensure we end at exactly the target values
        ApplyAnimation(1f, true);

        _currentAnimation = null;
        OnShowComplete();
    }

    private IEnumerator AnimateHide()
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);

            ApplyAnimation(1f - t, false);

            yield return null;
        }

        // Ensure we end at exactly the target values
        ApplyAnimation(0f, false);

        _currentAnimation = null;
        OnHideComplete();
    }

    /// <summary>
    /// Apply the animation at the given time value (0 = hidden, 1 = shown).
    /// </summary>
    /// <param name="t">Animation progress (0 to 1)</param>
    /// <param name="isShowing">True if showing, false if hiding</param>
    protected abstract void ApplyAnimation(float t, bool isShowing);
}
