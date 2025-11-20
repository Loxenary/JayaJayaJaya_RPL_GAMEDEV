using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player/Rotation Config", fileName = "NewRotationConfig")]
public class RotationConfig : ScriptableObject
{
    [SerializeField] private float rotationSpeed = 10f;

    public float RotationSpeed => rotationSpeed;
}