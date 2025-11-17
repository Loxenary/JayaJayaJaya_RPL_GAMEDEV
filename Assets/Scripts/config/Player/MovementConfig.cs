using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/MovementConfig", fileName = "NewMovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("Movement Settings")]

    [Tooltip("Configuration for Normal Speed")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("Configuration for the Sprint Speed")]
    [SerializeField] private float sprintSpeed = 8f;

    [Tooltip("Configuration for the Crouching Speed")]
    [SerializeField] private float crouchSpeed = 2.5f;

    [Tooltip("Speed of which the movement will build until it reach the max Speed")]
    [SerializeField] private float acceleration = 10f;

    [Tooltip("Speed of which the movement will decrease until it reach zero")]
    [SerializeField] private float deceleration = 10f;

    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;
    public float CrouchSpeed => crouchSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
}