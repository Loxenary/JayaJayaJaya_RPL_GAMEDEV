# ShowHide System - Usage Guide

## Overview
The ShowHide system provides a flexible framework for showing and hiding UI elements with different animation types. It uses an abstract base class with multiple implementations for different animation approaches.

## Architecture

### Components

1. **ShowHideBase** - Abstract base class
   - Manages visibility state (`IsVisible`, `IsTransitioning`)
   - Provides lifecycle hooks (Start/Complete callbacks)
   - Defines abstract methods for implementations

2. **ShowHideAnimation** - Animator-based implementation
   - Uses Unity Animator with triggers or state names
   - Requires Animation Events to signal completion
   - Best for complex, timeline-based animations

3. **ShowHideProcedural** - Code-based implementation
   - Uses coroutines for simple animations
   - Supports: Scale, Fade, Position, ScaleAndFade
   - No animator setup required

## Quick Start

### Using ShowHideAnimation

1. Add `ShowHideAnimation` component to your UI GameObject
2. Add an `Animator` component
3. Create animation clips for "Show" and "Hide"
4. Add Animation Events at the end of each clip:
   - Show clip: Call `AnimationEvent_ShowComplete()`
   - Hide clip: Call `AnimationEvent_HideComplete()`
5. Configure triggers/state names in inspector

```csharp
// In your code
var showHide = GetComponent<ShowHideAnimation>();
showHide.ShowUI();  // Triggers show animation
showHide.HideUI();  // Triggers hide animation
```

### Using ShowHideProcedural

1. Add `ShowHideProcedural` component to your UI GameObject
2. Choose animation type in inspector:
   - **Scale**: Animates from hiddenScale to shownScale
   - **Fade**: Requires CanvasGroup, animates alpha
   - **Position**: Slides from offset position
   - **ScaleAndFade**: Combines scale and fade
3. Configure duration and animation curve

```csharp
// In your code
var showHide = GetComponent<ShowHideProcedural>();
showHide.ShowUI();  // Starts procedural animation
showHide.HideUI();  // Starts hide animation
```

## Creating Custom Implementations

You can create your own show/hide behavior by inheriting from `ShowHideBase`:

```csharp
public class ShowHideCustom : ShowHideBase
{
    protected override void ShowInternal()
    {
        // Your custom show logic here
        // e.g., DOTween, custom shader, etc.

        // IMPORTANT: Call this when done
        OnShowComplete();
    }

    protected override void HideInternal()
    {
        // Your custom hide logic here

        // IMPORTANT: Call this when done
        OnHideComplete();
    }

    // Optional: Override lifecycle hooks
    protected override void ShowUIStart()
    {
        // Called before show animation starts
    }

    protected override void ShowUIComplete()
    {
        // Called after show animation completes
    }
}
```

## Properties

### ShowHideBase Properties

- `IsVisible` - True if UI is visible or showing
- `IsTransitioning` - True if currently animating

### ShowHideAnimation Inspector Fields

- **Show Trigger Name**: Animator trigger/state for show (default: "Show")
- **Hide Trigger Name**: Animator trigger/state for hide (default: "Hide")
- **Use Triggers**: Use triggers instead of PlayAnimation

### ShowHideProcedural Inspector Fields

- **Animation Type**: Scale, Fade, Position, or ScaleAndFade
- **Animation Duration**: Time in seconds (default: 0.3)
- **Animation Curve**: Easing curve for the animation
- **Hidden Scale**: Scale when hidden (default: Vector3.zero)
- **Shown Scale**: Scale when shown (default: Vector3.one)
- **Canvas Group**: Required for Fade types
- **Hidden Position Offset**: Offset for Position animation

## Lifecycle Hooks

Override these methods to add custom behavior:

```csharp
protected override void ShowUIStart()
{
    // Called when ShowUI() is invoked, before animation
    Debug.Log("Starting to show UI");
}

protected override void ShowUIComplete()
{
    // Called after show animation completes
    Debug.Log("UI is now fully visible");
    EventBus.Publish(new UIShownEvent());
}

protected override void HideUIStart()
{
    // Called when HideUI() is invoked, before animation
    Debug.Log("Starting to hide UI");
}

protected override void HideUIComplete()
{
    // Called after hide animation completes
    Debug.Log("UI is now fully hidden");
    gameObject.SetActive(false); // Optional: disable GameObject
}
```

## Integration Example: TransitionUI

Perfect for implementing your scene transition UI:

```csharp
public class TransitionUI : MonoBehaviour
{
    private ShowHideBase _showHide;

    private void Awake()
    {
        _showHide = GetComponent<ShowHideBase>();

        // Subscribe to events
        EventBus.Subscribe<ShowTransitionEvent>(OnShowTransition);
        EventBus.Subscribe<CloseTransitionEvent>(OnCloseTransition);
    }

    private void OnShowTransition(ShowTransitionEvent evt)
    {
        _showHide.ShowUI();
    }

    private void OnCloseTransition(CloseTransitionEvent evt)
    {
        _showHide.HideUI();
    }
}
```

## Best Practices

1. **Always call completion callbacks**: `OnShowComplete()` and `OnHideComplete()` must be called
2. **Check IsTransitioning**: Avoid starting new animations while one is in progress
3. **Use appropriate type**: Animation for complex, Procedural for simple effects
4. **Animation Events**: Place at the very end of animation clips
5. **Test both states**: Ensure show and hide work correctly from any state

## Troubleshooting

### Animation doesn't complete
- Check that Animation Events are placed correctly
- Ensure you're calling `AnimationEvent_ShowComplete()` / `AnimationEvent_HideComplete()`

### Fade doesn't work
- Ensure CanvasGroup component is attached
- Check that animation type is Fade or ScaleAndFade

### Position animation jumps
- Position animation uses Start() to capture original position
- Ensure the GameObject starts at its intended "shown" position

## Performance Notes

- **Procedural animations** use coroutines and update every frame during transition
- **Animator-based** uses Unity's animation system (generally more performant)
- Consider object pooling if frequently showing/hiding many UI elements
- Use `IsTransitioning` to avoid redundant animation starts
