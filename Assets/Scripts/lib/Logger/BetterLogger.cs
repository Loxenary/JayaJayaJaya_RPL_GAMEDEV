using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomLogger
{
    /// <summary>
    /// Enhanced logging system with categories, colors, timestamps, and conditional compilation
    /// </summary>
    public static class BetterLogger
    {
        public enum LogCategory
        {
            General,
            Audio,
            Player,
            UI,
            Network,
            Physics,
            AI,
            Input,
            System,
            Custom
        }

        private static readonly string[] CategoryColors = new[]
        {
            "#FFFFFF", // General - White
            "#FF69B4", // Audio - Pink
            "#00FF00", // Player - Green
            "#00BFFF", // UI - Sky Blue
            "#FFA500", // Network - Orange
            "#FF00FF", // Physics - Magenta
            "#FFFF00", // AI - Yellow
            "#9370DB", // Input - Medium Purple
            "#FF6347", // System - Tomato Red
            "#00FFFF"  // Custom - Cyan
        };

        private static BetterLoggerSettings _settings;

        private static BetterLoggerSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = Resources.Load<BetterLoggerSettings>("BetterLoggerSettings");
                    if (_settings == null)
                    {
                        // Use default settings if no ScriptableObject found
                        _settings = ScriptableObject.CreateInstance<BetterLoggerSettings>();
                    }
                }
                return _settings;
            }
        }

        #region Public API

        /// <summary>
        /// Log a regular message with optional category
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedMessage = FormatMessage(message.ToString(), category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log a warning message with optional category
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedMessage = FormatMessage(message.ToString(), category, "WARNING");
            Debug.LogWarning(formattedMessage, context);
        }

        /// <summary>
        /// Log an error message with optional category
        /// </summary>
        public static void LogError(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled) return;

            string formattedMessage = FormatMessage(message.ToString(), category, "ERROR");
            Debug.LogError(formattedMessage, context);
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        public static void LogException(Exception exception, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled) return;
            Debug.LogException(exception, context);
        }

        /// <summary>
        /// Log with custom color
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogColored(object message, string hexColor, LogCategory category = LogCategory.Custom, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedMessage = FormatMessage(message.ToString(), category, "LOG", hexColor);
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log only in the Unity Editor
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogEditor(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedMessage = FormatMessage(message.ToString(), category, "EDITOR");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log with a custom prefix
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWithPrefix(string prefix, object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedMessage = FormatMessage($"{prefix}: {message}", category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Assert a condition and log an error if it fails
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Assert(bool condition, object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!condition)
            {
                LogError($"ASSERTION FAILED: {message}", category, context);
            }
        }

        #endregion

        #region Formatting

        private static string FormatMessage(string message, LogCategory category, string logType, string customColor = null)
        {
            string timestamp = Settings.ShowTimestamp ? $"[{DateTime.Now:HH:mm:ss.fff}] " : "";
            string categoryStr = Settings.ShowCategory ? $"[{category}] " : "";
            string typeStr = Settings.ShowLogType ? $"[{logType}] " : "";

            string color = customColor ?? CategoryColors[(int)category];

            if (Settings.UseColors)
            {
                categoryStr = ColorizeText(categoryStr, color);
            }

            return $"{timestamp}{categoryStr}{typeStr}{message}";
        }

        private static string ColorizeText(string text, string hexColor)
        {
            return $"<color={hexColor}>{text}</color>";
        }

        #endregion

        #region Stack Trace Helpers

        /// <summary>
        /// Get the calling method name (useful for debugging)
        /// </summary>
        public static string GetCallerMethodName()
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(2);
            return frame?.GetMethod()?.Name ?? "Unknown";
        }

        /// <summary>
        /// Log with automatic caller method name
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogMethod(object message, LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            string methodName = GetCallerMethodName();
            LogWithPrefix($"{methodName}()", message, category, context);
        }

        #endregion
    }

    #region Extension Methods

    /// <summary>
    /// Extension methods for easier logging
    /// </summary>
    public static class LoggerExtensions
    {
        public static void Log(this MonoBehaviour mb, object message, BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.Log($"[{mb.GetType().Name}] {message}", category, mb);
        }

        public static void LogWarning(this MonoBehaviour mb, object message, BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogWarning($"[{mb.GetType().Name}] {message}", category, mb);
        }

        public static void LogError(this MonoBehaviour mb, object message, BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogError($"[{mb.GetType().Name}] {message}", category, mb);
        }
    }

    #endregion
}
