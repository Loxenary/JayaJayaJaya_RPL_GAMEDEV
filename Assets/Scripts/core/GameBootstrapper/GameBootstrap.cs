using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game entry point. Instantiates services, orchestrates initialization,
/// and loads the initial scenes. Services are expected to self-register.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Service Configuration")]
    [Tooltip("A list of service prefabs to instantiate at startup. These services should register themselves with the ServiceLocator in their Awake() method.")]
    [SerializeField] private List<GameObject> servicePrefabs;

    [Header("Scene Configuration")]
    [Tooltip("A list of scene references that should be loaded additively at startup.")]
    [SerializeField] private List<SceneReference> persistentSceneReferences;
    [Tooltip("The first scene to load after all services are initialized.")]
    [SerializeField] private SceneEnum initialScene;

    [Header("Testing Overrides")]
    [Tooltip("Enable this to load a specific test scene instead of the default initial scene.")]
    [SerializeField] private bool isTestingEnabled = false;

#if UNITY_EDITOR
    [Tooltip("The scene to load when IsTestingEnabled is true.")]
    [SerializeField] private SceneAsset testSceneToLoad;

    [SerializeField] private List<MonoScript> saveDataScriptsToReset;
#endif


    // Using async void Start is discouraged. This approach ensures exceptions are caught.
    private async void Start()
    {
        try
        {
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

        RegisterPersistentScenesWithService();

        await LoadPersistentScenesAsync();

        await InitializeServicesAsync();

        await LoadInitialSceneAsync();

    }

    /// <summary>
    /// Instantiates all service prefabs. Services are responsible for self-registration.
    /// </summary>
    private void InstantiateServices()
    {
        foreach (var prefab in servicePrefabs)
        {
            if (prefab != null)
            {
                Instantiate(prefab, transform);
            }
            else
            {
                Debug.LogWarning("[GameBootstrap] A null prefab was found in the servicePrefabs list.");
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


#if UNITY_EDITOR

        if (isTestingEnabled)
        {
            if (testSceneToLoad == null)
            {
                Debug.LogError("[GameBootstrap] Testing is enabled, but the 'Test Scene To Load' has not been assigned in the inspector.", this);
                return; // Stop here to prevent further errors.
            }

            var currentScene = SceneManager.GetActiveScene();

            // Loop through all loaded scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                // If the scene is loaded and it's not the current active scene, close it
                if (scene.isLoaded && scene.path != currentScene.path)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }



            // We assume the SceneService can also handle loading by a string scene name.
            // Using .name on a SceneAsset provides the correct scene name to load.
            SceneManager.LoadScene(testSceneToLoad.name, LoadSceneMode.Additive);

            // IMPORTANT: Exit the method here so we don't proceed to load the default initial scene.
            return;
        }
#endif

        // If we are in a build or if testing is disabled, load the default initial scene.
        await sceneService.LoadScene(initialScene, false);
    }
}