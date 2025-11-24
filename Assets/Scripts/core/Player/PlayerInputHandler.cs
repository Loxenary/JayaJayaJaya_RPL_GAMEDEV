using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    // Input values
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool crouchInput;
    private bool attackInput;
    private bool interactInput;

    // Input events
    public event Action OnJumpPerformed;
    public event Action OnAttackPerformed;
    public event Action OnInteractPerformed;

    // Properties to access input values
    public Vector2 MoveInput => moveInput;
    public Vector2 LookInput => lookInput;
    public bool IsSprintHeld => sprintInput;
    public bool IsCrouchHeld => crouchInput;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        // Enable the Player action map
        inputActions.Player.Enable();

        // Subscribe to input events
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.Jump.performed += OnJump;

        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;

        inputActions.Player.Crouch.performed += OnCrouch;
        inputActions.Player.Crouch.canceled += OnCrouch;

        inputActions.Player.Attack.performed += OnAttack;

        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Jump.performed -= OnJump;

        inputActions.Player.Sprint.performed -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;

        inputActions.Player.Crouch.performed -= OnCrouch;
        inputActions.Player.Crouch.canceled -= OnCrouch;

        inputActions.Player.Attack.performed -= OnAttack;

        inputActions.Player.Interact.performed -= OnInteract;

        // Disable the Player action map
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

    // Input callbacks
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = true;
        OnJumpPerformed?.Invoke();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprintInput = context.ReadValueAsButton();
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        crouchInput = context.ReadValueAsButton();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        attackInput = true;
        OnAttackPerformed?.Invoke();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        interactInput = true;
        OnInteractPerformed?.Invoke();
    }

    // Methods to consume one-time inputs (for jump, attack, etc.)
    public bool ConsumeJumpInput()
    {
        bool value = jumpInput;
        jumpInput = false;
        return value;
    }

    public bool ConsumeAttackInput()
    {
        bool value = attackInput;
        attackInput = false;
        return value;
    }

    public bool ConsumeInteractInput()
    {
        bool value = interactInput;
        interactInput = false;
        return value;
    }

    // Utility methods
    public void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}