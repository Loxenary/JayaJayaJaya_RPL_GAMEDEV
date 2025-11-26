using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SceneBootstrapper : MonoBehaviour
{
    

    private async void Awake()
    {
        if (GameBootstrap.FLAG_STARTUP_BOOTSTRAP)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        ServiceAutoLoader.LoadAllServices(transform);

        await InitializeServicesAsync();
    }

    private async Task InitializeServicesAsync()
    {
        var initializableServices = ServiceLocator.GetAll<IInitializableService>();
        var sortedServices = initializableServices.OrderBy(s => s.InitializationPriority);

        foreach (var service in sortedServices)
        {
            await service.Initialize();
        }
    }
}