using System.Threading.Tasks;
using CustomLogger;
using UnityEngine;

public class BackToMenuTrigger : MonoBehaviour
{
    public void BackToMenu() => BackToMenuAsync().Forget(nameof(BackToMenuTrigger));

    private async Task BackToMenuAsync()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService == null)
        {
            BetterLogger.LogError("[BackToMenuTrigger] SceneService tidak ditemukan — kembali ke menu dibatalkan. Periksa bootstrap.", BetterLogger.LogCategory.System);
            return;
        }

        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
        await sceneService.LoadScene(SceneEnum.MAIN_MENU, true);
    }
}
