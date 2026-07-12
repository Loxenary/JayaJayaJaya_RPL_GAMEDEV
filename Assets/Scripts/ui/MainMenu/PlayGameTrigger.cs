using UnityEngine;
using UnityEngine.UI;


public class PlayGameTrigger : BaseTrigger
{

    protected override void Trigger()
    {
        _ = ServiceLocator.Get<FlowManager>().OpenSelection();
    }
}