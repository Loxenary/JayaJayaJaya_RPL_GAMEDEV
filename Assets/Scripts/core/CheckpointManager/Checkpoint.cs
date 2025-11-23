using UnityEngine;

/// <summary>
/// Component untuk checkpoint di scene
/// Dapat diaktivasi secara otomatis (trigger) atau manual
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Configuration")]
    [SerializeField] private string checkpointID;
    [SerializeField] private bool activateOnTrigger = true;
    [SerializeField] private bool activateOnce = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField] private GameObject inactiveVisual;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private ParticleSystem activationEffect;

    [Header("Audio Feedback")]
    [SerializeField] private string activationSoundID;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private CheckpointManager checkpointManager;
    private AudioManager audioManager;
    private bool isActivated = false;

    public string CheckpointID => checkpointID;
    public bool IsActivated => isActivated;

    private void Awake()
    {
        // Validate collider setup
        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning($"[Checkpoint] {gameObject.name} collider should be set as trigger!", this);
        }

        // Auto-generate ID jika kosong
        if (string.IsNullOrEmpty(checkpointID))
        {
            checkpointID = $"Checkpoint_{gameObject.scene.name}_{gameObject.name}";

            if (showDebugInfo)
            {
                Debug.Log($"[Checkpoint] Auto-generated ID: {checkpointID}");
            }
        }
    }

    private void Start()
    {
        try
        {
            checkpointManager = ServiceLocator.Get<CheckpointManager>();

            // Optional: Get AudioManager jika tersedia
            try
            {
                audioManager = ServiceLocator.Get<AudioManager>();
            }
            catch
            {
                // AudioManager tidak tersedia, skip
            }

            checkpointManager.RegisterCheckpoint(this);

            // Check jika checkpoint ini sudah pernah diaktivasi sebelumnya
            isActivated = checkpointManager.IsCheckpointActivated(checkpointID);

            UpdateVisuals();

            if (showDebugInfo)
            {
                Debug.Log($"[Checkpoint] {checkpointID} initialized. Activated: {isActivated}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Checkpoint] Initialization failed: {e.Message}", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activateOnTrigger && other.CompareTag(playerTag))
        {
            if (activateOnce && isActivated)
            {
                return; // Sudah diaktivasi sebelumnya, skip
            }

            ActivateCheckpoint();
        }
    }

    /// <summary>
    /// Aktivasi checkpoint secara manual atau otomatis
    /// </summary>
    public void ActivateCheckpoint()
    {
        if (activateOnce && isActivated)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[Checkpoint] {checkpointID} already activated, skipping.");
            }
            return;
        }

        isActivated = true;
        checkpointManager.ActivateCheckpoint(checkpointID);

        UpdateVisuals();
        PlayActivationEffect();
        PlayActivationSound();

        if (showDebugInfo)
        {
            Debug.Log($"[Checkpoint] {checkpointID} activated!");
        }
    }

    /// <summary>
    /// Reset checkpoint (untuk testing)
    /// </summary>
    public void ResetCheckpoint()
    {
        isActivated = false;
        UpdateVisuals();

        if (showDebugInfo)
        {
            Debug.Log($"[Checkpoint] {checkpointID} reset");
        }
    }

    private void UpdateVisuals()
    {
        if (inactiveVisual != null)
        {
            inactiveVisual.SetActive(!isActivated);
        }

        if (activeVisual != null)
        {
            activeVisual.SetActive(isActivated);
        }
    }

    private void PlayActivationEffect()
    {
        if (activationEffect != null)
        {
            activationEffect.Play();
        }
    }

    private void PlayActivationSound()
    {
        if (audioManager != null && !string.IsNullOrEmpty(activationSoundID))
        {
            // Note: activationSoundID harus berupa SFXIdentifier enum
            // Uncomment jika sudah ada SFXIdentifier untuk checkpoint
            // audioManager.PlaySfx((SFXIdentifier)Enum.Parse(typeof(SFXIdentifier), activationSoundID));
        }
    }

    private void OnDestroy()
    {
        if (checkpointManager != null)
        {
            checkpointManager.UnregisterCheckpoint(checkpointID);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw checkpoint visualization di editor
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw arrow untuk menunjukkan arah spawn
        Gizmos.color = Color.blue;
        Vector3 direction = transform.forward * 2f;
        Gizmos.DrawLine(transform.position, transform.position + direction);

        // Draw arrow head
        Vector3 right = transform.right * 0.3f;
        Vector3 arrowTip = transform.position + direction;
        Gizmos.DrawLine(arrowTip, arrowTip - direction.normalized * 0.5f + right);
        Gizmos.DrawLine(arrowTip, arrowTip - direction.normalized * 0.5f - right);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detailed info saat selected
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 1f);

        // Draw checkpoint ID
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            string.IsNullOrEmpty(checkpointID) ? "No ID" : checkpointID
        );
        #endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure collider is trigger
        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }
#endif
}
