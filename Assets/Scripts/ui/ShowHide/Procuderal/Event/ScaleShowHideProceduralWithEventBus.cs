using UnityEngine;

/// <summary>
/// Bridge class combining ScaleShowHideProcedural with EventBus auto-subscription.
/// </summary>
public abstract class ScaleShowHideProceduralWithEventBus<T> : ScaleShowHideProcedural
    where T : ScaleShowHideProceduralWithEventBus<T>
{
    protected override void OnEnable()
    {
        base.OnEnable();

        if (autoSubscribeToEventBus)
        {
            EventBus.Subscribe<OpenUI<T>>(OnOpenUIEvent);
            EventBus.Subscribe<CloseUI<T>>(OnCloseUIEvent);
            EventBus.Subscribe<ToggleUI<T>>(OnToggleUIEvent);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (autoSubscribeToEventBus)
        {
            EventBus.Unsubscribe<OpenUI<T>>(OnOpenUIEvent);
            EventBus.Unsubscribe<CloseUI<T>>(OnCloseUIEvent);
            EventBus.Unsubscribe<ToggleUI<T>>(OnToggleUIEvent);
        }
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