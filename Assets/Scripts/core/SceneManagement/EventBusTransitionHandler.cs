// File: EventBusTransitionHandler.cs
using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Transition handler that uses EventBus to communicate with a TransitionUI system.
/// This maintains your current event-based approach but encapsulates it.
/// </summary>
public class EventBusTransitionHandler : ISceneTransitionHandler
{
    private readonly float _animationSpeed;
    private readonly float _waitDuration;

    public EventBusTransitionHandler(float animationSpeed = 1.1f)
    {
        _animationSpeed = animationSpeed;
        // Calculate wait duration based on animation speed
        _waitDuration = (1f / _animationSpeed) - 0.05f;
    }

    public async Task ShowTransition()
    {
        // Publish event to show the transition
        EventBus.Publish(new ShowTransitionEvent
        {
            animationSpeed = _animationSpeed
        });

        // Wait for the transition animation to cover the screen
        await Task.Delay(TimeSpan.FromSeconds(_waitDuration));
    }

    public Task HideTransition()
    {
        // Publish event to close/hide the transition
        EventBus.Publish(new CloseTransitionEvent());
        return Task.CompletedTask;
    }
}

public struct ShowTransitionEvent
{
    public float animationSpeed;
}

public struct CloseTransitionEvent
{

}
