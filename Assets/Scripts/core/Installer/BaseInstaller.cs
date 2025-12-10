using CustomLogger;
using UnityEngine;

/// <summary>
/// Abstract base class for scene-specific installers.
/// Extend this class to create installers for specific scenes (MenuInstaller, InGameInstaller, etc.).
/// Each installer is responsible for initializing scene-specific dependencies and objects.
/// </summary>
public abstract class BaseInstaller : MonoBehaviour
{
    [Header("Installer Settings")]
    [SerializeField] protected bool showDebugLogs = true;

    /// <summary>
    /// Called before scene initialization starts.
    /// Override this to perform any pre-initialization setup.
    /// </summary>
    protected virtual void Awake()
    {
        Log($"{GetType().Name} Awake - preparing to install...");
    }

    /// <summary>
    /// Called when the installer is enabled.
    /// Triggers the installation process.
    /// </summary>
    protected virtual void Start()
    {
        Log($"{GetType().Name} Start - beginning installation...");
        Install();
        Log($"{GetType().Name} installation complete");
    }

    /// <summary>
    /// Main installation method. Override this in derived classes to install scene-specific dependencies.
    /// This is where you should instantiate prefabs, configure scene objects, and set up scene-specific systems.
    /// </summary>
    protected abstract void Install();

    /// <summary>
    /// Optional cleanup method called when the installer is destroyed.
    /// Override this to perform cleanup if needed.
    /// </summary>
    protected virtual void OnDestroy()
    {
        Log($"{GetType().Name} OnDestroy - cleaning up...");
    }

    /// <summary>
    /// Helper method to log messages with installer name prefix.
    /// </summary>
    protected void Log(string message)
    {
        if (showDebugLogs)
        {
            BetterLogger.Log($"[{GetType().Name}] {message}");
        }
    }

    /// <summary>
    /// Helper method to log warnings with installer name prefix.
    /// </summary>
    protected void LogWarning(string message)
    {
        Debug.LogWarning($"[{GetType().Name}] {message}");
    }

    /// <summary>
    /// Helper method to log errors with installer name prefix.
    /// </summary>
    protected void LogError(string message)
    {
        Debug.LogError($"[{GetType().Name}] {message}");
    }
}
