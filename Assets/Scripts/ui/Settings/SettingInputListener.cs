using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingInputListener : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        // Enable the UI action map
        inputActions.UI.Enable();

        // Subscribe to the Setting action
        inputActions.UI.Setting.performed += ToggleSetting;
    }

    private void OnDisable()
    {
        // Unsubscribe from the Setting action
        inputActions.UI.Setting.performed -= ToggleSetting;

        // Disable the UI action map
        inputActions.UI.Disable();
    }

    private void OnDestroy()
    {
        // Dispose of input actions when destroyed
        inputActions?.Dispose();
    }

    private void ToggleSetting(InputAction.CallbackContext context)
    {
        Debug.Log("Settings toggle triggered!");
        UIManager.Toggle<SettingsUI>();
    }
}