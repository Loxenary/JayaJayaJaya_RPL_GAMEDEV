using UnityEngine;

public class BackToMenuTrigger : MonoBehaviour
{
    public async void BackToMenu()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        await sceneService.LoadScene(SceneEnum.MAIN_MENU, true);
    }
}