using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingInputListener : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private event Action OnSettingInputPerformed;

    private void OnEnable()
    {
        inputActions.UI.Setting.performed += ToggleSetting;
    }

    private void ToggleSetting(InputAction.CallbackContext context)
    {
        UIManager.Toggle<SettingsUI>();
    }
}