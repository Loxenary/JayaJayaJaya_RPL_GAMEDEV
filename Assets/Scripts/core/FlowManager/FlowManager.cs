using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class to handle the game flow
/// </summary>

public class FlowManager : ServiceBase<FlowManager>
{
    // Entry point tetap void agar kompatibel UnityEvent/button;
    // exception async tertangkap via Forget (B-15).
    public void PlayGame() => PlayGameAsync().Forget(nameof(PlayGame));

    private async Task PlayGameAsync()
    {
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
        await ServiceLocator.Get<SceneService>().LoadScene(SceneEnum.IN_GAME, true);
    }
}