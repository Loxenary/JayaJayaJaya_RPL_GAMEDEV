# Complete ShowHide + EventBus Guide

## 🎯 TL;DR - What You Need to Know

### For Most UI Panels (Recommended)

```csharp
// 1. Create your UI class
public class MyUI : ShowHideProceduralWithEventBus<MyUI>
{
    // That's it! No code needed for basic functionality
}

// 2. Use it anywhere
UIManager.Open<MyUI>();
UIManager.Close<MyUI>();
UIManager.Toggle<MyUI>();
```

### Configure in Inspector
- Animation Type: **Fade** (or Scale, Position, etc.)
- Duration: **0.3** seconds
- Auto Subscribe To EventBus: **✓ Checked**

---

## 📚 Complete Class Hierarchy

```
ShowHideBase (Abstract)
│
├─ ShowHideAnimation (Unity Animator)
│   └─ ShowHideAnimationWithEventBus<T> (+ EventBus)
│
├─ ShowHideProcedural (Code-based animations)
│   └─ ShowHideProceduralWithEventBus<T> (+ EventBus)
│
└─ ShowHideAutoEventBus<T> (Custom animations)
```

---

## 🤔 Which Class Should I Use?

### Use Case Matrix

| I want... | Use this class | File |
|-----------|---------------|------|
| **Simple fade/scale + EventBus** | `ShowHideProceduralWithEventBus<T>` | ShowHideBridges.cs |
| **Unity Animator + EventBus** | `ShowHideAnimationWithEventBus<T>` | ShowHideBridges.cs |
| **Custom animation + EventBus** | `ShowHideAutoEventBus<T>` | ShowHideAutoEventBus.cs |
| **No EventBus, just Animator** | `ShowHideAnimation` | ShowHideAnimation.cs |
| **No EventBus, just code animations** | `ShowHideProcedural` | ShowHideProcuderal.cs |

### Quick Decision

```
Do you want to use EventBus (UIManager.Open<T>())?
├─ YES
│  ├─ Simple animations? → ShowHideProceduralWithEventBus<T>
│  ├─ Unity Animator? → ShowHideAnimationWithEventBus<T>
│  └─ Custom/External lib? → ShowHideAutoEventBus<T>
│
└─ NO (just myUI.ShowUI())
   ├─ Simple animations? → ShowHideProcedural
   └─ Unity Animator? → ShowHideAnimation
```

---

## 📋 Complete Examples

### Example 1: Settings Panel (Most Common)

```csharp
public class SettingsUI : ShowHideProceduralWithEventBus<SettingsUI>
{
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => UIManager.Close<SettingsUI>());
    }

    protected override void ShowUIStart()
    {
        base.ShowUIStart();
        // Load settings
    }

    protected override void HideUIStart()
    {
        base.HideUIStart();
        // Save settings
    }
}

// Usage:
UIManager.Open<SettingsUI>();
```

**Inspector Setup:**
- Animation Type: Fade
- Duration: 0.3
- Canvas Group: Auto-assigned

### Example 2: Pause Menu

```csharp
public class PauseMenu : ShowHideProceduralWithEventBus<PauseMenu>
{
    protected override void ShowUIStart()
    {
        base.ShowUIStart();
        Time.timeScale = 0f; // Pause game
    }

    protected override void HideUIComplete()
    {
        base.HideUIComplete();
        Time.timeScale = 1f; // Resume game
    }
}

// Usage in game controller:
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        UIManager.Toggle<PauseMenu>();
    }
}
```

### Example 3: Inventory with Custom Animation

```csharp
public class InventoryUI : ShowHideAutoEventBus<InventoryUI>
{
    [SerializeField] private RectTransform panel;

    protected override void ShowInternal()
    {
        // Using LeanTween
        LeanTween.scale(panel, Vector3.one, 0.3f)
            .setEaseOutBack()
            .setOnComplete(() => OnShowComplete());
    }

    protected override void HideInternal()
    {
        LeanTween.scale(panel, Vector3.zero, 0.2f)
            .setEaseInBack()
            .setOnComplete(() => {
                gameObject.SetActive(false);
                OnHideComplete();
            });
    }
}
```

### Example 4: Complex Menu with Animator

```csharp
public class MainMenu : ShowHideAnimationWithEventBus<MainMenu>
{
    // Animator setup:
    // - Create "Show" animation clip
    // - Create "Hide" animation clip
    // - Add Animation Events at end of each clip:
    //   - Show clip: AnimationEvent_ShowComplete()
    //   - Hide clip: AnimationEvent_HideComplete()

    public void OnPlayButtonClicked()
    {
        UIManager.Chain()
            .Close<MainMenu>()
            .Open<GameHUD>()
            .Execute();
    }
}
```

---

## 🚀 Three Ways to Trigger UI

### 1. UIManager (Cleanest - Recommended)

```csharp
UIManager.Open<SettingsUI>();
UIManager.Close<SettingsUI>();
UIManager.Toggle<SettingsUI>();

// Fluent API
UIManager.Chain()
    .Close<MainMenu>()
    .Open<Settings>()
    .Execute();
```

### 2. EventBus (Explicit)

```csharp
EventBus.Publish(new OpenUI<SettingsUI>());
EventBus.Publish(new CloseUI<SettingsUI>());
EventBus.Publish(new ToggleUI<SettingsUI>());
```

### 3. Direct Call (No EventBus)

```csharp
settingsUI.ShowUI();
settingsUI.HideUI();
settingsUI.ToggleUI();
```

---

## 🎨 Animation Types (ShowHideProcedural)

| Type | Description | Requires |
|------|-------------|----------|
| **Fade** | Smooth opacity change | CanvasGroup |
| **Scale** | Grow/shrink | Nothing |
| **Position** | Slide in/out | Nothing |
| **ScaleAndFade** | Both scale + fade | CanvasGroup |

---

## 🔌 Integration Examples

### With Main Menu Button

```csharp
// In Unity Inspector:
// Button → OnClick() → Add UIManagerHelper.OpenSettings()

public class UIManagerHelper : MonoBehaviour
{
    public void OpenSettings() => UIManager.Open<SettingsUI>();
    public void OpenInventory() => UIManager.Open<InventoryUI>();
    public void OpenShop() => UIManager.Open<ShopUI>();
}
```

### With Input System

```csharp
public class InputHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            UIManager.Toggle<PauseMenu>();

        if (Input.GetKeyDown(KeyCode.I))
            UIManager.Toggle<InventoryUI>();

        if (Input.GetKeyDown(KeyCode.M))
            UIManager.Toggle<MapUI>();
    }
}
```

### With Game Events

```csharp
public class GameController : MonoBehaviour
{
    void OnPlayerDeath()
    {
        UIManager.Chain()
            .Close<GameHUD>()
            .Open<GameOverScreen>()
            .Execute();
    }

    void OnLevelComplete()
    {
        UIManager.Chain()
            .Close<GameHUD>()
            .Open<VictoryScreen>()
            .Execute();
    }
}
```

---

## 📁 File Reference

### Core System
- **ShowHideBase.cs** - Base class with event structs
- **ShowHideAnimation.cs** - Unity Animator implementation
- **ShowHideProcuderal.cs** - Code-based animations
- **ShowHideAutoEventBus.cs** - Auto EventBus helper

### EventBus Integration
- **ShowHideBridges.cs** - Bridge classes (Procedural/Animation + EventBus)
- **UIManager.cs** - Static helper with fluent API

### Documentation
- **COMPLETE_GUIDE.md** - This file
- **WHICH_CLASS_TO_USE.md** - Decision guide
- **QUICK_START.md** - Fast reference
- **README.md** - Full documentation
- **ARCHITECTURE.md** - System design

### Examples
- **SettingsUI.cs** - Complete Settings implementation
- **ShowHideUIExample.cs** - Various examples
- **ShowHideTestScene.cs** - Test script

---

## ✅ Setup Checklist

### For Each New UI Panel:

1. [ ] Create UI GameObject in scene
2. [ ] Add appropriate ShowHide component
3. [ ] Set **Auto Subscribe To EventBus: ✓**
4. [ ] Configure animation settings
5. [ ] Add CanvasGroup (if using Fade)
6. [ ] Wire up buttons/sliders
7. [ ] Test with `UIManager.Toggle<YourUI>()`

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| UI won't open | Check Auto Subscribe To EventBus is enabled |
| No animation | Verify Animation Type is set, duration > 0 |
| Fade not working | Add CanvasGroup component |
| EventBus not triggering | GameObject must be active (OnEnable called) |
| Compile error | Make sure class name matches generic parameter |

---

## 💡 Pro Tips

1. **Always use the Bridge classes** (ShowHideProceduralWithEventBus) unless you need custom animations
2. **UIManager is cleaner** than EventBus.Publish for most cases
3. **Use Chain()** for complex UI navigation
4. **Override ShowUIStart/Complete** for load/save logic
5. **Test with keyboard shortcuts** during development
6. **Use BetterLogger** to debug UI flow

---

## 🎓 Learning Path

1. ✅ Read this guide
2. ✅ Study [SettingsUI.cs](../Settings/SettingsUI.cs)
3. ✅ Follow [HOW_TO_USE_SETTINGS.md](../Settings/HOW_TO_USE_SETTINGS.md)
4. ✅ Create your first UI panel
5. ✅ Test with [ShowHideTestScene.cs](./ShowHideTestScene.cs)
6. ✅ Read [ARCHITECTURE.md](./ARCHITECTURE.md) for deep dive

---

## 🎉 You're Ready!

You now know everything to create awesome UI with the ShowHide + EventBus system!

**Quick reminder of the recommended approach:**

```csharp
// 1. Create class
public class MyAwesomeUI : ShowHideProceduralWithEventBus<MyAwesomeUI> { }

// 2. Configure in Inspector (Fade, 0.3s)

// 3. Use anywhere
UIManager.Open<MyAwesomeUI>();
```

That's it! Happy coding! 🚀
