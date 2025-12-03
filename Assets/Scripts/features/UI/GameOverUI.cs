using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for Game Over screen
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string gameOverMessage = "YOU DIED";
    [SerializeField] private bool pauseGameOnShow = true;

    private void Awake()
    {
        // Make sure canvas can work during pause (Time.timeScale = 0)
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            // Canvas will work even when Time.timeScale = 0
            canvas.worldCamera = null; // Use overlay mode
        }

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Hide panel on start
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

    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }

        if (pauseGameOnShow)
        {
            Time.timeScale = 0f;
            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Make sure buttons are interactable
        if (restartButton != null)
            restartButton.interactable = true;
        if (quitButton != null)
            quitButton.interactable = true;
    }

    /// <summary>
    /// Hide game over screen
    /// </summary>
    public void HideGameOver()
    {
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

        // Keep game paused and UI visible until reload completes
        // Time.timeScale stays at 0 to freeze the game

        // Get SceneService to reload current scene
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            // ReloadScene will handle everything - scene loads fresh with Time.timeScale = 1
            await sceneService.ReloadScene(addTransition: true);
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
    /// Quit button callback (load main menu or quit game)
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
            // In test mode, just reload the test scene as "quit" behavior
            // In normal mode, load main menu
            if (sceneService.IsTestMode)
            {
                await sceneService.ReloadScene(addTransition: true);
            }
            else
            {
                await sceneService.LoadScene(SceneEnum.MAIN_MENU, true);
            }
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
