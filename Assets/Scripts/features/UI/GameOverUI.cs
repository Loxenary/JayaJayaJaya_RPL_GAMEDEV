using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI controller for Game Over screen with death animation sequence.
/// Sequence: Red flash → Fade to black → "YOU DIED" grows → Buttons appear
/// Player can skip to buttons after fade to black.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image redFlashOverlay;
    [SerializeField] private Image blackOverlay;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Animation Settings")]
    [SerializeField] private string gameOverMessage = "YOU DIED";

    [Header("Red Flash")]
    [SerializeField] private Color flashColor = new Color(0.5f, 0f, 0f, 0.6f);
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float flashFadeOutDuration = 0.3f;

    [Header("Fade to Black")]
    [SerializeField] private float fadeToBlackDelay = 0.2f;
    [SerializeField] private float fadeToBlackDuration = 0.8f;

    [Header("Text Animation")]
    [SerializeField] private float textStartScale = 0.3f;
    [SerializeField] private float textEndScale = 1f;
    [SerializeField] private float textGrowDuration = 1.5f;
    [SerializeField] private float textStartDelay = 0.3f;

    [Header("Buttons")]
    [SerializeField] private float buttonsDelay = 2f;
    [SerializeField] private float buttonsFadeDuration = 0.5f;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnShow = true;

    private bool canSkipToButtons = false;
    private bool buttonsShown = false;
    private Coroutine deathSequence;

    private void Awake()
    {
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Hide everything on start
        HideGameOver();
    }

    private void OnEnable()
    {
        // Subscribe to player death event
        PlayerAttributes.onPlayerDead += ShowGameOver;
    }

    private void OnDisable()
    {
        // Unsubscribe from player death event
        PlayerAttributes.onPlayerDead -= ShowGameOver;
    }

    private void Update()
    {
        // Allow skipping to buttons after fade to black
        if (canSkipToButtons && !buttonsShown)
        {
            if (Input.anyKeyDown)
            {
                ShowButtonsImmediately();
            }
        }
    }

    /// <summary>
    /// Show game over screen with death animation
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (pauseGameOnShow)
        {
            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Start death animation sequence
        if (deathSequence != null)
            StopCoroutine(deathSequence);

        deathSequence = StartCoroutine(DeathAnimationSequence());
    }

    /// <summary>
    /// Death animation sequence coroutine
    /// </summary>
    private IEnumerator DeathAnimationSequence()
    {
        canSkipToButtons = false;
        buttonsShown = false;

        // Initialize states
        InitializeAnimationStates();

        // Phase 1: Red Flash
        yield return StartCoroutine(RedFlashAnimation());

        // Phase 2: Fade to Black
        yield return StartCoroutine(FadeToBlackAnimation());

        // Now pause the game (after visual feedback)
        if (pauseGameOnShow)
        {
            Time.timeScale = 0f;
        }

        // Player can now skip to buttons
        canSkipToButtons = true;

        // Phase 3: Text grows (using unscaled time for paused state)
        yield return StartCoroutine(TextGrowAnimation());

        // Phase 4: Buttons fade in
        yield return StartCoroutine(ButtonsFadeInAnimation());
    }

    private void InitializeAnimationStates()
    {
        // Red flash overlay - transparent
        if (redFlashOverlay != null)
        {
            redFlashOverlay.gameObject.SetActive(true);
            redFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        }

        // Black overlay - transparent
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.color = new Color(0f, 0f, 0f, 0f);
        }

        // Text - hidden and small
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
            gameOverText.text = gameOverMessage;
            gameOverText.transform.localScale = Vector3.one * textStartScale;
            gameOverText.alpha = 0f;
        }

        // Buttons - hidden
        if (buttonsGroup != null)
        {
            buttonsGroup.gameObject.SetActive(true);
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator RedFlashAnimation()
    {
        if (redFlashOverlay == null) yield break;

        // Flash in
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            redFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashColor.a * t);
            yield return null;
        }

        // Flash out
        elapsed = 0f;
        while (elapsed < flashFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / flashFadeOutDuration);
            redFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashColor.a * t);
            yield return null;
        }

        redFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }

    private IEnumerator FadeToBlackAnimation()
    {
        // Small delay before fade
        yield return new WaitForSeconds(fadeToBlackDelay);

        if (blackOverlay == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeToBlackDuration;
            // Ease out for smoother feel
            t = 1f - Mathf.Pow(1f - t, 2f);
            blackOverlay.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }

        blackOverlay.color = Color.black;
    }

    private IEnumerator TextGrowAnimation()
    {
        // Delay before text appears
        yield return new WaitForSecondsRealtime(textStartDelay);

        if (gameOverText == null) yield break;

        gameOverText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < textGrowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / textGrowDuration;

            // Ease out elastic-like effect
            float scaleT = EaseOutBack(t);
            float scale = Mathf.Lerp(textStartScale, textEndScale, scaleT);
            gameOverText.transform.localScale = Vector3.one * scale;

            // Fade in alpha
            float alphaT = Mathf.Clamp01(t * 2f); // Fade in faster
            gameOverText.alpha = alphaT;

            yield return null;
        }

        gameOverText.transform.localScale = Vector3.one * textEndScale;
        gameOverText.alpha = 1f;
    }

    private IEnumerator ButtonsFadeInAnimation()
    {
        // Delay before buttons appear
        yield return new WaitForSecondsRealtime(buttonsDelay - textGrowDuration - textStartDelay);

        ShowButtonsImmediately();
    }

    private void ShowButtonsImmediately()
    {
        if (buttonsShown) return;
        buttonsShown = true;
        canSkipToButtons = false;

        StartCoroutine(FadeInButtons());
    }

    private IEnumerator FadeInButtons()
    {
        if (buttonsGroup == null) yield break;

        buttonsGroup.interactable = true;
        buttonsGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < buttonsFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / buttonsFadeDuration;
            buttonsGroup.alpha = t;
            yield return null;
        }

        buttonsGroup.alpha = 1f;

        // Make sure buttons are interactable
        if (restartButton != null)
            restartButton.interactable = true;
        if (quitButton != null)
            quitButton.interactable = true;
    }

    /// <summary>
    /// Easing function for smooth animation
    /// </summary>
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// Hide game over screen
    /// </summary>
    public void HideGameOver()
    {
        if (deathSequence != null)
        {
            StopCoroutine(deathSequence);
            deathSequence = null;
        }

        canSkipToButtons = false;
        buttonsShown = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pauseGameOnShow)
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Restart button callback
    /// </summary>
    private async void OnRestartClicked()
    {
        // Disable buttons to prevent double-click
        if (restartButton != null)
            restartButton.interactable = false;
        if (quitButton != null)
            quitButton.interactable = false;

        // Get SceneService to reload current scene
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            RestartManager.Restart();
            await sceneService.ReloadScene(addTransition: true);
            ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
        }
        else
        {
            // Fallback: Reload using Unity SceneManager
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }

    /// <summary>
    /// Quit button callback
    /// </summary>
    private async void OnQuitClicked()
    {
        // Disable buttons to prevent double-click
        if (restartButton != null)
            restartButton.interactable = false;
        if (quitButton != null)
            quitButton.interactable = false;

        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            if (sceneService.IsTestMode)
            {
                await sceneService.ReloadScene(addTransition: true);
            }
            else
            {
                await sceneService.LoadScene(SceneEnum.MAIN_MENU, true);
            }
            ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
