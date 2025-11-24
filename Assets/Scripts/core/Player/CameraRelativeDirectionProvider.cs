using UnityEngine;

/// <summary>
/// Provides camera-relative direction conversion for player input.
/// Converts input (WASD) to world-space direction based on camera orientation.
/// This allows movement to feel natural even with tilted or rotating cameras.
/// </summary>
public class CameraRelativeDirectionProvider : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera;

    [Header("Settings")]
    [Tooltip("If true, ignores camera's vertical tilt (Y-axis remains level)")]
    [SerializeField] private bool flattenVertical = true;

    private void Awake()
    {
        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogError($"[CameraRelativeDirectionProvider] No camera assigned on {name}");
        }
    }

    /// <summary>
    /// Converts input direction (from keyboard/gamepad) to world-space direction based on camera orientation.
    /// </summary>
    /// <param name="inputDirection">Raw input (e.g., WASD as Vector2)</param>
    /// <returns>World-space direction the player should move</returns>
    public Vector3 GetCameraRelativeDirection(Vector2 inputDirection)
    {
        if (playerCamera == null)
        {
            // Fallback to world-space if no camera
            return new Vector3(inputDirection.x, 0f, inputDirection.y);
        }

        // Get camera's forward and right vectors
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        if (flattenVertical)
        {
            // Project vectors onto horizontal plane (ignore camera tilt)
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            // Normalize after projection
            cameraForward.Normalize();
            cameraRight.Normalize();
        }

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraRight * inputDirection.x) + (cameraForward * inputDirection.y);

        return moveDirection;
    }

    /// <summary>
    /// Set a new camera reference at runtime
    /// </summary>
    public void SetCamera(Camera camera)
    {
        playerCamera = camera;
    }

    /// <summary>
    /// Get the current camera reference
    /// </summary>
    public Camera GetCamera()
    {
        return playerCamera;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Vector3 position = transform.position + Vector3.up * 0.1f;

        // Draw camera forward (blue)
        Vector3 forward = playerCamera.transform.forward;
        if (flattenVertical)
        {
            forward.y = 0f;
            forward.Normalize();
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(position, forward * 2f);

        // Draw camera right (red)
        Vector3 right = playerCamera.transform.right;
        if (flattenVertical)
        {
            right.y = 0f;
            right.Normalize();
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(position, right * 2f);
    }
#endif
}
