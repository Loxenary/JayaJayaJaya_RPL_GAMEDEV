using UnityEngine;
using UnityEngine.UI;
using CustomLogger;

/// <summary>
/// Complete Settings UI implementation using ShowHideProcedural with EventBus.
/// This is the RECOMMENDED approach for most UI panels.
///
/// Features:
/// - Automatic fade animation (configurable in Inspector)
/// - EventBus integration (open from anywhere)
/// - Logging integration
///
/// Usage:
///   UIManager.Open<SettingsUI>();
///   UIManager.Close<SettingsUI>();
///   UIManager.Toggle<SettingsUI>();
///
/// Or with EventBus directly:
///   EventBus.Publish(new OpenUI<SettingsUI>());
/// </summary>
public class SettingsUI : ShowHideProceduralWithEventBus<SettingsUI>
{
    [Header("Settings UI Components")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Awake()
    {
        // Wire up button events
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }

        // Load saved settings
        LoadSettings();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
        }
    }

    #region ShowHide Hooks (Optional - for logging/custom behavior)

    protected override void ShowUIStart()
    {
        base.ShowUIStart();
        BetterLogger.Log("Settings panel opening...", BetterLogger.LogCategory.UI);

        // Refresh settings when shown
        LoadSettings();
    }

    protected override void ShowUIComplete()
    {
        base.ShowUIComplete();
        BetterLogger.Log("Settings panel opened", BetterLogger.LogCategory.UI);
    }

    protected override void HideUIStart()
    {
        base.HideUIStart();
        BetterLogger.Log("Settings panel closing...", BetterLogger.LogCategory.UI);

        // Save settings when closing
        SaveSettings();
    }

    protected override void HideUIComplete()
    {
        base.HideUIComplete();
        BetterLogger.Log("Settings panel closed", BetterLogger.LogCategory.UI);
    }

    #endregion

    #region Settings Logic

    private void LoadSettings()
    {
        // Load from PlayerPrefs or your settings system
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Settings_Volume", 1.0f);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        BetterLogger.Log("Settings loaded", BetterLogger.LogCategory.System);
    }

    private void SaveSettings()
    {
        // Save to PlayerPrefs or your settings system
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("Settings_Volume", volumeSlider.value);
        }

        PlayerPrefs.Save();
        BetterLogger.Log("Settings saved", BetterLogger.LogCategory.System);
    }

    private void OnVolumeChanged(float value)
    {
        // Apply volume change
        AudioListener.volume = value;
        BetterLogger.Log($"Volume changed to {value:F2}", BetterLogger.LogCategory.Audio);
    }

    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        BetterLogger.Log($"Fullscreen: {isFullscreen}", BetterLogger.LogCategory.System);
    }

    private void OnCloseButtonClicked()
    {
        // Close this panel
        UIManager.Close<SettingsUI>();
    }

    #endregion

    #region Public API (Optional - for direct access if needed)

    /// <summary>
    /// Reset all settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = 1.0f;
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = true;
        }

        BetterLogger.Log("Settings reset to defaults", BetterLogger.LogCategory.System);
    }

    #endregion
}

// ============================================================================
// ALTERNATIVE IMPLEMENTATIONS (Choose based on your needs)
// ============================================================================

#region Alternative 1: Using ShowHideAutoEventBus (Custom Animation)

/// <summary>
/// Settings UI with custom animation (e.g., for LeanTween, DOTween, MoreMountains Feel)
/// Use this if you need more control over the animation.
/// </summary>
public class SettingsUI_CustomAnimation : ShowHideAutoEventBus<SettingsUI_CustomAnimation>
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    protected override void ShowInternal()
    {
        gameObject.SetActive(true);

        // Example with LeanTween (uncomment if you have LeanTween)
        // LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
        //     .setEaseOutQuad()
        //     .setOnComplete(() => OnShowComplete());

        // Example with MoreMountains Feel (uncomment if you have MMF Player)
        // MMF_Player feedbacks = GetComponent<MMF_Player>();
        // feedbacks?.PlayFeedbacks();

        // For now, instant show
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        OnShowComplete();
    }

    protected override void HideInternal()
    {
        // Example with LeanTween (uncomment if you have LeanTween)
        // LeanTween.alphaCanvas(canvasGroup, 0f, fadeDuration)
        //     .setEaseInQuad()
        //     .setOnComplete(() =>
        //     {
        //         gameObject.SetActive(false);
        //         OnHideComplete();
        //     });

        // For now, instant hide
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        OnHideComplete();
    }
}

#endregion

#region Alternative 2: Using ShowHideAnimation (Unity Animator)

/// <summary>
/// Settings UI using Unity Animator.
/// Requires:
/// 1. Animator component with "Show" and "Hide" animations
/// 2. Animation events calling AnimationEvent_ShowComplete() and AnimationEvent_HideComplete()
/// </summary>
public class SettingsUI_WithAnimator : ShowHideAnimationWithEventBus<SettingsUI_WithAnimator>
{
    // No code needed! Just:
    // 1. Set up Animator with Show/Hide triggers
    // 2. Add Animation Events at the end of animations
    // 3. Configure trigger names in Inspector

    // Optional: Add your settings logic here (buttons, sliders, etc.)
}

#endregion

#region Alternative 3: Manual EventBus (Full Control)

/// <summary>
/// Settings UI with manual EventBus subscription.
/// Use this only if you need very specific control over event handling.
/// </summary>
public class SettingsUI_Manual : ShowHideProcedural
{
    protected override void OnEnable()
    {
        base.OnEnable();

        // Manual subscription with custom handling
        EventBus.Subscribe<OpenUI<SettingsUI_Manual>>(OnOpenEvent);
        EventBus.Subscribe<CloseUI<SettingsUI_Manual>>(OnCloseEvent);
        EventBus.Subscribe<ToggleUI<SettingsUI_Manual>>(OnToggleEvent);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.Unsubscribe<OpenUI<SettingsUI_Manual>>(OnOpenEvent);
        EventBus.Unsubscribe<CloseUI<SettingsUI_Manual>>(OnCloseEvent);
        EventBus.Unsubscribe<ToggleUI<SettingsUI_Manual>>(OnToggleEvent);
    }

    private void OnOpenEvent(OpenUI<SettingsUI_Manual> evt)
    {
        // Custom pre-open logic
        BetterLogger.Log("Custom open logic before showing", BetterLogger.LogCategory.UI);
        ShowUI();
    }

    private void OnCloseEvent(CloseUI<SettingsUI_Manual> evt)
    {
        // Custom pre-close logic
        BetterLogger.Log("Custom close logic before hiding", BetterLogger.LogCategory.UI);
        HideUI();
    }

    private void OnToggleEvent(ToggleUI<SettingsUI_Manual> evt)
    {
        // Custom toggle logic
        BetterLogger.Log("Custom toggle logic", BetterLogger.LogCategory.UI);
        ToggleUI();
    }
}

#endregion
