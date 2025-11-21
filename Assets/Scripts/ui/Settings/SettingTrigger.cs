using UnityEngine;
using UnityEngine.UI;


public class SettingTrigger : BaseTrigger
{
    protected override void Trigger()
    {
        UIManager.Open<SettingsUI>();
    }
}