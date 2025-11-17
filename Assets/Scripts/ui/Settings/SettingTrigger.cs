using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingTrigger : MonoBehaviour
{
    private Button _thisButton;

    private void Awake()
    {
        _thisButton = GetComponent<Button>();
        _thisButton.onClick.AddListener(Trigger);
    }

    private void Trigger()
    {
        UIManager.Open<Settings>();
    }
}