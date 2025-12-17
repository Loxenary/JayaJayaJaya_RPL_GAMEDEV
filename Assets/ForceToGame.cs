using UnityEngine;

public class ForceToGame : MonoBehaviour
{
    [ContextMenu("Force to Game")]
    public void ForceToGames()
    {
        ServiceLocator.Get<FlowManager>().PlayGame();

    }
}
