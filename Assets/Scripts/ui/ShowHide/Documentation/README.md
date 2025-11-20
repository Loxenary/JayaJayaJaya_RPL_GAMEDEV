# ShowHide System with Generic EventBus Integration

This system provides a powerful way to control UI panels using generic EventBus events.

## Quick Start

### Simple Usage (Recommended)

Derive from `ShowHideAutoEventBus<T>` for automatic EventBus integration:

```csharp
public class SettingsPanel : ShowHideAutoEventBus<SettingsPanel>
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
}
```

Then trigger it from anywhere:

```csharp
// Open the settings panel
EventBus.Publish(new OpenUI<SettingsPanel>());

// Close the settings panel
EventBus.Publish(new CloseUI<SettingsPanel>());

// Toggle the settings panel
EventBus.Publish(new ToggleUI<SettingsPanel>());
```

## Available Event Types

### `OpenUI<T>`
Opens/shows a UI panel of type T.

```csharp
EventBus.Publish(new OpenUI<SettingsPanel>());
```

### `CloseUI<T>`
Closes/hides a UI panel of type T.

```csharp
EventBus.Publish(new CloseUI<SettingsPanel>());
```

### `ToggleUI<T>`
Toggles a UI panel between shown and hidden states.

```csharp
EventBus.Publish(new ToggleUI<PauseMenu>());
```

## Advanced Usage

### Manual EventBus Subscription

If you need more control, derive from `ShowHideBase` directly:

```csharp
public class CustomUI : ShowHideBase
{
    protected override void SubscribeToEventBus()
    {
        EventBus.Subscribe<OpenUI<CustomUI>>(OnOpenUI);
        EventBus.Subscribe<CloseUI<CustomUI>>(OnCloseUI);
        EventBus.Subscribe<ToggleUI<CustomUI>>(OnToggleUI);
    }

    protected override void UnsubscribeFromEventBus()
    {
        EventBus.Unsubscribe<OpenUI<CustomUI>>(OnOpenUI);
        EventBus.Unsubscribe<CloseUI<CustomUI>>(OnCloseUI);
        EventBus.Unsubscribe<ToggleUI<CustomUI>>(OnToggleUI);
    }

    private void OnOpenUI(OpenUI<CustomUI> evt) => ShowUI();
    private void OnCloseUI(CloseUI<CustomUI> evt) => HideUI();
    private void OnToggleUI(ToggleUI<CustomUI> evt) => ToggleUI();

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

### Custom Event Handling

Override event handlers in `ShowHideAutoEventBus<T>` for custom behavior:

```csharp
public class InventoryUI : ShowHideAutoEventBus<InventoryUI>
{
    [SerializeField] private AudioClip openSound;

    protected override void OnOpenUIEvent(OpenUI<InventoryUI> evt)
    {
        // Play sound before opening
        AudioSource.PlayClipAtPoint(openSound, Camera.main.transform.position);

        // Call base to execute the standard show logic
        base.OnOpenUIEvent(evt);
    }

    protected override void ShowInternal()
    {
        // Your show animation
        LeanTween.scale(gameObject, Vector3.one, 0.3f)
            .setEaseOutBack()
            .setOnComplete(() => OnShowComplete());
    }

    protected override void HideInternal()
    {
        // Your hide animation
        LeanTween.scale(gameObject, Vector3.zero, 0.2f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
                OnHideComplete();
            });
    }
}
```

### Passing Data with Events (Optional)

The event structs have an optional `Data` field:

```csharp
// Open UI with reference to the instance
var settingsPanel = FindObjectOfType<SettingsPanel>();
EventBus.Publish(new OpenUI<SettingsPanel>(settingsPanel));

// In your event handler:
protected override void OnOpenUIEvent(OpenUI<SettingsPanel> evt)
{
    if (evt.Data != null)
    {
        // Use the specific instance
        evt.Data.ShowUI();
    }
    else
    {
        // Default behavior - show this instance
        ShowUI();
    }
}
```

## Practical Examples

### Example 1: Pause Menu

```csharp
public class PauseMenu : ShowHideAutoEventBus<PauseMenu>
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventBus.Publish(new ToggleUI<PauseMenu>());
        }
    }

    protected override void ShowInternal()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f; // Pause game
        OnShowComplete();
    }

    protected override void HideInternal()
    {
        Time.timeScale = 1f; // Resume game
        gameObject.SetActive(false);
        OnHideComplete();
    }
}
```

### Example 2: Button Integration

```csharp
public class UIButtons : MonoBehaviour
{
    public void OnSettingsButtonClick()
    {
        EventBus.Publish(new OpenUI<SettingsPanel>());
    }

    public void OnInventoryButtonClick()
    {
        EventBus.Publish(new ToggleUI<InventoryUI>());
    }

    public void OnCloseButtonClick()
    {
        // Close all UI panels
        EventBus.Publish(new CloseUI<SettingsPanel>());
        EventBus.Publish(new CloseUI<InventoryUI>());
        EventBus.Publish(new CloseUI<PauseMenu>());
    }
}
```

### Example 3: UI State Machine

```csharp
public class MainMenuController : MonoBehaviour
{
    public void NavigateToSettings()
    {
        EventBus.Publish(new CloseUI<MainMenu>());
        EventBus.Publish(new OpenUI<SettingsPanel>());
    }

    public void BackToMainMenu()
    {
        EventBus.Publish(new CloseUI<SettingsPanel>());
        EventBus.Publish(new OpenUI<MainMenu>());
    }
}
```

## Configuration

In the Inspector, you'll see:

- **Auto Subscribe To Event Bus**: Enable/disable automatic EventBus subscription (default: true)

Set this to `false` if you want to manually control when the UI subscribes to events.

## Benefits

1. **Type-Safe**: Generic events ensure compile-time type checking
2. **Decoupled**: No direct references needed between UI and caller code
3. **Flexible**: Works from anywhere - buttons, code, input systems, etc.
4. **Clean**: Minimal boilerplate with `ShowHideAutoEventBus<T>`
5. **Debuggable**: EventBus provides debugging tools to track publish/subscribe

## Tips

- Use `ShowHideAutoEventBus<T>` for most cases (less boilerplate)
- Use `ShowHideBase` when you need full control
- Always call `OnShowComplete()` after show animation finishes
- Always call `OnHideComplete()` after hide animation finishes
- The generic type `T` should be the derived class itself (CRTP pattern)

## Architecture

```
OpenUI<T>, CloseUI<T>, ToggleUI<T>
         ↓ (EventBus)
ShowHideAutoEventBus<T> or ShowHideBase
         ↓
    Your UI Implementation
```
