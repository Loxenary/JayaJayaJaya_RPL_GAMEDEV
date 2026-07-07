using System.Threading.Tasks;
using CustomLogger;
using UnityEngine;

public class RestartInvoker : MonoBehaviour
{
    public void Restart() => RestartAsync().Forget(nameof(RestartInvoker));

    private async Task RestartAsync()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService == null)
        {
            BetterLogger.LogError("[RestartInvoker] SceneService tidak ditemukan — restart dibatalkan. Periksa bootstrap.", BetterLogger.LogCategory.System);
            return;
        }

        RestartManager.Restart();
        await sceneService.ReloadScene(addTransition: true);
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
    }
}
