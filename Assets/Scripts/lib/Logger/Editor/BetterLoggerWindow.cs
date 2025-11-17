using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace CustomLogger.Editor
{
    /// <summary>
    /// Enhanced editor window for managing logger settings
    /// Access via: Window > Better Logger
    /// </summary>
    public class BetterLoggerWindow : EditorWindow
    {
        private BetterLoggerSettings settings;
        private Vector2 scrollPosition;
        private bool showCategoryFilters = true;
        private bool showFormatSettings = true;
        private bool showQuickActions = true;

        private static readonly Dictionary<BetterLogger.LogCategory, string> CategoryDescriptions = new()
        {
            { BetterLogger.LogCategory.General, "General purpose logs" },
            { BetterLogger.LogCategory.Audio, "Audio and sound related logs" },
            { BetterLogger.LogCategory.Player, "Player controller and behavior logs" },
            { BetterLogger.LogCategory.UI, "UI and interface logs" },
            { BetterLogger.LogCategory.Network, "Network and multiplayer logs" },
            { BetterLogger.LogCategory.Physics, "Physics and collision logs" },
            { BetterLogger.LogCategory.AI, "AI and NPC behavior logs" },
            { BetterLogger.LogCategory.Input, "Input and control logs" },
            { BetterLogger.LogCategory.System, "System and framework logs" },
            { BetterLogger.LogCategory.Custom, "Custom category logs" }
        };

        [MenuItem("Window/Better Logger")]
        public static void ShowWindow()
        {
            var window = GetWindow<BetterLoggerWindow>("Better Logger");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            settings = Resources.Load<BetterLoggerSettings>("BetterLoggerSettings");

            if (settings == null)
            {
                // Try to find it anywhere in the project
                string[] guids = AssetDatabase.FindAssets("t:BetterLoggerSettings");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    settings = AssetDatabase.LoadAssetAtPath<BetterLoggerSettings>(path);
                }
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            if (settings == null)
            {
                DrawNoSettingsFound();
            }
            else
            {
                DrawQuickActions();
                EditorGUILayout.Space(10);

                DrawFormatSettings();
                EditorGUILayout.Space(10);

                DrawCategoryFilters();
                EditorGUILayout.Space(10);

                DrawTestSection();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("Better Logger Settings", titleStyle);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (settings != null)
            {
                if (GUILayout.Button("Select Settings Asset", GUILayout.Width(150)))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                LoadSettings();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawNoSettingsFound()
        {
            EditorGUILayout.HelpBox(
                "No BetterLoggerSettings asset found!\n\n" +
                "Create one via: Assets > Create > Logger > Logger Settings\n" +
                "For automatic loading, place it in a Resources folder.",
                MessageType.Warning
            );

            if (GUILayout.Button("Create Logger Settings", GUILayout.Height(30)))
            {
                CreateLoggerSettings();
            }
        }

        private void CreateLoggerSettings()
        {
            // Ensure Resources folder exists
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Create the settings asset
            settings = ScriptableObject.CreateInstance<BetterLoggerSettings>();
            string assetPath = $"{resourcesPath}/BetterLoggerSettings.asset";

            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);

            Debug.Log($"Created BetterLoggerSettings at {assetPath}");
        }

        private void DrawQuickActions()
        {
            showQuickActions = EditorGUILayout.BeginFoldoutHeaderGroup(showQuickActions, "Quick Actions");

            if (showQuickActions)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Enable All Categories", GUILayout.Height(25)))
                {
                    Undo.RecordObject(settings, "Enable All Categories");
                    settings.EnableAllCategories();
                    EditorUtility.SetDirty(settings);
                }

                if (GUILayout.Button("Disable All Categories", GUILayout.Height(25)))
                {
                    Undo.RecordObject(settings, "Disable All Categories");
                    settings.DisableAllCategories();
                    EditorUtility.SetDirty(settings);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawFormatSettings()
        {
            showFormatSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showFormatSettings, "Format Settings");

            if (showFormatSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                SerializedObject so = new SerializedObject(settings);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(so.FindProperty("isEnabled"), new GUIContent("Logging Enabled"));
                EditorGUILayout.PropertyField(so.FindProperty("showTimestamp"), new GUIContent("Show Timestamp"));
                EditorGUILayout.PropertyField(so.FindProperty("showCategory"), new GUIContent("Show Category"));
                EditorGUILayout.PropertyField(so.FindProperty("showLogType"), new GUIContent("Show Log Type"));
                EditorGUILayout.PropertyField(so.FindProperty("useColors"), new GUIContent("Use Colors"));

                if (EditorGUI.EndChangeCheck())
                {
                    so.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawCategoryFilters()
        {
            showCategoryFilters = EditorGUILayout.BeginFoldoutHeaderGroup(showCategoryFilters, "Category Filters");

            if (showCategoryFilters)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                SerializedObject so = new SerializedObject(settings);
                SerializedProperty categoryFilters = so.FindProperty("categoryFilters");

                for (int i = 0; i < categoryFilters.arraySize; i++)
                {
                    SerializedProperty filter = categoryFilters.GetArrayElementAtIndex(i);
                    SerializedProperty category = filter.FindPropertyRelative("category");
                    SerializedProperty enabled = filter.FindPropertyRelative("enabled");

                    BetterLogger.LogCategory categoryEnum = (BetterLogger.LogCategory)category.enumValueIndex;

                    EditorGUILayout.BeginHorizontal();

                    // Color indicator
                    Color categoryColor = GetCategoryColor(categoryEnum);
                    EditorGUI.DrawRect(GUILayoutUtility.GetRect(4, 20), categoryColor);
                    GUILayout.Space(5);

                    // Toggle
                    EditorGUI.BeginChangeCheck();
                    bool newEnabled = EditorGUILayout.Toggle(enabled.boolValue, GUILayout.Width(15));
                    if (EditorGUI.EndChangeCheck())
                    {
                        enabled.boolValue = newEnabled;
                    }

                    // Category name and description
                    EditorGUILayout.LabelField(categoryEnum.ToString(), GUILayout.Width(80));

                    if (CategoryDescriptions.TryGetValue(categoryEnum, out string description))
                    {
                        GUIStyle descStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            normal = { textColor = Color.gray }
                        };
                        EditorGUILayout.LabelField(description, descStyle);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                so.ApplyModifiedProperties();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTestSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Test Logging", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Click a button to test logging with different categories:", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Player"))
            {
                BetterLogger.Log("This is a test player log!", BetterLogger.LogCategory.Player);
            }

            if (GUILayout.Button("Test Audio"))
            {
                BetterLogger.Log("This is a test audio log!", BetterLogger.LogCategory.Audio);
            }

            if (GUILayout.Button("Test UI"))
            {
                BetterLogger.Log("This is a test UI log!", BetterLogger.LogCategory.UI);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Warning"))
            {
                BetterLogger.LogWarning("This is a test warning!", BetterLogger.LogCategory.System);
            }

            if (GUILayout.Button("Test Error"))
            {
                BetterLogger.LogError("This is a test error!", BetterLogger.LogCategory.System);
            }

            if (GUILayout.Button("Test Custom Color"))
            {
                BetterLogger.LogColored("This is a custom colored log!", "#FF1493");
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
