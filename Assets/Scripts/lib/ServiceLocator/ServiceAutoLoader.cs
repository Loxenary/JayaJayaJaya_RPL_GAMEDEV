using System.Collections.Generic;
using System.Linq;
using CustomLogger;
using UnityEngine;

/// <summary>
/// Automatically discovers and loads service prefabs from Resources/Services folder.
/// Services must be prefabs placed in Resources/Services/ to be auto-discovered.
/// Services should implement IService and register themselves in Awake().
/// </summary>
public static class ServiceAutoLoader
{
    private const string SERVICES_RESOURCE_PATH = "Services";

    /// <summary>
    /// Automatically loads all service prefabs from Resources/Services folder.
    /// Returns a list of instantiated service GameObjects.
    /// </summary>
    /// <param name="parent">Optional parent transform for the instantiated services</param>
    public static List<GameObject> LoadAllServices(Transform parent = null)
    {
        var instantiatedServices = new List<GameObject>();

        // Load all GameObjects from Resources/Services folder
        var servicePrefabs = Resources.LoadAll<GameObject>(SERVICES_RESOURCE_PATH);

        if (servicePrefabs.Length == 0)
        {
            BetterLogger.LogWarning(
                $"No service prefabs found in Resources/{SERVICES_RESOURCE_PATH}. Make sure your service prefabs are placed in Assets/Resources/Services/",
                BetterLogger.LogCategory.System
            );
            return instantiatedServices;
        }

        BetterLogger.Log($"Found {servicePrefabs.Length} service prefabs. Loading...", BetterLogger.LogCategory.System);

        foreach (var prefab in servicePrefabs)
        {
            var serviceComponent = prefab.GetComponent<MonoBehaviour>() as IService;

            if (serviceComponent == null)
            {
                var components = prefab.GetComponents<MonoBehaviour>();
                serviceComponent = components.FirstOrDefault(c => c is IService) as IService;
            }

            if (serviceComponent != null)
            {
                var instance = Object.Instantiate(prefab, parent);
                instance.name = prefab.name;
                instantiatedServices.Add(instance);
                BetterLogger.Log($"Loaded service: {prefab.name}", BetterLogger.LogCategory.System);
            }
            else
            {
                BetterLogger.LogWarning(
                    $"Prefab '{prefab.name}' in Resources/{SERVICES_RESOURCE_PATH} does not contain any component implementing IService. Skipping.",
                    BetterLogger.LogCategory.System
                );
            }
        }

        BetterLogger.Log($"Successfully loaded {instantiatedServices.Count} services.", BetterLogger.LogCategory.System);
        return instantiatedServices;
    }

    /// <summary>
    /// Loads a specific service by name from Resources/Services folder.
    /// </summary>
    public static GameObject LoadService(string serviceName, Transform parent = null)
    {
        var prefab = Resources.Load<GameObject>($"{SERVICES_RESOURCE_PATH}/{serviceName}");

        if (prefab == null)
        {
            BetterLogger.LogError(
                $"Service prefab '{serviceName}' not found in Resources/{SERVICES_RESOURCE_PATH}",
                BetterLogger.LogCategory.System
            );
            return null;
        }

        var instance = Object.Instantiate(prefab, parent);
        instance.name = prefab.name;
        BetterLogger.Log($"Loaded service: {serviceName}", BetterLogger.LogCategory.System);
        return instance;
    }
}
