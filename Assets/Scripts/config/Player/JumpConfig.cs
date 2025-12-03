using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/Jump Config", fileName = "NewJumpConfig")]
public class JumpConfig : ScriptableObject
{
    [Tooltip("How high the player can jump (in meters)")]
    [SerializeField] private float jumpHeight = 1.2f;

    [Tooltip("Base gravity (negative value)")]
    [SerializeField] private float gravity = -20f;

    [Tooltip("Multiplier applied to gravity when falling (makes descent faster)")]
    [SerializeField] private float fallMultiplier = 2.5f;

    [Tooltip("Multiplier when player releases jump button early (short hop)")]
    [SerializeField] private float lowJumpMultiplier = 2f;

    public float JumpHeight => jumpHeight;
    public float Gravity => gravity;
    public float FallMultiplier => fallMultiplier;
    public float LowJumpMultiplier => lowJumpMultiplier;
}
