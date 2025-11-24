using UnityEngine;
using UnityEngine.UI;


public class PlayGameTrigger : BaseTrigger
{

    protected override void Trigger()
    {    
        ServiceLocator.Get<FlowManager>().PlayGame();
    }
}