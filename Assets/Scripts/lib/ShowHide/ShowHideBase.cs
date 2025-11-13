using UnityEngine;

/// <summary>
/// Base class for UI elements that can be shown and hidden.
/// Provides hooks for start/complete callbacks and manages visibility state.
/// Derived classes should implement specific show/hide behaviors (animation, procedural, etc.).
/// </summary>
public abstract class ShowHideBase : MonoBehaviour
{
    /// <summary>
    /// True if the UI is currently visible or transitioning to visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// True if the UI is currently transitioning (showing or hiding).
    /// </summary>
    public bool IsTransitioning { get; private set; }

    /// <summary>
    /// Shows the UI element. Calls ShowUIStart, then triggers the show implementation.
    /// </summary>
    public void ShowUI()
    {
        if (IsVisible && !IsTransitioning)
        {
            return; // Already visible
        }

        IsTransitioning = true;
        IsVisible = true;
        ShowUIStart();
        ShowInternal();
    }

    /// <summary>
    /// Hides the UI element. Calls HideUIStart, then triggers the hide implementation.
    /// </summary>
    public void HideUI()
    {
        if (!IsVisible && !IsTransitioning)
        {
            return; // Already hidden
        }

        IsTransitioning = true;
        HideUIStart();
        HideInternal();
    }

    /// <summary>
    /// Called when the show transition is complete.
    /// Derived classes should call this after their show animation/transition finishes.
    /// </summary>
    protected void OnShowComplete()
    {
        IsTransitioning = false;
        ShowUIComplete();
    }

    /// <summary>
    /// Called when the hide transition is complete.
    /// Derived classes should call this after their hide animation/transition finishes.
    /// </summary>
    protected void OnHideComplete()
    {
        IsTransitioning = false;
        IsVisible = false;
        HideUIComplete();
    }

    /// <summary>
    /// Override this to perform actions when hiding starts (before the hide animation).
    /// </summary>
    protected virtual void HideUIStart()
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Override this to perform actions when hiding completes (after the hide animation).
    /// </summary>
    protected virtual void HideUIComplete()
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Override this to perform actions when showing starts (before the show animation).
    /// </summary>
    protected virtual void ShowUIStart()
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Override this to perform actions when showing completes (after the show animation).
    /// </summary>
    protected virtual void ShowUIComplete()
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Implement the actual show behavior (animation, tween, instant, etc.).
    /// Must call OnShowComplete() when finished.
    /// </summary>
    protected abstract void ShowInternal();

    /// <summary>
    /// Implement the actual hide behavior (animation, tween, instant, etc.).
    /// Must call OnHideComplete() when finished.
    /// </summary>
    protected abstract void HideInternal();
}