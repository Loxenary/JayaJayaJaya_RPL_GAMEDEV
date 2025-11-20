# ShowHide EventBus - Quick Start Guide

## TL;DR - The Fastest Way

### 1. Create Your UI Class (One-Time Setup)

```csharp
public class SettingsUI : ShowHideAutoEventBus<SettingsUI>
{
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
```

### 2. Use It Anywhere in Your Code

```csharp
// Direct EventBus approach
EventBus.Publish(new OpenUI<SettingsUI>());
EventBus.Publish(new CloseUI<SettingsUI>());
EventBus.Publish(new ToggleUI<SettingsUI>());

// Or using UIManager (cleaner)
UIManager.Open<SettingsUI>();
UIManager.Close<SettingsUI>();
UIManager.Toggle<SettingsUI>();
```

That's it! ✨

---

## Three Ways to Use This System

### Option 1: Direct EventBus (Most Explicit)

```csharp
EventBus.Publish(new OpenUI<MyPanel>());
```

**Pros:** Direct, clear what's happening
**Use when:** You want explicit control

### Option 2: UIManager (Cleanest)

```csharp
UIManager.Open<MyPanel>();
```

**Pros:** Cleaner syntax, provides tracking and fluent API
**Use when:** You want the nicest API (recommended)

### Option 3: UIManager Fluent API (Most Powerful)

```csharp
UIManager.Chain()
    .Close<MainMenu>()
    .Open<Settings>()
    .Execute();
```

**Pros:** Chain multiple operations, great for navigation
**Use when:** You need to do multiple UI operations at once

---

## Common Patterns

### Pattern 1: Button Click

```csharp
public void OnSettingsButtonClicked()
{
    UIManager.Open<SettingsUI>();
}
```

### Pattern 2: Keyboard Shortcut

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        UIManager.Toggle<PauseMenu>();
    }
}
```

### Pattern 3: Menu Navigation

```csharp
public void NavigateToSettings()
{
    UIManager.Chain()
        .Close<MainMenu>()
        .Open<SettingsMenu>()
        .Execute();
}
```

### Pattern 4: Close All UI

```csharp
public void CloseAllPanels()
{
    UIManager.CloseAll();
}
```

---

## File Structure

Your project now has:

```
Assets/Scripts/ui/ShowHide/
├── ShowHideBase.cs               (Base class with events)
├── ShowHideAutoEventBus.cs       (Auto-subscription helper)
├── UIManager.cs                  (Static helper class)
├── ShowHideUIExample.cs          (Manual subscription examples)
├── ShowHideTestScene.cs          (Test/demo script)
├── README.md                     (Full documentation)
└── QUICK_START.md                (This file)
```

---

## Examples in Your Project

Check these files for complete examples:
- [ShowHideUIExample.cs](./ShowHideUIExample.cs) - Manual EventBus subscription
- [ShowHideAutoEventBus.cs](./ShowHideAutoEventBus.cs) - Auto subscription examples
- [UIManager.cs](./UIManager.cs) - UIManager usage examples
- [ShowHideTestScene.cs](./ShowHideTestScene.cs) - Testing/demo scene

---

## Full Documentation

See [README.md](./README.md) for complete documentation including:
- Advanced usage patterns
- Custom event handling
- Passing data with events
- Animation integration
- Architecture details

---

## Troubleshooting

**UI doesn't open?**
- Make sure your UI class derives from `ShowHideAutoEventBus<YourClass>`
- Check that `autoSubscribeToEventBus` is enabled in Inspector
- Verify the GameObject is enabled in the scene

**EventBus subscription not working?**
- Ensure OnEnable/OnDisable are being called
- Check that you're using the correct type in the generic parameter

**Need more help?**
- See full examples in the example files
- Read the complete [README.md](./README.md)
- Check Unity Console for any errors
