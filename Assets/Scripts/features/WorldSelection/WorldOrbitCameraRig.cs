using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Free orbit/pan camera rig for the world selection map.
/// The rig's own position is the orbit pivot; the controlled camera is placed
/// at (pitch, yaw, distance) around it. Pitch is clamped so the view stays
/// top-down-ish. Also provides scripted zoom in/out tweens toward a building's
/// camera anchor, used when entering/leaving a story.
/// </summary>
public class WorldOrbitCameraRig : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera moved by this rig (usually the scene's Main Camera).")]
    [SerializeField] private Transform cameraTransform;

    [Header("Orbit (left drag)")]
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 55f;
    [SerializeField] private float minPitch = 35f;
    [SerializeField] private float maxPitch = 75f;
    [Tooltip("Degrees per pixel dragged.")]
    [SerializeField] private float orbitSpeed = 0.25f;

    [Header("Zoom (scroll)")]
    [SerializeField] private float distance = 30f;
    [SerializeField] private float minDistance = 12f;
    [SerializeField] private float maxDistance = 60f;
    [SerializeField] private float zoomStep = 3f;

    [Header("Pan (right/middle drag)")]
    [Tooltip("World units per pixel dragged.")]
    [SerializeField] private float panSpeed = 0.05f;
    [Tooltip("How far the pivot may pan from its start position on X/Z.")]
    [SerializeField] private Vector2 panExtents = new Vector2(25f, 25f);

    [Header("Smoothing")]
    [SerializeField] private float smoothing = 12f;

    /// <summary>Set false to ignore mouse input (during animations/replays).</summary>
    public bool InputEnabled { get; set; } = true;

    /// <summary>True while a zoom tween or snapped pose controls the camera instead of the orbit.</summary>
    public bool IsPoseOverridden => _poseOverride;

    private Vector3 _initialPivot;
    private float _currentYaw, _currentPitch, _currentDistance;
    private bool _poseOverride;
    private bool _uiBlockedDrag;
    private Tween _activeTween;

    private void Awake()
    {
        _initialPivot = transform.position;
        SyncCurrentToTargets();
    }

    private void Start()
    {
        if (!_poseOverride)
        {
            ApplyOrbitPose();
        }
    }

    private void LateUpdate()
    {
        if (_poseOverride || cameraTransform == null)
        {
            return;
        }

        if (InputEnabled)
        {
            ReadInput();
        }

        float t = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        _currentYaw = Mathf.LerpAngle(_currentYaw, yaw, t);
        _currentPitch = Mathf.Lerp(_currentPitch, pitch, t);
        _currentDistance = Mathf.Lerp(_currentDistance, distance, t);

        ApplyOrbitPose();
    }

    private void OnDestroy()
    {
        _activeTween?.Kill();
    }

    // ---------------------------------------------------------------------
    // Scripted zoom choreography
    // ---------------------------------------------------------------------

    /// <summary>Tweens the camera toward a building anchor (entering a story).</summary>
    public Tween ZoomTo(Transform anchor, float duration)
    {
        BeginPoseOverride();

        var sequence = DOTween.Sequence()
            .Join(cameraTransform.DOMove(anchor.position, duration).SetEase(Ease.InOutSine))
            .Join(cameraTransform.DORotateQuaternion(anchor.rotation, duration).SetEase(Ease.InOutSine))
            .SetUpdate(true);

        _activeTween = sequence;
        return sequence;
    }

    /// <summary>
    /// Snaps the camera to a building anchor and tweens it back out to the orbit pose
    /// (returning from a story). Orbit input is re-enabled when the tween completes.
    /// </summary>
    public Tween ZoomOutFrom(Transform anchor, float duration)
    {
        BeginPoseOverride();
        cameraTransform.SetPositionAndRotation(anchor.position, anchor.rotation);

        SyncCurrentToTargets();
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = transform.position - targetRotation * Vector3.forward * distance;

        var sequence = DOTween.Sequence()
            .Join(cameraTransform.DOMove(targetPosition, duration).SetEase(Ease.InOutSine))
            .Join(cameraTransform.DORotateQuaternion(targetRotation, duration).SetEase(Ease.InOutSine))
            .SetUpdate(true)
            .OnComplete(EndPoseOverride);

        _activeTween = sequence;
        return sequence;
    }

    /// <summary>Places the camera at an anchor without animating (before a zoom-out).</summary>
    public void SnapTo(Transform anchor)
    {
        BeginPoseOverride();
        cameraTransform.SetPositionAndRotation(anchor.position, anchor.rotation);
    }

    // ---------------------------------------------------------------------

    private void ReadInput()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Orbit: hold left button and drag (a drag that started over UI is ignored entirely)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            _uiBlockedDrag = overUI;
        }
        if (mouse.leftButton.isPressed && !_uiBlockedDrag)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * orbitSpeed;
            pitch = Mathf.Clamp(pitch - delta.y * orbitSpeed, minPitch, maxPitch);
        }

        // Pan: hold right or middle button and drag, moving the pivot on the ground plane
        if ((mouse.rightButton.isPressed || mouse.middleButton.isPressed) && !overUI)
        {
            Vector2 delta = mouse.delta.ReadValue();
            Vector3 forward = Quaternion.Euler(0f, _currentYaw, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, _currentYaw, 0f) * Vector3.right;

            Vector3 target = transform.position + (-right * delta.x - forward * delta.y) * panSpeed;
            target.x = Mathf.Clamp(target.x, _initialPivot.x - panExtents.x, _initialPivot.x + panExtents.x);
            target.z = Mathf.Clamp(target.z, _initialPivot.z - panExtents.y, _initialPivot.z + panExtents.y);
            target.y = _initialPivot.y;
            transform.position = target;
        }

        // Zoom: scroll wheel (sign only, so trackpads and mice behave the same)
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * zoomStep, minDistance, maxDistance);
        }
    }

    private void ApplyOrbitPose()
    {
        Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 position = transform.position - rotation * Vector3.forward * _currentDistance;
        cameraTransform.SetPositionAndRotation(position, rotation);
    }

    private void BeginPoseOverride()
    {
        _activeTween?.Kill();
        _activeTween = null;
        _poseOverride = true;
        InputEnabled = false;
    }

    private void EndPoseOverride()
    {
        _activeTween = null;
        _poseOverride = false;
        InputEnabled = true;
    }

    private void SyncCurrentToTargets()
    {
        _currentYaw = yaw;
        _currentPitch = pitch;
        _currentDistance = distance;
    }
}
