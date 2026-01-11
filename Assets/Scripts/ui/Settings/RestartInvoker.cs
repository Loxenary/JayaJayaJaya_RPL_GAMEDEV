using UnityEngine;
public class RestartInvoker : MonoBehaviour
{
    public async void Restart()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService != null)
        {
            RestartManager.Restart();
            await sceneService.ReloadScene(addTransition: true);
            ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
        }
    }
}