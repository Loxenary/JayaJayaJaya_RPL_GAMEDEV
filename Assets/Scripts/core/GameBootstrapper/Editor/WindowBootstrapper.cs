using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor window that provides a button to play the game from the Bootstrap scene.
/// Automatically saves current scenes, loads Bootstrap, and enters play mode.
/// </summary>
public class WindowBootstrapper : EditorWindow
{
    private const string BOOTSTRAP_SCENE_PATH = "Assets/Scenes/Core/Bootstrap.unity";
    private const string MENU_PATH = "Tools/Play from Bootstrap";
    private const string WINDOW_TITLE = "Bootstrap Player";

    private static string[] _scenesToRestore;
    private static bool _isPlayingFromBootstrap = false;

    [MenuItem(MENU_PATH + " _F5")] // F5 hotkey
    public static void PlayFromBootstrap()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[WindowBootstrapper] Already in play mode.");
            return;
        }

        // Save current open scenes before switching
        SaveCurrentScenes();

        // Save current scene if it has changes
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            _isPlayingFromBootstrap = true;

            // Load Bootstrap scene
            EditorSceneManager.OpenScene(BOOTSTRAP_SCENE_PATH, OpenSceneMode.Single);

            // Enter play mode
            EditorApplication.isPlaying = true;
        }
    }

    [MenuItem("Tools/Bootstrap Player Window")]
    public static void ShowWindow()
    {
        GetWindow<WindowBootstrapper>(WINDOW_TITLE);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Bootstrap Scene Player", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Click the button below to automatically load the Bootstrap scene and enter play mode.\n\n" +
            "Your current scenes will be saved and restored when you exit play mode.\n\n" +
            "Shortcut: F5",
            MessageType.Info
        );

        GUILayout.Space(10);

        GUI.enabled = !EditorApplication.isPlaying;
        if (GUILayout.Button("Play from Bootstrap (F5)", GUILayout.Height(40)))
        {
            PlayFromBootstrap();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Game is running...", MessageType.None);
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Bootstrap Scene Path:", EditorStyles.miniLabel);
        EditorGUILayout.SelectableLabel(BOOTSTRAP_SCENE_PATH, EditorStyles.textField, GUILayout.Height(18));
    }

    private static void SaveCurrentScenes()
    {
        int sceneCount = SceneManager.sceneCount;
        _scenesToRestore = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            _scenesToRestore[i] = SceneManager.GetSceneAt(i).path;
        }
    }

    private static void RestorePreviousScenes()
    {
        if (_scenesToRestore == null || _scenesToRestore.Length == 0)
        {
            return;
        }

        // Load the first scene
        if (!string.IsNullOrEmpty(_scenesToRestore[0]))
        {
            EditorSceneManager.OpenScene(_scenesToRestore[0], OpenSceneMode.Single);
        }

        // Load additional scenes additively
        for (int i = 1; i < _scenesToRestore.Length; i++)
        {
            if (!string.IsNullOrEmpty(_scenesToRestore[i]))
            {
                EditorSceneManager.OpenScene(_scenesToRestore[i], OpenSceneMode.Additive);
            }
        }

        _scenesToRestore = null;
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!_isPlayingFromBootstrap)
        {
            return;
        }

        switch (state)
        {
            case PlayModeStateChange.ExitingPlayMode:
                // Restore previous scenes when exiting play mode
                EditorApplication.delayCall += () =>
                {
                    RestorePreviousScenes();
                    _isPlayingFromBootstrap = false;
                };
                break;
        }
    }
}