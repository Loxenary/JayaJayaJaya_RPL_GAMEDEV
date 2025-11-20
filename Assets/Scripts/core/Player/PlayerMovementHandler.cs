using UnityEngine;
using System;

[Serializable]
internal class PlayerMovementHandler
{
    private CharacterController characterController;

    [Header("Movement Settings")]
    [SerializeField] private MovementConfig movementConfig;

    [Header("Jump Settings")]
    [SerializeField] private JumpConfig jumpConfig;

    [Header("Rotation Settings")]
    [SerializeField] private RotationConfig rotationConfig;

    private Vector3 currentVelocity;
    private Vector3 verticalVelocity;
    private bool isSprinting;
    private bool isCrouching;


    // Movement Variables
    private float _acceleration => movementConfig.Acceleration;
    private float _deceleration => movementConfig.Deceleration;
    private float _walkSpeed => movementConfig.WalkSpeed;
    private float _sprintSpeed => movementConfig.SprintSpeed;
    private float _crouchSpeed => movementConfig.CrouchSpeed;

    // Jump Variables
    private float _jumpHeight => jumpConfig.JumpHeight;
    private float _gravity => jumpConfig.Gravity;

    // Rotation Variables
    private float _rotationSpeed => rotationConfig.RotationSpeed;

    public void SetCharacterController(CharacterController controller)
    {
        characterController = controller;
    }

    /// <summary>
    /// Move the character with optional world-space direction override.
    /// </summary>
    /// <param name="inputDirection">Raw input direction (WASD as Vector2)</param>
    /// <param name="sprint">Is sprint button held</param>
    /// <param name="crouch">Is crouch button held</param>
    /// <param name="worldDirection">Optional: pre-converted world-space direction. If provided, ignores inputDirection</param>
    public void Move(Vector2 inputDirection, bool sprint, bool crouch, Vector3? worldDirection = null)
    {
        isSprinting = sprint;
        isCrouching = crouch;

        float targetSpeed = GetTargetSpeed();

        // Use provided world direction or convert input to world-space
        Vector3 moveDirection = worldDirection ?? new Vector3(inputDirection.x, 0f, inputDirection.y);
        Vector3 targetVelocity = moveDirection.normalized * targetSpeed;

        // Smooth movement using interpolation
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            (targetVelocity.magnitude > currentVelocity.magnitude ? _acceleration : _deceleration) * Time.deltaTime
        );

        // Apply gravity
        if (characterController.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // Small negative value to keep grounded
        }
        else
        {
            verticalVelocity.y += _gravity * Time.deltaTime;
        }

        // Combine horizontal and vertical movement
        Vector3 movement = (currentVelocity + verticalVelocity) * Time.deltaTime;
        characterController.Move(movement);
    }

    public void Rotate(Vector3 direction)
    {
        if (direction.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            characterController.transform.rotation = Quaternion.Slerp(
                characterController.transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }
    }

    public void RotateTowards(Vector3 targetDirection)
    {
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            characterController.transform.rotation = Quaternion.Slerp(
                characterController.transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
    }

    public float GetMoveSpeed()
    {
        return GetTargetSpeed();
    }

    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    public bool IsSprinting()
    {
        return isSprinting && !isCrouching;
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }

    private float GetTargetSpeed()
    {
        if (isCrouching) return _crouchSpeed;
        if (isSprinting) return _sprintSpeed;
        return _walkSpeed;
    }
}