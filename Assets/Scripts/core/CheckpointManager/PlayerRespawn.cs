using UnityEngine;

/// <summary>
/// Example component untuk menangani respawn player menggunakan CheckpointManager
/// Attach script ini ke player GameObject
/// </summary>
public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private bool respawnOnStart = false;
    [SerializeField] private float healthThreshold = 0f;

    private CheckpointManager checkpointManager;
    private IPlayerHealth playerHealth;

    private void Start()
    {
        // Get CheckpointManager service
        try
        {
            checkpointManager = ServiceLocator.Get<CheckpointManager>();

            // Subscribe to checkpoint events
            checkpointManager.OnCheckpointActivated += OnCheckpointReached;
            checkpointManager.OnPlayerRespawned += OnRespawned;
            checkpointManager.OnBeforeRespawn += OnBeforeRespawn;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerRespawn] Failed to get CheckpointManager: {e.Message}");
        }

        // Try to get player health component (optional)
        playerHealth = GetComponent<IPlayerHealth>();

        // Respawn ke checkpoint terakhir jika ada
        if (respawnOnStart && checkpointManager != null)
        {
            var checkpoint = checkpointManager.GetCurrentCheckpoint();
            if (checkpoint != null)
            {
                transform.position = checkpoint.position;
                transform.rotation = checkpoint.rotation;
                Debug.Log($"[PlayerRespawn] Spawned at checkpoint: {checkpoint.checkpointID}");
            }
        }
    }

    private void OnDestroy()
    {
        if (checkpointManager != null)
        {
            checkpointManager.OnCheckpointActivated -= OnCheckpointReached;
            checkpointManager.OnPlayerRespawned -= OnRespawned;
            checkpointManager.OnBeforeRespawn -= OnBeforeRespawn;
        }
    }

    private void Update()
    {
        // Example: Respawn jika health <= 0
        if (playerHealth != null && playerHealth.GetCurrentHealth() <= healthThreshold)
        {
            Die();
        }

        // Example: Press R to respawn (for testing)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[PlayerRespawn] Manual respawn triggered");
            RespawnAtLastCheckpoint();
        }
    }

    /// <summary>
    /// Panggil method ini ketika player mati
    /// </summary>
    public void Die()
    {
        Debug.Log("[PlayerRespawn] Player died! Respawning...");
        RespawnAtLastCheckpoint();
    }

    /// <summary>
    /// Respawn player ke checkpoint terakhir
    /// </summary>
    public void RespawnAtLastCheckpoint()
    {
        if (checkpointManager != null)
        {
            checkpointManager.RespawnPlayer(gameObject);
        }
        else
        {
            Debug.LogWarning("[PlayerRespawn] CheckpointManager not available!");
        }
    }

    #region Event Handlers

    private void OnCheckpointReached(CheckpointData data)
    {
        Debug.Log($"[PlayerRespawn] Checkpoint reached: {data.checkpointID} at {data.position}");

        // TODO: Add visual/audio feedback
        // - Play checkpoint reached sound
        // - Show UI notification
        // - Play particle effect
        // - Save game progress
    }

    private void OnBeforeRespawn()
    {
        Debug.Log("[PlayerRespawn] Preparing to respawn...");

        // TODO: Add respawn preparation
        // - Fade out screen
        // - Disable player input
        // - Play death animation
        // - Stop current actions
    }

    private void OnRespawned(CheckpointData data)
    {
        Debug.Log($"[PlayerRespawn] Respawned at: {data.checkpointID}");

        // TODO: Add post-respawn actions
        // - Fade in screen
        // - Enable player input
        // - Play respawn animation
        // - Reset player state

        // Reset player health (if available)
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (checkpointManager == null) return;

        var checkpoint = checkpointManager.GetCurrentCheckpoint();
        if (checkpoint != null)
        {
            // Draw line from player to last checkpoint
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, checkpoint.position);
        }
    }

    #endregion
}
