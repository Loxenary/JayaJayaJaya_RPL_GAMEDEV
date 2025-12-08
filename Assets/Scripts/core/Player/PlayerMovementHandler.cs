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

  [Header("Sprint Camera Shake")]
  [SerializeField] private bool enableSprintShake = true;
  [SerializeField] private float shakeInterval = 0.4f; // Time between shakes
  [SerializeField] private float shakeForce = 0.15f; // Subtle shake

  private Vector3 currentVelocity;
  private Vector3 verticalVelocity;
  private bool isSprinting;
  private bool isCrouching;
  private bool isFrozen = false;
  private float lastShakeTime = 0f;

  // Use reflection to avoid hard dependency on Cinemachine
  private Component impulseSource;
  private System.Reflection.MethodInfo generateImpulseMethod;


  // Movement Variables
  private float _acceleration => movementConfig.Acceleration;
  private float _deceleration => movementConfig.Deceleration;
  private float _walkSpeed => movementConfig.WalkSpeed;
  private float _sprintSpeed => movementConfig.SprintSpeed;
  private float _crouchSpeed => movementConfig.CrouchSpeed;

  // Jump Variables
  private float _jumpHeight => jumpConfig.JumpHeight;
  private float _gravity => jumpConfig.Gravity;
  private float _fallMultiplier => jumpConfig.FallMultiplier;
  private float _lowJumpMultiplier => jumpConfig.LowJumpMultiplier;

  // Rotation Variables
  private float _rotationSpeed => rotationConfig.RotationSpeed;

  public void SetCharacterController(CharacterController controller)
  {
    characterController = controller;

    if (enableSprintShake)
    {
      // Try to setup Cinemachine Impulse Source using reflection (no hard dependency)
      SetupCameraShake(controller.gameObject);
    }
  }

  private void SetupCameraShake(GameObject playerObject)
  {
    // Try to find CinemachineImpulseSource type (works for both Cinemachine 2.x and 3.x)
    System.Type impulseType = System.Type.GetType("Unity.Cinemachine.CinemachineImpulseSource, Unity.Cinemachine") ??
                              System.Type.GetType("Cinemachine.CinemachineImpulseSource, Cinemachine");

    if (impulseType == null)
    {
      Debug.LogWarning("[PlayerMovement] Cinemachine not found! Install Cinemachine package for camera shake.");
      enableSprintShake = false;
      return;
    }

    // Get or add component
    impulseSource = playerObject.GetComponent(impulseType);
    if (impulseSource == null)
    {
      impulseSource = playerObject.AddComponent(impulseType);
      Debug.Log("[PlayerMovement] CinemachineImpulseSource added");
    }

    // Get GenerateImpulse method
    generateImpulseMethod = impulseType.GetMethod("GenerateImpulse", new System.Type[] { typeof(Vector3) });
    if (generateImpulseMethod == null)
    {
      // Try parameterless version
      generateImpulseMethod = impulseType.GetMethod("GenerateImpulse", System.Type.EmptyTypes);
    }

    if (generateImpulseMethod != null)
    {
      Debug.Log("[PlayerMovement] Camera shake ready!");
    }
    else
    {
      Debug.LogWarning("[PlayerMovement] Could not find GenerateImpulse method");
    }
  }

  /// <summary>
  /// Freeze or unfreeze player movement
  /// </summary>
  public void SetFrozen(bool frozen)
  {
    isFrozen = frozen;

    if (frozen)
    {
      // Stop all movement when frozen
      currentVelocity = Vector3.zero;
    }
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
    // Don't move if frozen
    if (isFrozen)
    {
      currentVelocity = Vector3.zero;
      return;
    }

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

    // Apply gravity with fall multiplier for realistic jump feel
    if (characterController.isGrounded && verticalVelocity.y < 0)
    {
      verticalVelocity.y = -2f; // Small negative value to keep grounded
    }
    else if (verticalVelocity.y < 0)
    {
      // Falling - apply fall multiplier for faster descent
      verticalVelocity.y += _gravity * _fallMultiplier * Time.deltaTime;
    }
    else
    {
      // Rising - normal gravity (or low jump multiplier if not holding jump)
      verticalVelocity.y += _gravity * Time.deltaTime;
    }

    // Combine horizontal and vertical movement
    Vector3 movement = (currentVelocity + verticalVelocity) * Time.deltaTime;
    characterController.Move(movement);

    // Camera shake when sprinting on ground
    if (enableSprintShake && isSprinting && characterController.isGrounded && moveDirection.magnitude > 0.1f)
    {
      if (Time.time - lastShakeTime >= shakeInterval)
      {
        TriggerCameraShake();
        lastShakeTime = Time.time;
      }
    }
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

  private void TriggerCameraShake()
  {
    if (impulseSource == null || generateImpulseMethod == null)
      return;

    try
    {
      // Generate shake direction - biased downward (simulating footstep impact)
      Vector3 shakeVelocity = new Vector3(
        UnityEngine.Random.Range(-shakeForce * 0.5f, shakeForce * 0.5f), // Horizontal (subtle)
        UnityEngine.Random.Range(-shakeForce, -shakeForce * 0.3f),       // Downward (main direction)
        0
      );

      // Try to invoke with Vector3 parameter
      if (generateImpulseMethod.GetParameters().Length > 0)
      {
        generateImpulseMethod.Invoke(impulseSource, new object[] { shakeVelocity });
      }
      else
      {
        // Fallback: parameterless version
        generateImpulseMethod.Invoke(impulseSource, null);
      }
    }
    catch (System.Exception e)
    {
      Debug.LogWarning($"[PlayerMovement] Camera shake failed: {e.Message}");
      enableSprintShake = false; // Disable to prevent spam
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
