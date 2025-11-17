# ShowHide Class Comparison

## Visual Comparison Chart

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ShowHideBase                                     │
│                      (Abstract Base Class)                               │
│  • Manages IsVisible / IsTransitioning state                            │
│  • Provides lifecycle hooks (Start/Complete)                            │
│  • EventBus subscription support                                        │
└─────────────────────────────────────────────────────────────────────────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        │                         │                         │
        ▼                         ▼                         ▼
┌───────────────────┐   ┌───────────────────┐   ┌───────────────────┐
│ShowHideAnimation  │   │ShowHideProcedural │   │ShowHideAutoEventBus│
│                   │   │                   │   │                   │
│Uses: Animator     │   │Uses: Coroutines   │   │Uses: Custom Code  │
│Setup: Medium      │   │Setup: Easy        │   │Setup: Code-based  │
│EventBus: No       │   │EventBus: No       │   │EventBus: Yes      │
└───────────────────┘   └───────────────────┘   └───────────────────┘
        │                         │
        │ +EventBus               │ +EventBus
        ▼                         ▼
┌───────────────────┐   ┌───────────────────┐
│Animation          │   │Procedural         │
│WithEventBus<T>    │   │WithEventBus<T>    │
│                   │   │                   │
│= Animator +       │   │= Coroutines +     │
│  EventBus         │   │  EventBus         │
│                   │   │                   │
│✅ RECOMMENDED     │   │✅ RECOMMENDED     │
│   (for Animator)  │   │   (for most UIs)  │
└───────────────────┘   └───────────────────┘
```

## Side-by-Side Comparison

### Class Feature Matrix

| Feature | ShowHideAnimation | ShowHideProcedural | ShowHideAutoEventBus | AnimationWithEventBus | ProceduralWithEventBus |
|---------|-------------------|--------------------|--------------------|---------------------|----------------------|
| **EventBus Built-in** | ❌ | ❌ | ✅ | ✅ | ✅ |
| **Animation System** | Unity Animator | Coroutines | Custom | Unity Animator | Coroutines |
| **Setup Difficulty** | Medium | Easy | Easy | Medium | Easy |
| **Code Required** | None | None | Minimal | None | None |
| **Inspector Config** | Animator + Triggers | Animation Type | None | Animator + Triggers | Animation Type |
| **Flexibility** | High | Medium | Very High | High | Medium |
| **Best For** | Timeline animations | Simple UI | Custom libs | Timeline + EventBus | Simple + EventBus |

### Usage Comparison

#### 1. Opening the UI

```csharp
// ShowHideAnimation or ShowHideProcedural (No EventBus)
myUI.ShowUI();  // Direct call only

// ShowHideAutoEventBus, AnimationWithEventBus, ProceduralWithEventBus
UIManager.Open<MyUI>();           // ✅ Works
EventBus.Publish(new OpenUI<MyUI>());  // ✅ Works
myUI.ShowUI();                    // ✅ Also works
```

#### 2. Class Declaration

```csharp
// ShowHideAnimation (No EventBus)
public class MyUI : ShowHideAnimation
{
    // Just configure Animator in Inspector
}

// ShowHideProcedural (No EventBus)
public class MyUI : ShowHideProcedural
{
    // Just configure animation type in Inspector
}

// ShowHideAutoEventBus (Custom animation)
public class MyUI : ShowHideAutoEventBus<MyUI>
{
    protected override void ShowInternal() { /* your code */ }
    protected override void HideInternal() { /* your code */ }
}

// ShowHideAnimationWithEventBus (Animator + EventBus)
public class MyUI : ShowHideAnimationWithEventBus<MyUI>
{
    // Configure Animator in Inspector
    // EventBus automatically integrated!
}

// ShowHideProceduralWithEventBus (Coroutines + EventBus)
public class MyUI : ShowHideProceduralWithEventBus<MyUI>
{
    // Configure animation type in Inspector
    // EventBus automatically integrated!
}
```

#### 3. Inspector Configuration

| Class | Inspector Requirements |
|-------|----------------------|
| **ShowHideAnimation** | • Animator component<br>• Show/Hide trigger names<br>• Animation Events in clips |
| **ShowHideProcedural** | • Animation Type dropdown<br>• Duration slider<br>• CanvasGroup (for fade) |
| **ShowHideAutoEventBus** | • Auto Subscribe checkbox<br>• (Animation in code) |
| **AnimationWithEventBus** | • Everything from ShowHideAnimation<br>• Auto Subscribe checkbox |
| **ProceduralWithEventBus** | • Everything from ShowHideProcedural<br>• Auto Subscribe checkbox |

## Recommendation Matrix

### Choose Based on Your Needs

```
┌─────────────────────────────────────────────────────────────┐
│ "I want simple fade/scale animation + EventBus"             │
│ ✅ USE: ShowHideProceduralWithEventBus<T>                   │
│ Setup: 2 min | Code: None | Rating: ⭐⭐⭐⭐⭐              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ "I have complex Unity Animator animations + need EventBus"  │
│ ✅ USE: ShowHideAnimationWithEventBus<T>                    │
│ Setup: 10 min | Code: None | Rating: ⭐⭐⭐⭐               │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ "I want to use DOTween/LeanTween/MMF + EventBus"           │
│ ✅ USE: ShowHideAutoEventBus<T>                             │
│ Setup: 5 min | Code: 2 methods | Rating: ⭐⭐⭐⭐⭐         │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ "I don't need EventBus, just simple animations"             │
│ ✅ USE: ShowHideProcedural                                   │
│ Setup: 2 min | Code: None | Rating: ⭐⭐⭐⭐                │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ "I don't need EventBus, have Unity Animator"                │
│ ✅ USE: ShowHideAnimation                                    │
│ Setup: 10 min | Code: None | Rating: ⭐⭐⭐                 │
└─────────────────────────────────────────────────────────────┘
```

## Real-World Use Cases

### Settings Panel
**Best Choice:** `ShowHideProceduralWithEventBus<SettingsUI>`
- Simple fade animation
- Open from anywhere with UIManager
- No complex animation needed

### Main Menu
**Best Choice:** `ShowHideAnimationWithEventBus<MainMenuUI>`
- Designer-created complex animations
- Multiple UI elements with different timings
- EventBus for navigation

### HUD/Inventory
**Best Choice:** `ShowHideAutoEventBus<InventoryUI>`
- Custom animations with LeanTween/DOTween
- Integration with existing animation library
- Complex show/hide logic

### Tutorial Popups
**Best Choice:** `ShowHideProceduralWithEventBus<TutorialPopup>`
- Quick setup
- Simple scale/fade
- Trigger from game events

### Pause Menu
**Best Choice:** `ShowHideProceduralWithEventBus<PauseMenuUI>`
- Simple scale animation
- ESC key to toggle
- Easy setup

## Migration Guide

### From ShowHideProcedural → ProceduralWithEventBus

```csharp
// Before (No EventBus)
public class MyUI : ShowHideProcedural
{
}

// After (With EventBus)
public class MyUI : ShowHideProceduralWithEventBus<MyUI>
{
}

// Usage changes from:
myUI.ShowUI();

// To:
UIManager.Open<MyUI>();
```

### From ShowHideAnimation → AnimationWithEventBus

```csharp
// Before (No EventBus)
public class MyUI : ShowHideAnimation
{
}

// After (With EventBus)
public class MyUI : ShowHideAnimationWithEventBus<MyUI>
{
}

// Usage changes from:
myUI.ShowUI();

// To:
UIManager.Open<MyUI>();
```

## Performance Comparison

| Class | Memory | CPU (Show/Hide) | GC Alloc |
|-------|--------|-----------------|----------|
| ShowHideAnimation | Low | Very Low (GPU) | None |
| ShowHideProcedural | Low | Low | Minimal (coroutine) |
| ShowHideAutoEventBus | Low | Depends on impl | Depends on impl |
| WithEventBus variants | Low | +Negligible | None (struct events) |

**Note:** EventBus adds negligible overhead (dictionary lookup + delegate call)

## Summary Table

### Quick Reference

| I Need... | Use This |
|-----------|----------|
| 🎯 **Most common case** | `ShowHideProceduralWithEventBus<T>` |
| 🎨 Complex animations | `ShowHideAnimationWithEventBus<T>` |
| 🔧 Full control | `ShowHideAutoEventBus<T>` |
| 🚫 No EventBus needed | `ShowHideProcedural` or `ShowHideAnimation` |
| 📚 Learning/Tutorial | Start with `ShowHideProceduralWithEventBus<T>` |

## Final Recommendation

### For 90% of UI Panels:
```csharp
public class YourUI : ShowHideProceduralWithEventBus<YourUI>
{
    // Configure Fade animation in Inspector
    // Use: UIManager.Open<YourUI>();
}
```

### Why?
- ✅ Minimal setup
- ✅ No code required
- ✅ EventBus integrated
- ✅ Good performance
- ✅ Covers most use cases
- ✅ Easy to understand
- ✅ Inspector-configurable
