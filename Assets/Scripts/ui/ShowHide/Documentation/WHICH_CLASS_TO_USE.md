# Which ShowHide Class Should I Use?

## Quick Decision Tree

```
Do you want EventBus integration? (Open with EventBus.Publish(new OpenUI<MyUI>()))
├─ YES → Continue below
└─ NO  → Use ShowHideAnimation or ShowHideProcedural directly

What kind of animation do you want?
├─ Unity Animator (timeline-based)
│   └─ Use: ShowHideAnimation + ShowHideAutoEventBus
│
├─ Code-based (scale, fade, position)
│   └─ Use: ShowHideProcedural + ShowHideAutoEventBus
│
└─ Custom (your own animation code)
    └─ Use: ShowHideAutoEventBus directly
```

## Class Overview

### Option 1: ShowHideAnimation (Unity Animator)
**Use when:** You have Unity Animator with animation clips
**Best for:** Complex, designer-created animations

```csharp
// Requires Unity Animator component with "Show" and "Hide" states/triggers
public class MyUI : ShowHideAnimation
{
    // No code needed! Configure in Inspector:
    // - Animator component
    // - Show/Hide trigger names
}
```

### Option 2: ShowHideProcedural (Code-based)
**Use when:** You want simple animations without setting up Animator
**Best for:** Quick, simple UI transitions (fade, scale, slide)

```csharp
// Built-in animations: Scale, Fade, Position, ScaleAndFade
public class MyUI : ShowHideProcedural
{
    // No code needed! Configure in Inspector:
    // - Animation Type (Scale/Fade/Position/etc.)
    // - Duration
    // - Animation Curve
}
```

### Option 3: ShowHideAutoEventBus (Custom)
**Use when:** You want full control or use external animation systems (DOTween, LeanTween, MoreMountains)
**Best for:** Custom animations, integration with existing systems

```csharp
public class MyUI : ShowHideAutoEventBus<MyUI>
{
    protected override void ShowInternal()
    {
        // Your custom show animation
        LeanTween.scale(gameObject, Vector3.one, 0.3f)
            .setOnComplete(() => OnShowComplete());
    }

    protected override void HideInternal()
    {
        // Your custom hide animation
        LeanTween.scale(gameObject, Vector3.zero, 0.3f)
            .setOnComplete(() => OnHideComplete());
    }
}
```

## Combining EventBus with Existing Classes

Since ShowHideAnimation and ShowHideProcedural don't have EventBus support by default, you can combine them!

### Pattern: Multiple Inheritance Simulation

```csharp
// Create a bridge class that combines both
public class ShowHideProceduralWithEventBus<T> : ShowHideProcedural
    where T : ShowHideProceduralWithEventBus<T>
{
    protected virtual void OnEnable()
    {
        if (autoSubscribeToEventBus)
        {
            EventBus.Subscribe<OpenUI<T>>(evt => ShowUI());
            EventBus.Subscribe<CloseUI<T>>(evt => HideUI());
            EventBus.Subscribe<ToggleUI<T>>(evt => ToggleUI());
        }
    }

    protected virtual void OnDisable()
    {
        if (autoSubscribeToEventBus)
        {
            EventBus.Unsubscribe<OpenUI<T>>(evt => ShowUI());
            EventBus.Unsubscribe<CloseUI<T>>(evt => HideUI());
            EventBus.Unsubscribe<ToggleUI<T>>(evt => ToggleUI());
        }
    }
}

// Now use it!
public class MyUI : ShowHideProceduralWithEventBus<MyUI>
{
    // That's it! You get procedural animations + EventBus!
}
```

## Comparison Table

| Feature | ShowHideAnimation | ShowHideProcedural | ShowHideAutoEventBus |
|---------|-------------------|--------------------|--------------------|
| **EventBus Built-in** | ❌ No | ❌ No | ✅ Yes |
| **Animation Type** | Unity Animator | Code (Coroutines) | Custom |
| **Setup Complexity** | Medium (Animator) | Low (Inspector) | Low (Code) |
| **Flexibility** | High | Medium | Very High |
| **Best For** | Timeline animations | Simple transitions | Custom/External libs |
| **Code Required** | None | None | Minimal (2 methods) |

## Recommended Combinations

### 1. Simple UI with Fade (Easiest)
```csharp
// Just inherit, configure in Inspector
public class SettingsUI : ShowHideProcedural
{
    // Set in Inspector:
    // - Animation Type: Fade
    // - Duration: 0.3
    // - Auto Subscribe To EventBus: true
}

// Manual triggering (no EventBus)
settingsUI.ShowUI();
settingsUI.HideUI();
```

### 2. Simple UI with Fade + EventBus (Recommended)
```csharp
// See complete example in "Creating Your Settings Script" below
public class SettingsUI : ShowHideProceduralWithEventBus<SettingsUI>
{
    // Configure in Inspector, use EventBus
}

// Trigger from anywhere
UIManager.Open<SettingsUI>();
```

### 3. Complex Animation + EventBus
```csharp
public class SettingsUI : ShowHideAutoEventBus<SettingsUI>
{
    protected override void ShowInternal()
    {
        // Your custom animation (DOTween, MoreMountains, etc.)
        OnShowComplete();
    }

    protected override void HideInternal()
    {
        // Your custom animation
        OnHideComplete();
    }
}

UIManager.Open<SettingsUI>();
```
