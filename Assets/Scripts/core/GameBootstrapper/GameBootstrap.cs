using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game entry point. Instantiates services, orchestrates initialization,
/// and loads the initial scenes. Services are expected to self-register.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Service Configuration")]
    [Tooltip("Enable to automatically load all services from Resources/Services folder. If disabled, uses the manual servicePrefabs list.")]
    [SerializeField] private bool useAutoServiceDiscovery = true;

    [Tooltip("A list of service prefabs to instantiate at startup. Only used if useAutoServiceDiscovery is false. Services should register themselves with the ServiceLocator in their Awake() method.")]
    [SerializeField] private List<GameObject> servicePrefabs;

    [Header("Game Configuration")]
    [SerializeField] private BootstrapConfig _config;

    [Header("Scene Configuration")]
    private List<SceneReference> persistentSceneReferences => _config.PersistanceSceneReferences ?? new();

    // Properties to get scene values from config
    private SceneEnum initialScene => _config.InitialScene;
    private bool isTestingEnabled => _config.IsTestingEnabled;

#if UNITY_EDITOR
    [Header("Editor Only - Data Reset")]
    [SerializeField] private List<MonoScript> saveDataScriptsToReset;
#endif

    public static bool FLAG_STARTUP_BOOTSTRAP = false;


    // Using async void Start is discouraged. This approach ensures exceptions are caught.
    private async void Start()
    {
        try
        {
            FLAG_STARTUP_BOOTSTRAP = true;
            await BootstrapApplication();
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameBootstrap] A critical error occurred during bootstrap: {e.Message}\n{e.StackTrace}");
            // Consider quitting the application or showing an error screen
            // Application.Quit();
        }
    }

    private void RegisterPersistentScenesWithService()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService == null)
        {
            Debug.LogError("[GameBootstrap] SceneService not found after instantiation. Cannot register persistent scenes.");
            return;
        }

        // Get the names of the persistent scenes from the inspector list.
        var persistentSceneNames = persistentSceneReferences
            .Where(r => r != null && !string.IsNullOrEmpty(r.SceneName))
            .Select(r => r.SceneName);

        sceneService.RegisterPersistentScenes(persistentSceneNames);
    }

    /// <summary>
    /// Coordinates the entire startup sequence.
    /// </summary>
    private async Task BootstrapApplication()
    {
        DontDestroyOnLoad(gameObject);

        InstantiateServices();

#if UNITY_EDITOR
        // In testing mode, skip persistent scenes and load test scene directly
        if (isTestingEnabled)
        {
            await InitializeServicesAsync();
            await LoadTestSceneAsync();
            return;
        }
#endif

        RegisterPersistentScenesWithService();

        await LoadPersistentScenesAsync();

        await InitializeServicesAsync();

        await LoadInitialSceneAsync();

    }

    /// <summary>
    /// Instantiates all service prefabs. Services are responsible for self-registration.
    /// Uses auto-discovery if enabled, otherwise uses the manual servicePrefabs list.
    /// </summary>
    private void InstantiateServices()
    {
        if (useAutoServiceDiscovery)
        {
            ServiceAutoLoader.LoadAllServices(transform);
        }
        else
        {
            foreach (var prefab in servicePrefabs)
            {
                if (prefab != null)
                {
                    Instantiate(prefab, transform);
                }
            }
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// Deletes player save data asynchronously for testing purposes.
    /// The callback is invoked with 'true' for success and 'false' for failure.
    /// Using 'async void' is acceptable here as it's a top-level "fire-and-forget" call from a UI event.
    /// </summary>
    public async void ResetData(Action<(bool success, string message)> onCompleted)
    {
        var result = await SaveLoadManager.ResetResettableDataAsync();
        onCompleted?.Invoke(result);
    }
    /// <summary>
    /// Deletes specific player save data asynchronously based on a list of script references.
    /// </summary>
    public async void ResetSelectedData(List<MonoScript> scriptsToReset, Action<(bool success, string message)> onCompleted)
    {
        var typesToReset = scriptsToReset
            .Select(script => script.GetClass())
            .Where(type => type != null)
            .ToList();

        var result = await SaveLoadManager.ResetSelectedDataAsync(typesToReset);
        onCompleted?.Invoke(result);
    }
#endif

    /// <summary>
    /// Finds all registered services that require initialization and runs their Initialize method.
    /// Services are initialized based on their specified priority.
    /// </summary>
    private async Task InitializeServicesAsync()
    {

        var initializableServices = ServiceLocator.GetAll<IInitializableService>();

        // Sort services by priority to ensure correct initialization order.
        var sortedServices = initializableServices.OrderBy(s => s.InitializationPriority);

        foreach (var service in sortedServices)
        {
            await service.Initialize();
        }
    }

    /// <summary>
    /// Loads all persistent scenes additively at startup.
    /// </summary>
    private async Task LoadPersistentScenesAsync()
    {
        foreach (var sceneRef in persistentSceneReferences)
        {
            if (sceneRef != null && !string.IsNullOrEmpty(sceneRef.SceneName) && !SceneManager.GetSceneByName(sceneRef.SceneName).isLoaded)
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneRef.SceneName, LoadSceneMode.Additive);
                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }
            }
        }
    }

    /// <summary>
    /// Loads the final gameplay or test scene.
    /// </summary>
    private async Task LoadInitialSceneAsync()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService == null)
        {
            throw new ApplicationException("[GameBootstrap] SceneService not found. Cannot load the initial scene. Ensure it is included in the service prefabs list.");
        }

        await sceneService.LoadScene(initialScene, false);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Loads the test scene directly in Editor mode (no Build Settings required).
    /// Skips persistent scenes for clean isolated testing.
    /// </summary>
    private async Task LoadTestSceneAsync()
    {
        string testScenePath = _config.TestScenePath;
        string testSceneName = _config.TestSceneName;

        if (string.IsNullOrEmpty(testScenePath))
        {
            Debug.LogError("[GameBootstrap] Testing is enabled, but no test scene has been assigned in BootstrapConfig.", this);
            return;
        }

        // Register test scene info with SceneService for reload support
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            sceneService.SetTestMode(testScenePath, testSceneName);
        }

        // Load scene directly by path in Editor - no Build Settings needed!
        var loadOperation = SceneManager.LoadSceneAsync(testScenePath, LoadSceneMode.Additive);
        while (!loadOperation.isDone)
        {
            await Task.Yield();
        }

        // Set it as the active scene
        var loadedTestScene = SceneManager.GetSceneByName(testSceneName);
        if (loadedTestScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedTestScene);
        }
    }
#endif
}
