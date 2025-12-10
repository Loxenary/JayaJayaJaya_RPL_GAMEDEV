using CustomLogger;
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

    [Header("Camera-Relative Movement")]
    [Tooltip("If enabled, movement will be relative to camera direction")]
    [SerializeField] private bool useCameraRelativeMovement = true;

    private PlayerInputHandler inputHandler;
    private CameraRelativeDirectionProvider directionProvider;

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

        // Setup camera-relative direction provider if enabled
        if (useCameraRelativeMovement)
        {
            directionProvider = GetComponent<CameraRelativeDirectionProvider>();
            if (directionProvider == null)
            {
                directionProvider = gameObject.AddComponent<CameraRelativeDirectionProvider>();

                BetterLogger.Log("Addding Camera Relative direction for Player Controlller");

                // Set camera reference
                if (playerCamera != null)
                {
                    directionProvider.SetCamera(playerCamera);
                }
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

        // Convert input to world-space direction if camera-relative movement is enabled
        if (useCameraRelativeMovement && directionProvider != null)
        {
            Vector3 worldDirection = directionProvider.GetCameraRelativeDirection(moveInput);
            movementHandler.Move(moveInput, sprint, crouch, worldDirection);
        }
        else
        {
            // Use default world-space movement
            movementHandler.Move(moveInput, sprint, crouch);
        }
    }

    private void HandleJump()
    {
        movementHandler.Jump();
    }

    /// <summary>
    /// Freeze or unfreeze the player (disables movement)
    /// </summary>
    public void SetFrozen(bool frozen)
    {
        if (movementHandler != null)
        {
            movementHandler.SetFrozen(frozen);
        }
    }

    public PlayerInputHandler GetInputHandler() => inputHandler;
}
