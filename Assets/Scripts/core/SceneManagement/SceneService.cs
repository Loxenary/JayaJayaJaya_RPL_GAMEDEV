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
            Debug.Log($"[SceneService] Found GameBootstrap in scene: {gameBootstrap.gameObject.scene.name}");
            return gameBootstrap.gameObject.scene;
        }

        
        Debug.Log("[SceneService] No GameBootstrap found - test mode, no scene will be protected");
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
            await LoadSceneGroup(groupToLoad);
            await UnloadUnusedScenes(groupToLoad);

            _currentSceneGroup = groupToLoad;
            CurrentScene = sceneId;
            OnSceneChanging?.Invoke(sceneId);
        }, addTransition);
    }


    /// <summary>
    /// Loads all scenes within a SceneGroup additively.
    /// </summary>
    private async Task LoadSceneGroup(SceneGroup group)
    {
        _loadedScenes.Clear();

        // Load the main scene first
        if (group.ActiveScene != null && !IsSceneLoaded(group.ActiveScene.SceneName))
        {
            await LoadSingleScene(group.ActiveScene.SceneName, true);
        }

        // Load all additional scenes
        if (group.AdditionalScenes != null)
        {
            foreach (var sceneRef in group.AdditionalScenes)
            {
                if (sceneRef != null && !IsSceneLoaded(sceneRef.SceneName))
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

        Debug.Log($"[SceneService] UnloadUnusedScenes - Bootstrap scene: {(_bootstrapScene.IsValid() ? _bootstrapScene.name : "NONE")}");
        Debug.Log($"[SceneService] Scenes to keep: {string.Join(", ", scenesToKeep)}");
        Debug.Log($"[SceneService] Persistent scenes: {string.Join(", ", _persistentSceneNames)}");
        Debug.Log($"[SceneService] Total loaded scenes: {SceneManager.sceneCount}");

        List<Task> unloadTasks = new List<Task>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            Debug.Log($"[SceneService] Checking scene '{scene.name}': isBootstrap={scene == _bootstrapScene}, inKeepList={scenesToKeep.Contains(scene.name)}, isPersistent={_persistentSceneNames.Contains(scene.name)}");

            // Don't unload the bootstrap scene (where this service lives) or scenes we want to keep
            if (scene == _bootstrapScene || scenesToKeep.Contains(scene.name) || _persistentSceneNames.Contains(scene.name))
            {
                Debug.Log($"[SceneService] KEEPING scene: {scene.name}");
                continue;
            }

            if (scene.isLoaded)
            {
                BetterLogger.Log($"Scene To Unload:  {scene.name}", BetterLogger.LogCategory.System);
                Debug.Log($"[SceneService] UNLOADING scene: {scene.name}");
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
        if (_currentSceneGroup == null)
        {
            Debug.LogWarning("Cannot reload scene: No current scene group is active.");
            return;
        }

        await _transitionCoordinator.ExecuteWithTransition(async () =>
        {
            await UnloadUnusedScenes(null);
            await LoadSceneGroup(_currentSceneGroup);
            OnSceneChanging?.Invoke(CurrentScene);
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