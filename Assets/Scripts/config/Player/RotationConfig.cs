using UnityEngine;

public class RotationConfig : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;

    public float RotationSpeed => rotationSpeed;
}