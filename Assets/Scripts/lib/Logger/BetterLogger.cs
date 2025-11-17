using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        #region Collection Logging

        /// <summary>
        /// Log a collection (List, Queue, Array, etc.) with formatted output
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogCollection<T>(IEnumerable<T> collection, string collectionName = "Collection", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedCollection = FormatCollection(collection, collectionName);
            string formattedMessage = FormatMessage(formattedCollection, category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log a collection as a warning
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogCollectionWarning<T>(IEnumerable<T> collection, string collectionName = "Collection", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedCollection = FormatCollection(collection, collectionName);
            string formattedMessage = FormatMessage(formattedCollection, category, "WARNING");
            Debug.LogWarning(formattedMessage, context);
        }

        /// <summary>
        /// Log a collection as an error
        /// </summary>
        public static void LogCollectionError<T>(IEnumerable<T> collection, string collectionName = "Collection", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled) return;

            string formattedCollection = FormatCollection(collection, collectionName);
            string formattedMessage = FormatMessage(formattedCollection, category, "ERROR");
            Debug.LogError(formattedMessage, context);
        }

        /// <summary>
        /// Log a list with automatic type detection
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogList<T>(List<T> list, string listName = "List", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedList = FormatCollection(list, listName, "List");
            string formattedMessage = FormatMessage(formattedList, category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log a queue with automatic type detection
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogQueue<T>(Queue<T> queue, string queueName = "Queue", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedQueue = FormatCollection(queue, queueName, "Queue");
            string formattedMessage = FormatMessage(formattedQueue, category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log a stack with automatic type detection
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogStack<T>(Stack<T> stack, string stackName = "Stack", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedStack = FormatCollection(stack, stackName, "Stack");
            string formattedMessage = FormatMessage(formattedStack, category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log an array with automatic type detection
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogArray<T>(T[] array, string arrayName = "Array", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedArray = FormatCollection(array, arrayName, "Array");
            string formattedMessage = FormatMessage(formattedArray, category, "LOG");
            Debug.Log(formattedMessage, context);
        }

        /// <summary>
        /// Log a dictionary with key-value pairs
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string dictionaryName = "Dictionary", LogCategory category = LogCategory.General, UnityEngine.Object context = null)
        {
            if (!Settings.IsEnabled || !Settings.IsCategoryEnabled(category)) return;

            string formattedDictionary = FormatDictionary(dictionary, dictionaryName);
            string formattedMessage = FormatMessage(formattedDictionary, category, "LOG");
            Debug.Log(formattedMessage, context);
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

        /// <summary>
        /// Format a collection into a readable string
        /// </summary>
        private static string FormatCollection<T>(IEnumerable<T> collection, string collectionName, string collectionType = null)
        {
            if (collection == null)
            {
                return $"{collectionName}: null";
            }

            var items = collection.ToList();
            int count = items.Count;

            string typeInfo = collectionType != null ? $"<{collectionType}>" : "";
            var sb = new StringBuilder();
            sb.AppendLine($"{collectionName}{typeInfo} (Count: {count})");

            if (count == 0)
            {
                sb.Append("  [Empty]");
                return sb.ToString();
            }

            for (int i = 0; i < items.Count; i++)
            {
                string prefix = i == items.Count - 1 ? "  └─" : "  ├─";
                sb.AppendLine($"{prefix} [{i}]: {FormatItem(items[i])}");
            }

            return sb.ToString().TrimEnd('\n', '\r');
        }

        /// <summary>
        /// Format a dictionary into a readable string
        /// </summary>
        private static string FormatDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string dictionaryName)
        {
            if (dictionary == null)
            {
                return $"{dictionaryName}: null";
            }

            int count = dictionary.Count;
            var sb = new StringBuilder();
            sb.AppendLine($"{dictionaryName} (Count: {count})");

            if (count == 0)
            {
                sb.Append("  [Empty]");
                return sb.ToString();
            }

            int index = 0;
            foreach (var kvp in dictionary)
            {
                string prefix = index == count - 1 ? "  └─" : "  ├─";
                sb.AppendLine($"{prefix} {FormatItem(kvp.Key)} => {FormatItem(kvp.Value)}");
                index++;
            }

            return sb.ToString().TrimEnd('\n', '\r');
        }

        /// <summary>
        /// Format an individual item for display
        /// </summary>
        private static string FormatItem(object item)
        {
            if (item == null) return "null";

            // Handle Unity objects specially
            if (item is UnityEngine.Object unityObj)
            {
                return unityObj != null ? $"{unityObj.GetType().Name}({unityObj.name})" : "null";
            }

            // Handle strings with quotes
            if (item is string str)
            {
                return $"\"{str}\"";
            }

            return item.ToString();
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

        // Collection logging extension methods
        public static void LogCollection<T>(this MonoBehaviour mb, IEnumerable<T> collection, string collectionName = "Collection", BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogCollection(collection, $"[{mb.GetType().Name}] {collectionName}", category, mb);
        }

        public static void LogList<T>(this MonoBehaviour mb, List<T> list, string listName = "List", BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogList(list, $"[{mb.GetType().Name}] {listName}", category, mb);
        }

        public static void LogQueue<T>(this MonoBehaviour mb, Queue<T> queue, string queueName = "Queue", BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogQueue(queue, $"[{mb.GetType().Name}] {queueName}", category, mb);
        }

        public static void LogStack<T>(this MonoBehaviour mb, Stack<T> stack, string stackName = "Stack", BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogStack(stack, $"[{mb.GetType().Name}] {stackName}", category, mb);
        }

        public static void LogArray<T>(this MonoBehaviour mb, T[] array, string arrayName = "Array", BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogArray(array, $"[{mb.GetType().Name}] {arrayName}", category, mb);
        }

        public static void LogDictionary<TKey, TValue>(this MonoBehaviour mb, Dictionary<TKey, TValue> dictionary, string dictionaryName = "Dictionary", BetterLogger.LogCategory category = BetterLogger.LogCategory.General)
        {
            BetterLogger.LogDictionary(dictionary, $"[{mb.GetType().Name}] {dictionaryName}", category, mb);
        }
    }

    #endregion
}
