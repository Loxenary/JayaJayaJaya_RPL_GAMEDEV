using System;
using System.Collections.Generic;

/// <summary>
/// Static utility class providing convenient methods for UI management via EventBus.
/// Simplifies common UI operations with a clean, fluent API.
/// </summary>
public static class UIManager
{
    // Track currently open UIs (optional, for debugging)
    private static readonly HashSet<Type> _openUIs = new HashSet<Type>();

    #region Open Methods

    /// <summary>
    /// Open a UI panel of type T
    /// </summary>
    public static void Open<T>() where T : ShowHideBase
    {
        EventBus.Publish(new OpenUI<T>());
        _openUIs.Add(typeof(T));
    }

    /// <summary>
    /// Open a UI panel with a specific instance reference
    /// </summary>
    public static void Open<T>(T instance) where T : ShowHideBase
    {
        EventBus.Publish(new OpenUI<T>(instance));
        _openUIs.Add(typeof(T));
    }

    #endregion

    #region Close Methods

    /// <summary>
    /// Close a UI panel of type T
    /// </summary>
    public static void Close<T>() where T : ShowHideBase
    {
        EventBus.Publish(new CloseUI<T>());
        _openUIs.Remove(typeof(T));
    }

    /// <summary>
    /// Close a UI panel with a specific instance reference
    /// </summary>
    public static void Close<T>(T instance) where T : ShowHideBase
    {
        EventBus.Publish(new CloseUI<T>(instance));
        _openUIs.Remove(typeof(T));
    }

    /// <summary>
    /// Close all tracked UI panels
    /// Note: This only closes UIs that were opened through UIManager
    /// </summary>
    public static void CloseAll()
    {
        foreach (var uiType in new List<Type>(_openUIs))
        {
            // Use reflection to call Close for each type
            var method = typeof(UIManager).GetMethod(nameof(Close));
            var genericMethod = method.MakeGenericMethod(uiType);
            genericMethod.Invoke(null, null);
        }
        _openUIs.Clear();
    }

    #endregion

    #region Toggle Methods

    /// <summary>
    /// Toggle a UI panel between open and closed states
    /// </summary>
    public static void Toggle<T>() where T : ShowHideBase
    {
        EventBus.Publish(new ToggleUI<T>());

        // Update tracking
        if (_openUIs.Contains(typeof(T)))
            _openUIs.Remove(typeof(T));
        else
            _openUIs.Add(typeof(T));
    }

    /// <summary>
    /// Toggle a UI panel with a specific instance reference
    /// </summary>
    public static void Toggle<T>(T instance) where T : ShowHideBase
    {
        EventBus.Publish(new ToggleUI<T>(instance));

        // Update tracking
        if (_openUIs.Contains(typeof(T)))
            _openUIs.Remove(typeof(T));
        else
            _openUIs.Add(typeof(T));
    }

    #endregion

    #region Fluent API

    /// <summary>
    /// Start a fluent UI operation chain
    /// </summary>
    public static UIOperationChain Chain()
    {
        return new UIOperationChain();
    }

    /// <summary>
    /// Fluent API for chaining multiple UI operations
    /// </summary>
    public class UIOperationChain
    {
        private readonly List<Action> _operations = new List<Action>();

        public UIOperationChain Open<T>() where T : ShowHideBase
        {
            _operations.Add(() => UIManager.Open<T>());
            return this;
        }

        public UIOperationChain Close<T>() where T : ShowHideBase
        {
            _operations.Add(() => UIManager.Close<T>());
            return this;
        }

        public UIOperationChain Toggle<T>() where T : ShowHideBase
        {
            _operations.Add(() => UIManager.Toggle<T>());
            return this;
        }

        /// <summary>
        /// Execute all chained operations
        /// </summary>
        public void Execute()
        {
            foreach (var operation in _operations)
            {
                operation.Invoke();
            }
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if a UI type is currently tracked as open
    /// Note: Only tracks UIs opened through UIManager
    /// </summary>
    public static bool IsOpen<T>() where T : ShowHideBase
    {
        return _openUIs.Contains(typeof(T));
    }

    /// <summary>
    /// Get count of currently tracked open UIs
    /// </summary>
    public static int GetOpenUICount()
    {
        return _openUIs.Count;
    }

    /// <summary>
    /// Clear the internal tracking (useful for scene transitions)
    /// </summary>
    public static void ClearTracking()
    {
        _openUIs.Clear();
    }

    #endregion
}
