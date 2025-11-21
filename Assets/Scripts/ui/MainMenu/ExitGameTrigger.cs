using UnityEngine;


public class ExitGameTrigger : BaseTrigger
{
    protected override void Trigger()
    {
        if (Application.isPlaying)
        {
            Application.Quit();
        }
    }
}