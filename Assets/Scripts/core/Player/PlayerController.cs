using UnityEngine;


///<summary>
/// This Class is a glue code class to handle Player with its input, then integrate witht the movement

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;

    [Header("Handlers")]
    [SerializeField] private PlayerMovementHandler movementHandler;


    private PlayerInputHandler inputHandler;

    private void Awake()
    {
        // Get or add input handler
        inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler == null)
        {
            inputHandler = gameObject.AddComponent<PlayerInputHandler>();
        }

        // Validate character controller
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError($"CharacterController is missing on {name}");
            }
        }

        // Lock cursor at start
        inputHandler.SetCursorState(true);
        movementHandler.SetCharacterController(characterController);
    }

    private void OnEnable()
    {
        // Subscribe to input events
        if (inputHandler != null)
        {
            inputHandler.OnJumpPerformed += HandleJump;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        if (inputHandler != null)
        {
            inputHandler.OnJumpPerformed -= HandleJump;
        }
    }

    private void Update()
    {
        HandleMovement();
        // HandleCamera();
    }

    private void HandleMovement()
    {
        if (inputHandler == null) return;

        // Get input
        Vector2 moveInput = inputHandler.MoveInput;
        bool sprint = inputHandler.IsSprintHeld;
        bool crouch = inputHandler.IsCrouchHeld;

        movementHandler.Move(moveInput, sprint, crouch);
    }

    private void HandleJump()
    {
        movementHandler.Jump();
    }
    public PlayerInputHandler GetInputHandler() => inputHandler;
}