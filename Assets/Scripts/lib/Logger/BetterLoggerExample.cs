using UnityEngine;
using CustomLogger;

/// <summary>
/// Example usage of the BetterLogger system
/// Attach this to a GameObject to see various logging examples
/// </summary>
public class BetterLoggerExample : MonoBehaviour
{
    private void Start()
    {
        DemonstrateBasicLogging();
        DemonstrateCategories();
        DemonstrateExtensionMethods();
        DemonstrateSpecialFeatures();
    }

    private void DemonstrateBasicLogging()
    {
        // Basic logs
        BetterLogger.Log("This is a basic log message");
        BetterLogger.LogWarning("This is a warning message");
        BetterLogger.LogError("This is an error message");
    }

    private void DemonstrateCategories()
    {
        // Logs with different categories
        BetterLogger.Log("Player spawned at position (0, 0, 0)", BetterLogger.LogCategory.Player);
        BetterLogger.Log("Playing background music", BetterLogger.LogCategory.Audio);
        BetterLogger.Log("Main menu loaded", BetterLogger.LogCategory.UI);
        BetterLogger.Log("Connected to server", BetterLogger.LogCategory.Network);
        BetterLogger.Log("Collision detected", BetterLogger.LogCategory.Physics);
        BetterLogger.Log("Enemy AI state changed to Patrol", BetterLogger.LogCategory.AI);
        BetterLogger.Log("Jump button pressed", BetterLogger.LogCategory.Input);
        BetterLogger.Log("Game initialized", BetterLogger.LogCategory.System);
    }

    private void DemonstrateExtensionMethods()
    {
        // Using extension methods - automatically includes class name and context
        this.Log("This log uses the MonoBehaviour extension method");
        this.LogWarning("This warning includes the MonoBehaviour context", BetterLogger.LogCategory.Player);
        this.LogError("This error shows which GameObject logged it");
    }

    private void DemonstrateSpecialFeatures()
    {
        // Custom colored log
        BetterLogger.LogColored("This log has a custom pink color!", "#FF1493");

        // Log with custom prefix
        BetterLogger.LogWithPrefix("CUSTOM", "This has a custom prefix");

        // Editor-only log (won't appear in builds)
        BetterLogger.LogEditor("This only appears in the Unity Editor");

        // Log with automatic method name
        BetterLogger.LogMethod("This automatically includes the method name");

        // Assert - only logs if condition is false
        bool isPlayerAlive = true;
        BetterLogger.Assert(isPlayerAlive, "Player should be alive at this point");

        // With GameObject context (makes it clickable in console)
        BetterLogger.Log("Click this log to highlight the GameObject!", BetterLogger.LogCategory.General, gameObject);
    }

    private void Update()
    {
        // Example: Log only when a key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BetterLogger.Log("Space key pressed!", BetterLogger.LogCategory.Input, this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Example: Physics logging
        BetterLogger.Log(
            $"Collision with {collision.gameObject.name} at velocity {collision.relativeVelocity.magnitude:F2}",
            BetterLogger.LogCategory.Physics,
            gameObject
        );
    }

    // Example of logging in a coroutine
    private System.Collections.IEnumerator ExampleCoroutine()
    {
        BetterLogger.Log("Coroutine started", BetterLogger.LogCategory.System);

        yield return new WaitForSeconds(1f);

        BetterLogger.Log("Coroutine completed after 1 second", BetterLogger.LogCategory.System);
    }

    // Example of conditional logging
    [SerializeField] private bool debugMode = true;

    private void ConditionalLog(string message)
    {
        if (debugMode)
        {
            BetterLogger.Log(message, BetterLogger.LogCategory.General, this);
        }
    }
}

/* ====================================
   USAGE GUIDE
   ====================================

1. SETUP:
   - Create a BetterLoggerSettings asset: Assets > Create > Logger > Logger Settings
   - Place it in a Resources folder (e.g., Assets/Resources/)
   - Configure settings via Window > Better Logger

2. BASIC USAGE:
   using CustomLogger;

   BetterLogger.Log("Your message");
   BetterLogger.LogWarning("Warning message");
   BetterLogger.LogError("Error message");

3. WITH CATEGORIES:
   BetterLogger.Log("Player jumped", BetterLogger.LogCategory.Player);
   BetterLogger.Log("Sound played", BetterLogger.LogCategory.Audio);

4. EXTENSION METHODS (MonoBehaviour):
   this.Log("Easy logging from MonoBehaviour");
   this.LogWarning("With automatic context");

5. SPECIAL FEATURES:
   - Custom colors: BetterLogger.LogColored("Message", "#FF0000");
   - Custom prefix: BetterLogger.LogWithPrefix("PREFIX", "Message");
   - Editor only: BetterLogger.LogEditor("Only in editor");
   - Method name: BetterLogger.LogMethod("Auto method name");
   - Assert: BetterLogger.Assert(condition, "Error message");

6. FILTERING:
   - Use Window > Better Logger to enable/disable categories
   - Disable all logging via settings.IsEnabled = false
   - Filter categories in real-time

7. CONDITIONAL COMPILATION:
   - Most logs use [Conditional] attributes
   - Automatically removed from release builds
   - Errors always remain for debugging

8. COLORS BY CATEGORY:
   General  - White
   Audio    - Pink
   Player   - Green
   UI       - Sky Blue
   Network  - Orange
   Physics  - Magenta
   AI       - Yellow
   Input    - Purple
   System   - Tomato Red
   Custom   - Cyan

====================================*/
