# How to Use the Settings UI

## Step-by-Step Setup

### 1. Create Your Settings Panel in Unity

1. Create a new UI Canvas (if you don't have one)
2. Create a Panel GameObject named "SettingsPanel"
3. Add your UI elements (buttons, sliders, toggles)
4. Add the `SettingsUI` component to the SettingsPanel

### 2. Configure in Inspector

Select your SettingsPanel and configure:

```
SettingsUI Component:
├─ EventBus Settings
│  └─ Auto Subscribe To EventBus: ✓ (checked)
│
├─ Procedural Animation Settings
│  ├─ Animation Type: Fade (or Scale, ScaleAndFade, etc.)
│  ├─ Animation Duration: 0.3
│  └─ Animation Curve: Default (or customize)
│
├─ Fade Animation
│  └─ Canvas Group: [Auto-assigned]
│
└─ Settings UI Components
   ├─ Close Button: [Drag your close button here]
   ├─ Volume Slider: [Drag your volume slider here]
   └─ Fullscreen Toggle: [Drag your toggle here]
```

### 3. Open the Settings Panel

From anywhere in your code:

```csharp
// Method 1: Using UIManager (Recommended)
UIManager.Open<SettingsUI>();

// Method 2: Using EventBus directly
EventBus.Publish(new OpenUI<SettingsUI>());

// Method 3: From a button click (add this to your button's OnClick)
public void OnSettingsButtonClick()
{
    UIManager.Open<SettingsUI>();
}
```

### 4. Close the Settings Panel

```csharp
// Method 1: Using UIManager
UIManager.Close<SettingsUI>();

// Method 2: Using EventBus
EventBus.Publish(new CloseUI<SettingsUI>());

// Method 3: From inside SettingsUI (already implemented)
// The close button automatically calls UIManager.Close<SettingsUI>()
```

### 5. Toggle the Settings Panel

```csharp
// Using UIManager
UIManager.Toggle<SettingsUI>();

// Using EventBus
EventBus.Publish(new ToggleUI<SettingsUI>());

// Common use case: ESC key to toggle
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        UIManager.Toggle<SettingsUI>();
    }
}
```

## Common Usage Patterns

### Pattern 1: Open Settings from Main Menu

```csharp
public class MainMenuUI : MonoBehaviour
{
    public void OnSettingsButtonClicked()
    {
        UIManager.Open<SettingsUI>();
    }
}
```

### Pattern 2: Open Settings with Keyboard Shortcut

```csharp
public class GameInputHandler : MonoBehaviour
{
    void Update()
    {
        // Press F1 to open settings
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UIManager.Open<SettingsUI>();
        }
    }
}
```

### Pattern 3: Navigate Between Menus

```csharp
public class MenuNavigator : MonoBehaviour
{
    public void GoToSettings()
    {
        // Close main menu and open settings
        UIManager.Chain()
            .Close<MainMenuUI>()
            .Open<SettingsUI>()
            .Execute();
    }

    public void BackToMainMenu()
    {
        // Close settings and return to main menu
        UIManager.Chain()
            .Close<SettingsUI>()
            .Open<MainMenuUI>()
            .Execute();
    }
}
```

### Pattern 4: Pause Game and Open Settings

```csharp
public class PauseManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0f; // Pause game
            UIManager.Open<SettingsUI>();
        }
    }
}
```

## Customizing the Settings UI

### Adding New Settings

Edit `SettingsUI.cs` and add your components:

```csharp
[Header("Settings UI Components")]
[SerializeField] private Button closeButton;
[SerializeField] private Slider volumeSlider;
[SerializeField] private Toggle fullscreenToggle;
[SerializeField] private Slider brightnessSlider;  // NEW
[SerializeField] private Dropdown qualityDropdown; // NEW

private void Awake()
{
    // Wire up your new components
    if (brightnessSlider != null)
    {
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
    }

    if (qualityDropdown != null)
    {
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }
}

private void OnBrightnessChanged(float value)
{
    // Apply brightness
    RenderSettings.ambientIntensity = value;
}

private void OnQualityChanged(int index)
{
    // Apply quality setting
    QualitySettings.SetQualityLevel(index);
}
```

### Changing Animation Type

In the Inspector, change the Animation Type:

- **Fade**: Smooth fade in/out (requires CanvasGroup)
- **Scale**: Grow/shrink animation
- **Position**: Slide in/out
- **ScaleAndFade**: Combined scale and fade

### Using Different Animation Systems

If you want to use LeanTween, DOTween, or MoreMountains Feel:

1. Use `SettingsUI_CustomAnimation` from the SettingsUI.cs file
2. Uncomment the animation code you need
3. Implement your custom animation in ShowInternal/HideInternal

## Troubleshooting

### Settings Won't Open

**Check:**
1. Is `Auto Subscribe To EventBus` checked in Inspector?
2. Is the GameObject enabled in the scene?
3. Check Console for any errors

**Solution:**
```csharp
// Try direct call to verify the component works
FindObjectOfType<SettingsUI>().ShowUI();
```

### Animation Not Working

**Check:**
1. Animation Type is set correctly
2. For Fade: CanvasGroup component exists
3. Animation Duration is not 0

### EventBus Not Triggering

**Check:**
1. Is the component enabled (OnEnable called)?
2. Try logging in OnOpenUIEvent to verify

```csharp
protected override void OnOpenUIEvent(OpenUI<SettingsUI> evt)
{
    Debug.Log("Settings UI received open event!");
    base.OnOpenUIEvent(evt);
}
```

## Inspector Setup Checklist

- [ ] SettingsUI component added to GameObject
- [ ] Auto Subscribe To EventBus: CHECKED
- [ ] Animation Type: Set (Fade recommended)
- [ ] Animation Duration: 0.3 (or desired value)
- [ ] Canvas Group: Assigned (for Fade animations)
- [ ] Close Button: Assigned
- [ ] Volume Slider: Assigned (optional)
- [ ] Fullscreen Toggle: Assigned (optional)
- [ ] GameObject is active in hierarchy

## Quick Test

Add this to any MonoBehaviour to test:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        Debug.Log("Testing Settings UI...");
        UIManager.Toggle<SettingsUI>();
    }
}
```

Then press 'T' in Play mode to test!
