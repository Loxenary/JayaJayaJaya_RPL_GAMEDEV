using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class to handle the game flow
/// </summary>

public class FlowManager : ServiceBase<FlowManager>
{

    public async void PlayGame()
    {
        await ServiceLocator.Get<SceneService>().LoadScene(SceneEnum.IN_GAME, true);
    }
}