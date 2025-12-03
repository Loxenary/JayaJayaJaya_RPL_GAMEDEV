// File: SceneService.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomLogger;


public class SceneService : MonoBehaviour, IInitializableService
{
    public event Action OnInitialized;

    // The list now holds records pointing to SceneGroup assets.
    public List<SceneRecord> SceneRecords;

    public event Action<SceneEnum> OnSceneChanging;

    private readonly List<string> _persistentSceneNames = new List<string>();

    private SceneGroup _currentSceneGroup;
    public SceneEnum CurrentScene { get; private set; }

    [SerializeField] private SceneTransitionSettings transitionSettings;

    public bool IsInitialized { get; private set; } = false;
    ServicePriority IInitializableService.InitializationPriority => ServicePriority.PRIMARY;

    private readonly List<Scene> _loadedScenes = new List<Scene>();
    private SceneTransitionCoordinator _transitionCoordinator;
    private Scene _bootstrapScene;

    // Testing mode support
    private bool _isTestMode = false;
    private string _testScenePath;
    private string _testSceneName;


    private void Awake()
    {
        ServiceLocator.Register(this);

        _bootstrapScene = FindBootstrapScene();
    }

    private Scene FindBootstrapScene()
    {
        var gameBootstrap = FindFirstObjectByType<GameBootstrap>();
        if (gameBootstrap != null)
        {
            return gameBootstrap.gameObject.scene;
        }
        return default;
    }

    public Task Initialize()
    {
        // Initialize the transition system
        ISceneTransitionHandler transitionHandler = CreateTransitionHandler();
        _transitionCoordinator = new SceneTransitionCoordinator(transitionHandler);

        IsInitialized = true;
        OnInitialized?.Invoke();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets test mode for the SceneService (Editor only).
    /// Enables reload support for test scenes.
    /// </summary>
    public void SetTestMode(string scenePath, string sceneName)
    {
        _isTestMode = true;
        _testScenePath = scenePath;
        _testSceneName = sceneName;
    }

    /// <summary>
    /// Returns true if currently in test mode.
    /// </summary>
    public bool IsTestMode => _isTestMode;

    /// <summary>
    /// Creates the appropriate transition handler based on settings.
    /// </summary>
    private ISceneTransitionHandler CreateTransitionHandler()
    {
        if (transitionSettings == null)
        {
            Debug.LogWarning("No SceneTransitionSettings assigned. Using default EventBus handler.");
            return new EventBusTransitionHandler();
        }

        switch (transitionSettings.handlerType)
        {
            case TransitionHandlerType.EventBus:
                return new EventBusTransitionHandler(transitionSettings.animationSpeed);
            case TransitionHandlerType.None:
                return new NullTransitionHandler();
            default:
                Debug.LogWarning($"Unknown transition handler type: {transitionSettings.handlerType}. Using EventBus.");
                return new EventBusTransitionHandler();
        }
    }

    public void RegisterPersistentScenes(IEnumerable<string> sceneNames)
    {
        _persistentSceneNames.Clear();
        _persistentSceneNames.AddRange(sceneNames);
    }

    /// <summary>
    /// Loads a scene group and unloads any scenes not in that group.
    /// </summary>
    public async Task LoadScene(SceneEnum sceneId, bool addTransition = true)
    {
        await _transitionCoordinator.ExecuteWithTransition(async () =>
        {
            // 1. Find the scene group to load
            SceneRecord record = SceneRecords.Find(r => r.SceneId == sceneId);
            if (record == null || record.SceneGroupAsset == null)
            {
                Debug.LogError($"No SceneGroup found for SceneEnum: {sceneId}");
                return;
            }
            SceneGroup groupToLoad = record.SceneGroupAsset;

            // Load new scenes first, then unload old ones
            await LoadSceneGroup(groupToLoad, forceReload: false);
            await UnloadUnusedScenes(groupToLoad);

            _currentSceneGroup = groupToLoad;
            CurrentScene = sceneId;

            OnSceneChanging?.Invoke(sceneId);
        }, addTransition);
    }


    /// <summary>
    /// Loads all scenes within a SceneGroup additively.
    /// </summary>
    private async Task LoadSceneGroup(SceneGroup group, bool forceReload = false)
    {
        _loadedScenes.Clear();

        // Load the main scene first
        if (group.ActiveScene != null)
        {
            bool shouldLoad = forceReload || !IsSceneLoaded(group.ActiveScene.SceneName);
            if (shouldLoad)
            {
                await LoadSingleScene(group.ActiveScene.SceneName, true);
            }
            else
            {
                // Scene already loaded, just set it as active
                var existingScene = SceneManager.GetSceneByName(group.ActiveScene.SceneName);
                if (existingScene.IsValid())
                {
                    SceneManager.SetActiveScene(existingScene);
                }
            }
        }

        // Load all additional scenes
        if (group.AdditionalScenes != null)
        {
            foreach (var sceneRef in group.AdditionalScenes)
            {
                bool shouldLoad = forceReload || !IsSceneLoaded(sceneRef.SceneName);
                if (sceneRef != null && shouldLoad)
                {
                    await LoadSingleScene(sceneRef.SceneName, false);
                }
            }
        }
    }

    /// <summary>
    /// Unloads any scenes that are currently loaded but are not part of the target group.
    /// </summary>
    private async Task UnloadUnusedScenes(SceneGroup targetGroup)
    {
        List<string> scenesToKeep = new List<string>();
        // If targetGroup is null (e.g., during a reload), scenesToKeep remains empty, so everything is unloaded.
        if (targetGroup != null)
        {
            if (targetGroup.ActiveScene != null)
            {
                scenesToKeep.Add(targetGroup.ActiveScene.SceneName);
            }
            if (targetGroup.AdditionalScenes != null)
            {
                scenesToKeep.AddRange(targetGroup.AdditionalScenes.Where(s => s != null).Select(s => s.SceneName));
            }
        }

        List<Task> unloadTasks = new List<Task>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Don't unload the bootstrap scene (where this service lives) or scenes we want to keep
            if (scene == _bootstrapScene || scenesToKeep.Contains(scene.name) || _persistentSceneNames.Contains(scene.name))
            {
                continue;
            }

            if (scene.isLoaded)
            {
                BetterLogger.Log($"Scene To Unload:  {scene.name}", BetterLogger.LogCategory.System);
                unloadTasks.Add(SceneManager.UnloadSceneAsync(scene).AsTask());
            }
        }
        if (unloadTasks.Any())
        {
            await Task.WhenAll(unloadTasks);
        }
    }

    private async Task LoadSingleScene(string sceneName, bool setActive)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        await asyncOp.AsTask();

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            _loadedScenes.Add(loadedScene);
            if (setActive)
            {
                SceneManager.SetActiveScene(loadedScene);
            }
        }
    }

    public async Task ReloadScene(bool addTransition = true)
    {
        // Handle test mode reload
        if (_isTestMode && !string.IsNullOrEmpty(_testScenePath))
        {
            await ReloadTestScene(addTransition);
            return;
        }

        if (_currentSceneGroup == null)
        {
            Debug.LogError("Cannot reload scene: No current scene group is active.");

            // Fallback: Try to find current scene group from CurrentScene
            SceneRecord record = SceneRecords.Find(r => r.SceneId == CurrentScene);
            if (record != null && record.SceneGroupAsset != null)
            {
                _currentSceneGroup = record.SceneGroupAsset;
            }
            else
            {
                Debug.LogError($"[SceneService] No SceneGroup found for current scene: {CurrentScene}");
                return;
            }
        }

        await _transitionCoordinator.ExecuteWithTransition(async () =>
        {
            // Capture scenes to unload BEFORE loading new ones (by handle, not name)
            var scenesToUnload = GetAllGameScenes();

            // Load new scenes
            await LoadSceneGroup(_currentSceneGroup, forceReload: true);

            // Now unload the old scenes (the ones we captured before)
            await UnloadScenes(scenesToUnload);

            OnSceneChanging?.Invoke(CurrentScene);
        }, addTransition);
    }

    /// <summary>
    /// Gets all game scenes (excludes bootstrap and persistent scenes).
    /// Returns actual Scene handles, not names.
    /// </summary>
    private List<Scene> GetAllGameScenes()
    {
        List<Scene> gameScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Skip bootstrap and persistent scenes
            if (scene == _bootstrapScene || _persistentSceneNames.Contains(scene.name))
                continue;

            if (scene.isLoaded && scene.IsValid())
            {
                gameScenes.Add(scene);
            }
        }
        return gameScenes;
    }

    /// <summary>
    /// Unloads a specific list of scenes.
    /// </summary>
    private async Task UnloadScenes(List<Scene> scenes)
    {
        List<Task> unloadTasks = new List<Task>();
        foreach (var scene in scenes)
        {
            if (scene.isLoaded && scene.IsValid())
            {
                BetterLogger.Log($"Scene To Unload: {scene.name}", BetterLogger.LogCategory.System);
                unloadTasks.Add(SceneManager.UnloadSceneAsync(scene).AsTask());
            }
        }
        if (unloadTasks.Any())
        {
            await Task.WhenAll(unloadTasks);
        }
    }

    /// <summary>
    /// Reloads the test scene in Editor mode.
    /// </summary>
    private async Task ReloadTestScene(bool addTransition = true)
    {
        await _transitionCoordinator.ExecuteWithTransition(async () =>
        {
            // Capture all current game scenes by handle BEFORE loading new one
            var scenesToUnload = GetAllGameScenes();

            // Load new instance of test scene
            var loadOperation = SceneManager.LoadSceneAsync(_testScenePath, LoadSceneMode.Additive);
            while (!loadOperation.isDone)
            {
                await Task.Yield();
            }

            // Find and set the NEW scene as active (it will be the one not in our unload list)
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == _testSceneName && !scenesToUnload.Contains(scene))
                {
                    SceneManager.SetActiveScene(scene);
                    break;
                }
            }

            // Now unload all the old scenes
            await UnloadScenes(scenesToUnload);
        }, addTransition);
    }

    private bool IsSceneLoaded(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}


[Serializable]
public class SceneRecord
{
    public SceneEnum SceneId;
    // Changed to hold a SceneGroup
    public SceneGroup SceneGroupAsset;
}

// Helper extension for converting AsyncOperation to Task
public static class AsyncOperationExtensions
{
    public static Task AsTask(this AsyncOperation operation)
    {
        if (operation == null)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<object>();
        operation.completed += _ => tcs.SetResult(null);
        return tcs.Task;
    }
}
