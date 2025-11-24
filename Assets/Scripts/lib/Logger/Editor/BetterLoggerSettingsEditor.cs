using UnityEngine;
using UnityEditor;

namespace CustomLogger.Editor
{
    /// <summary>
    /// Custom inspector for BetterLoggerSettings with enhanced UI
    /// </summary>
    [CustomEditor(typeof(BetterLoggerSettings))]
    public class BetterLoggerSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty isEnabled;
        private SerializedProperty showTimestamp;
        private SerializedProperty showCategory;
        private SerializedProperty showLogType;
        private SerializedProperty useColors;
        private SerializedProperty categoryFilters;

        private void OnEnable()
        {
            isEnabled = serializedObject.FindProperty("isEnabled");
            showTimestamp = serializedObject.FindProperty("showTimestamp");
            showCategory = serializedObject.FindProperty("showCategory");
            showLogType = serializedObject.FindProperty("showLogType");
            useColors = serializedObject.FindProperty("useColors");
            categoryFilters = serializedObject.FindProperty("categoryFilters");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BetterLoggerSettings settings = (BetterLoggerSettings)target;

            // Header
            DrawHeader();
            EditorGUILayout.Space(5);

            // Main Enable/Disable
            DrawMainToggle();
            EditorGUILayout.Space(10);

            // Format Settings
            DrawFormatSettings();
            EditorGUILayout.Space(10);

            // Category Filters with visual improvements
            DrawCategoryFilters(settings);
            EditorGUILayout.Space(10);

            // Quick Actions
            DrawQuickActions(settings);
            EditorGUILayout.Space(10);

            // Test Section
            DrawTestSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("Better Logger Settings", titleStyle);

            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray }
            };

            EditorGUILayout.LabelField("Enhanced logging system with categories and colors", subtitleStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawMainToggle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            GUIStyle toggleStyle = new GUIStyle(EditorStyles.toggle)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            isEnabled.boolValue = EditorGUILayout.Toggle(
                new GUIContent("Enable Logging", "Master switch for all logging"),
                isEnabled.boolValue,
                toggleStyle
            );

            // Status indicator
            Color statusColor = isEnabled.boolValue ? Color.green : Color.red;
            string statusText = isEnabled.boolValue ? "ACTIVE" : "DISABLED";

            GUIStyle statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = statusColor },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };

            EditorGUILayout.LabelField(statusText, statusStyle, GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();

            if (!isEnabled.boolValue)
            {
                EditorGUILayout.HelpBox("All logging is currently disabled!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawFormatSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Format Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(showTimestamp, new GUIContent("Show Timestamp", "Display timestamp in logs"));
            EditorGUILayout.PropertyField(showCategory, new GUIContent("Show Category", "Display category in logs"));
            EditorGUILayout.PropertyField(showLogType, new GUIContent("Show Log Type", "Display log type (LOG/WARNING/ERROR)"));
            EditorGUILayout.PropertyField(useColors, new GUIContent("Use Colors", "Colorize categories in console"));

            // Preview
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel);

            string preview = GeneratePreview();
            EditorGUILayout.HelpBox(preview, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        private string GeneratePreview()
        {
            string preview = "";

            if (showTimestamp.boolValue)
                preview += "[12:34:56.789] ";

            if (showCategory.boolValue)
                preview += "[Player] ";

            if (showLogType.boolValue)
                preview += "[LOG] ";

            preview += "Your log message here";

            return preview;
        }

        private void DrawCategoryFilters(BetterLoggerSettings settings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Category Filters", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Enable or disable specific log categories", MessageType.Info);

            for (int i = 0; i < categoryFilters.arraySize; i++)
            {
                SerializedProperty filter = categoryFilters.GetArrayElementAtIndex(i);
                SerializedProperty category = filter.FindPropertyRelative("category");
                SerializedProperty enabled = filter.FindPropertyRelative("enabled");

                BetterLogger.LogCategory categoryEnum = (BetterLogger.LogCategory)category.enumValueIndex;

                EditorGUILayout.BeginHorizontal();

                // Color indicator
                Color categoryColor = GetCategoryColor(categoryEnum);
                Rect colorRect = GUILayoutUtility.GetRect(8, EditorGUIUtility.singleLineHeight);
                EditorGUI.DrawRect(colorRect, categoryColor);

                GUILayout.Space(5);

                // Toggle with category name
                enabled.boolValue = EditorGUILayout.Toggle(
                    new GUIContent(categoryEnum.ToString()),
                    enabled.boolValue,
                    GUILayout.Width(200)
                );

                // Status
                string status = enabled.boolValue ? "✓ Enabled" : "✗ Disabled";
                Color statusColor = enabled.boolValue ? new Color(0, 0.8f, 0) : new Color(0.8f, 0, 0);

                GUIStyle statusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = statusColor }
                };

                EditorGUILayout.LabelField(status, statusStyle);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions(BetterLoggerSettings settings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft))
            {
                Undo.RecordObject(settings, "Enable All Categories");
                settings.EnableAllCategories();
                EditorUtility.SetDirty(settings);
            }

            if (GUILayout.Button("Disable All", EditorStyles.miniButtonMid))
            {
                Undo.RecordObject(settings, "Disable All Categories");
                settings.DisableAllCategories();
                EditorUtility.SetDirty(settings);
            }

            if (GUILayout.Button("Open Logger Window", EditorStyles.miniButtonRight))
            {
                BetterLoggerWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawTestSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Test Logging", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Test different log types and categories", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Log"))
            {
                BetterLogger.Log("This is a test log message!", BetterLogger.LogCategory.General);
            }

            if (GUILayout.Button("Test Warning"))
            {
                BetterLogger.LogWarning("This is a test warning!", BetterLogger.LogCategory.System);
            }

            if (GUILayout.Button("Test Error"))
            {
                BetterLogger.LogError("This is a test error!", BetterLogger.LogCategory.System);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Player Log", EditorStyles.miniButtonLeft))
            {
                BetterLogger.Log("Player moved to position (10, 0, 5)", BetterLogger.LogCategory.Player);
            }

            if (GUILayout.Button("Audio Log", EditorStyles.miniButtonMid))
            {
                BetterLogger.Log("Playing sound: footstep.wav", BetterLogger.LogCategory.Audio);
            }

            if (GUILayout.Button("UI Log", EditorStyles.miniButtonRight))
            {
                BetterLogger.Log("Button clicked: StartGame", BetterLogger.LogCategory.UI);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private Color GetCategoryColor(BetterLogger.LogCategory category)
        {
            string[] colors = new[]
            {
                "#FFFFFF", "#FF69B4", "#00FF00", "#00BFFF", "#FFA500",
                "#FF00FF", "#FFFF00", "#9370DB", "#FF6347", "#00FFFF"
            };

            if (ColorUtility.TryParseHtmlString(colors[(int)category], out Color color))
            {
                return color;
            }

            return Color.white;
        }
    }
}
