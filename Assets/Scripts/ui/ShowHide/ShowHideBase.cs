using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic event to open a specific UI panel
/// </summary>
public struct OpenUI<T> where T : ShowHideBase
{
    public T Data;

    public OpenUI(T data = null)
    {
        Data = data;
    }
}

/// <summary>
/// Generic event to close a specific UI panel
/// </summary>
public struct CloseUI<T> where T : ShowHideBase
{
    public T Data;

    public CloseUI(T data = null)
    {
        Data = data;
    }
}

/// <summary>
/// Generic event to toggle a specific UI panel
/// </summary>
public struct ToggleUI<T> where T : ShowHideBase
{
    public T Data;

    public ToggleUI(T data = null)
    {
        Data = data;
    }
}

/// <summary>
/// Base class for UI elements that can be shown and hidden.
/// Provides hooks for start/complete callbacks and manages visibility state.
/// Derived classes should implement specific show/hide behaviors (animation, procedural, etc.).
/// Also supports automatic EventBus subscription for OpenUI/CloseUI/ToggleUI events.
/// </summary>
public abstract class ShowHideBase : MonoBehaviour
{
    /// <summary>
    /// Set to true to automatically subscribe to EventBus for OpenUI/CloseUI/ToggleUI events
    /// </summary>
    [Header("EventBus Settings")]
    [SerializeField] protected bool autoSubscribeToEventBus = true;

    [SerializeField] protected UnityEvent onShowStarted;
    [SerializeField] protected UnityEvent onShowCompleted;
    [SerializeField] protected UnityEvent onHideStarted;
    [SerializeField] protected UnityEvent onHideCompleted;

    [Tooltip("Turn this on so that the UI will stop time when shown and resume time when hidden")]
    [SerializeField] protected bool isStopTime = false;

    [Tooltip("Show the cursor on the UI Show")]
    [SerializeField] protected bool isShowCursor = false;

    /// <summary>
    /// True if the UI is currently visible or transitioning to visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// True if the UI is currently transitioning (showing or hiding).
    /// </summary>
    public bool IsTransitioning { get; private set; }

    private bool _lastCursorVisible = false;
    private CursorLockMode _lastCursorLockMode = CursorLockMode.None;

    /// <summary>
    /// Shows the UI element. Calls ShowUIStart, then triggers the show implementation.
    /// </summary>
    public void ShowUI()
    {
        if (IsVisible && !IsTransitioning)
        {
            return; // Already visible
        }

        // Save cursor state BEFORE showing
        if (isShowCursor)
        {
            _lastCursorVisible = Cursor.visible;
            _lastCursorLockMode = Cursor.lockState;
        }

        IsTransitioning = true;
        IsVisible = true;
        ShowUIStart();
        ShowInternal();
        if (isStopTime)
        {
            ServiceLocator.Get<TimeService>().RequestStopTime(this);
        }
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
    /// Toggles the UI element between shown and hidden states.
    /// </summary>
    public void ToggleUI()
    {
        if (IsVisible)
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
    }

    /// <summary>
    /// Called when the component is enabled. Subscribes to EventBus if autoSubscribeToEventBus is true.
    /// </summary>
    protected virtual void OnEnable()
    {
        if (autoSubscribeToEventBus)
        {
            SubscribeToEventBus();
        }
    }

    /// <summary>
    /// Called when the component is disabled. Unsubscribes from EventBus if autoSubscribeToEventBus is true.
    /// </summary>
    protected virtual void OnDisable()
    {
        if (autoSubscribeToEventBus)
        {
            UnsubscribeFromEventBus();
        }
    }

    /// <summary>
    /// Subscribe to EventBus for this specific UI type.
    /// Override this in derived classes to implement type-specific subscriptions.
    /// </summary>
    protected virtual void SubscribeToEventBus()
    {
        // Derived classes should implement this using their specific type
        // Example in derived class:
        // EventBus.Subscribe<OpenUI<MyUIClass>>(OnOpenUI);
        // EventBus.Subscribe<CloseUI<MyUIClass>>(OnCloseUI);
        // EventBus.Subscribe<ToggleUI<MyUIClass>>(OnToggleUI);
    }

    /// <summary>
    /// Unsubscribe from EventBus for this specific UI type.
    /// Override this in derived classes to implement type-specific unsubscriptions.
    /// </summary>
    protected virtual void UnsubscribeFromEventBus()
    {
        // Derived classes should implement this using their specific type
        // Example in derived class:
        // EventBus.Unsubscribe<OpenUI<MyUIClass>>(OnOpenUI);
        // EventBus.Unsubscribe<CloseUI<MyUIClass>>(OnCloseUI);
        // EventBus.Unsubscribe<ToggleUI<MyUIClass>>(OnToggleUI);
    }

    /// <summary>
    /// Called when the show transition is complete.
    /// Derived classes should call this after their show animation/transition finishes.
    /// </summary>
    protected void OnShowComplete()
    {
        IsTransitioning = false;
        ShowUIComplete();
        if (isShowCursor)
        {
            // Unlock and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
        if (isStopTime)
        {
            ServiceLocator.Get<TimeService>().RequestResumeTime(this);
        }

        if (isShowCursor)
        {
            // Restore previous cursor state
            Cursor.visible = _lastCursorVisible;
            Cursor.lockState = _lastCursorLockMode;
        }
    }

    /// <summary>
    /// Override this to perform actions when hiding starts (before the hide animation).
    /// </summary>
    protected virtual void HideUIStart()
    {
        // Override in derived classes if needed
        onShowStarted?.Invoke();
    }

    /// <summary>
    /// Override this to perform actions when hiding completes (after the hide animation).
    /// </summary>
    protected virtual void HideUIComplete()
    {
        // Override in derived classes if needed
        onHideCompleted?.Invoke();
    }

    /// <summary>
    /// Override this to perform actions when showing starts (before the show animation).
    /// </summary>
    protected virtual void ShowUIStart()
    {
        // Override in derived classes if needed
        onShowStarted?.Invoke();
    }

    /// <summary>
    /// Override this to perform actions when showing completes (after the show animation).
    /// </summary>
    protected virtual void ShowUIComplete()
    {
        // Override in derived classes if needed
        onShowCompleted?.Invoke();
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