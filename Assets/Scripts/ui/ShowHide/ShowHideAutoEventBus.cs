using UnityEngine;

/// <summary>
/// Enhanced ShowHideBase that automatically handles EventBus subscriptions using reflection.
/// Derive from this class instead of ShowHideBase for automatic EventBus integration.
/// No need to override SubscribeToEventBus/UnsubscribeFromEventBus!
/// </summary>
/// <typeparam name="T">The concrete type of the derived class (use CRTP pattern)</typeparam>
public abstract class ShowHideAutoEventBus<T> : ShowHideBase where T : ShowHideAutoEventBus<T>
{
    /// <summary>
    /// Automatically subscribes to EventBus for the specific derived type
    /// </summary>
    protected override void SubscribeToEventBus()
    {
        EventBus.Subscribe<OpenUI<T>>(OnOpenUIEvent);
        EventBus.Subscribe<CloseUI<T>>(OnCloseUIEvent);
        EventBus.Subscribe<ToggleUI<T>>(OnToggleUIEvent);
    }

    /// <summary>
    /// Automatically unsubscribes from EventBus
    /// </summary>
    protected override void UnsubscribeFromEventBus()
    {
        EventBus.Unsubscribe<OpenUI<T>>(OnOpenUIEvent);
        EventBus.Unsubscribe<CloseUI<T>>(OnCloseUIEvent);
        EventBus.Unsubscribe<ToggleUI<T>>(OnToggleUIEvent);
    }

    /// <summary>
    /// Called when an OpenUI event is received
    /// </summary>
    protected virtual void OnOpenUIEvent(OpenUI<T> evt)
    {
        ShowUI();
    }

    /// <summary>
    /// Called when a CloseUI event is received
    /// </summary>
    protected virtual void OnCloseUIEvent(CloseUI<T> evt)
    {
        HideUI();
    }

    /// <summary>
    /// Called when a ToggleUI event is received
    /// </summary>
    protected virtual void OnToggleUIEvent(ToggleUI<T> evt)
    {
        ToggleUI();
    }
}

/// <summary>
/// Example usage of ShowHideAutoEventBus - much simpler than manual subscription!
/// Just derive from ShowHideAutoEventBus<YourClassName> and implement ShowInternal/HideInternal
/// </summary>
public class SimpleSettingsUI : ShowHideAutoEventBus<SimpleSettingsUI>
{
    protected override void ShowInternal()
    {
        gameObject.SetActive(true);
        // Add your show animation here
        OnShowComplete();
    }

    protected override void HideInternal()
    {
        gameObject.SetActive(false);
        // Add your hide animation here
        OnHideComplete();
    }

    // That's it! EventBus subscriptions are handled automatically
    // Just use: EventBus.Publish(new OpenUI<SimpleSettingsUI>());
}

/// <summary>
/// Another example showing you can still override the event handlers if needed
/// </summary>
public class AdvancedInventoryUI : ShowHideAutoEventBus<AdvancedInventoryUI>
{
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    protected override void OnOpenUIEvent(OpenUI<AdvancedInventoryUI> evt)
    {
        // Custom logic before opening
        if (openSound != null)
        {
            // Play sound
        }

        base.OnOpenUIEvent(evt); // Call base to actually show the UI
    }

    protected override void OnCloseUIEvent(CloseUI<AdvancedInventoryUI> evt)
    {
        // Custom logic before closing
        if (closeSound != null)
        {
            // Play sound
        }

        base.OnCloseUIEvent(evt); // Call base to actually hide the UI
    }

    protected override void ShowInternal()
    {
        gameObject.SetActive(true);
        OnShowComplete();
    }

    protected override void HideInternal()
    {
        gameObject.SetActive(false);
        OnHideComplete();
    }
}
