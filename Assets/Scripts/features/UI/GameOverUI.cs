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

        Debug.Log("[GameOverUI] Game Over screen shown");
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
        Debug.Log("[GameOverUI] Restart button clicked");

        // Hide panel first
        HideGameOver();

        // Reset time scale before restart
        Time.timeScale = 1f;

        // Disable this object to prevent any further interaction
        if (restartButton != null)
            restartButton.interactable = false;

        // Get SceneService to reload current scene
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            Debug.Log("[GameOverUI] Calling SceneService.ReloadScene...");
            await sceneService.ReloadScene(addTransition: true);
            Debug.Log("[GameOverUI] Scene reload completed");
        }
        else
        {
            // Fallback: Reload using Unity SceneManager
            Debug.LogWarning("[GameOverUI] SceneService not found, using fallback reload");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }

    /// <summary>
    /// Quit button callback (load main menu or quit game)
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("[GameOverUI] Quit button clicked");

        // Reset time scale
        Time.timeScale = 1f;

        // Try to load main menu scene
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            // Assuming you have a MainMenu scene enum
            // sceneService.LoadScene(SceneEnum.MainMenu, false);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
