using UnityEngine;

/// <summary>
/// Makes a Canvas work properly when Time.timeScale = 0 (game paused)
/// Attach this to any Canvas that needs to work during pause (menus, game over screens, etc.)
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UnscaledTimeCanvas : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();

        // Ensure canvas works in pause
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    private void Update()
    {
        // This keeps the canvas updating even when Time.timeScale = 0
        // by using unscaledDeltaTime instead of deltaTime
        if (Time.timeScale == 0f && canvas != null)
        {
            canvas.enabled = true;
        }
    }
}
