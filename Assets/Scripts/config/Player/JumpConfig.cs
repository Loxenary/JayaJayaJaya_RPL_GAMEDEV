using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/Jump Config", fileName = "NewJumpConfig")]
public class JumpConfig : ScriptableObject
{
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    public float JumpHeight => jumpHeight;  

    public float Gravity => gravity;
}