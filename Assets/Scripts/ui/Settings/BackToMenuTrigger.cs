using UnityEngine;

public class BackToMenuTrigger : MonoBehaviour
{
    public async void BackToMenu()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
        await sceneService.LoadScene(SceneEnum.MAIN_MENU, true);
    }
}