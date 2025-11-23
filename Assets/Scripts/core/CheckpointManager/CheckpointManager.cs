using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Service untuk mengelola sistem checkpoint dalam game
/// Mendukung registrasi checkpoint, aktivasi, respawn player, dan persistensi data
/// </summary>
public class CheckpointManager : MonoBehaviour, IInitializableService
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool autoSaveOnCheckpoint = true;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private bool debugMode = false;

    private Dictionary<string, CheckpointData> checkpoints = new Dictionary<string, CheckpointData>();
    private CheckpointData currentCheckpoint;
    private CheckpointData lastActivatedCheckpoint;

    /// <summary>
    /// Event dipanggil ketika checkpoint diaktivasi
    /// </summary>
    public event Action<CheckpointData> OnCheckpointActivated;

    /// <summary>
    /// Event dipanggil ketika player respawn
    /// </summary>
    public event Action<CheckpointData> OnPlayerRespawned;

    /// <summary>
    /// Event dipanggil sebelum player respawn (untuk fade out, dll)
    /// </summary>
    public event Action OnBeforeRespawn;

    public ServicePriority InitializationPriority => ServicePriority.SECONDARY;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    public async Task Initialize()
    {
        try
        {
            await Task.Yield(); // Membuat method async
            LoadCheckpointData();

            if (debugMode)
            {
                Debug.Log("[CheckpointManager] Initialized successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CheckpointManager] Initialization failed: {e.Message}");
        }
    }

    /// <summary>
    /// Register checkpoint ke dalam sistem
    /// </summary>
    /// <param name="checkpoint">Component checkpoint yang akan diregister</param>
    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
        {
            Debug.LogWarning("[CheckpointManager] Attempted to register null checkpoint");
            return;
        }

        if (!checkpoints.ContainsKey(checkpoint.CheckpointID))
        {
            var data = new CheckpointData(
                checkpoint.CheckpointID,
                checkpoint.transform.position,
                checkpoint.transform.rotation,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );

            checkpoints.Add(checkpoint.CheckpointID, data);

            if (debugMode)
            {
                Debug.Log($"[CheckpointManager] Registered checkpoint: {checkpoint.CheckpointID}");
            }
        }
        else
        {
            Debug.LogWarning($"[CheckpointManager] Checkpoint {checkpoint.CheckpointID} already registered");
        }
    }

    /// <summary>
    /// Unregister checkpoint dari sistem (untuk cleanup)
    /// </summary>
    /// <param name="checkpointID">ID checkpoint yang akan diunregister</param>
    public void UnregisterCheckpoint(string checkpointID)
    {
        if (checkpoints.ContainsKey(checkpointID))
        {
            checkpoints.Remove(checkpointID);

            if (debugMode)
            {
                Debug.Log($"[CheckpointManager] Unregistered checkpoint: {checkpointID}");
            }
        }
    }

    /// <summary>
    /// Aktivasi checkpoint dan simpan sebagai spawn point
    /// </summary>
    /// <param name="checkpointID">ID checkpoint yang akan diaktivasi</param>
    public void ActivateCheckpoint(string checkpointID)
    {
        if (checkpoints.TryGetValue(checkpointID, out CheckpointData data))
        {
            data.isActivated = true;
            data.activationTime = DateTime.Now;

            lastActivatedCheckpoint = data;
            currentCheckpoint = data;

            OnCheckpointActivated?.Invoke(data);

            if (autoSaveOnCheckpoint)
            {
                SaveCheckpointData();
            }

            if (debugMode)
            {
                Debug.Log($"[CheckpointManager] Activated checkpoint: {checkpointID} at position {data.position}");
            }
        }
        else
        {
            Debug.LogWarning($"[CheckpointManager] Checkpoint {checkpointID} not found in registry");
        }
    }

    /// <summary>
    /// Respawn player ke checkpoint terakhir
    /// </summary>
    /// <param name="player">GameObject player yang akan direspawn</param>
    public void RespawnPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[CheckpointManager] Cannot respawn null player");
            return;
        }

        if (currentCheckpoint == null)
        {
            Debug.LogWarning("[CheckpointManager] No checkpoint available for respawn! Using current position.");
            return;
        }

        StartCoroutine(RespawnCoroutine(player));
    }

    private System.Collections.IEnumerator RespawnCoroutine(GameObject player)
    {
        OnBeforeRespawn?.Invoke();

        yield return new WaitForSeconds(respawnDelay);

        // Teleport player
        if (player != null)
        {
            player.transform.position = currentCheckpoint.position;
            player.transform.rotation = currentCheckpoint.rotation;

            // Reset player state (jika ada interface IPlayerHealth)
            var playerHealth = player.GetComponent<IPlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            // Reset rigidbody velocity jika ada
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            OnPlayerRespawned?.Invoke(currentCheckpoint);

            if (debugMode)
            {
                Debug.Log($"[CheckpointManager] Player respawned at: {currentCheckpoint.checkpointID}");
            }
        }
    }

    /// <summary>
    /// Get posisi checkpoint terakhir
    /// </summary>
    public Vector3 GetLastCheckpointPosition()
    {
        return currentCheckpoint?.position ?? Vector3.zero;
    }

    /// <summary>
    /// Get rotation checkpoint terakhir
    /// </summary>
    public Quaternion GetLastCheckpointRotation()
    {
        return currentCheckpoint?.rotation ?? Quaternion.identity;
    }

    /// <summary>
    /// Get data checkpoint terakhir
    /// </summary>
    public CheckpointData GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    /// <summary>
    /// Check apakah checkpoint tertentu sudah diaktivasi
    /// </summary>
    public bool IsCheckpointActivated(string checkpointID)
    {
        if (checkpoints.TryGetValue(checkpointID, out CheckpointData data))
        {
            return data.isActivated;
        }
        return false;
    }

    /// <summary>
    /// Get semua checkpoint yang sudah diaktivasi
    /// </summary>
    public List<CheckpointData> GetActivatedCheckpoints()
    {
        var activated = new List<CheckpointData>();
        foreach (var checkpoint in checkpoints.Values)
        {
            if (checkpoint.isActivated)
            {
                activated.Add(checkpoint);
            }
        }
        return activated;
    }

    /// <summary>
    /// Reset semua checkpoint (untuk testing atau new game)
    /// </summary>
    public void ResetAllCheckpoints()
    {
        foreach (var checkpoint in checkpoints.Values)
        {
            checkpoint.isActivated = false;
        }
        currentCheckpoint = null;
        lastActivatedCheckpoint = null;

        if (debugMode)
        {
            Debug.Log("[CheckpointManager] All checkpoints reset");
        }
    }

    private async void SaveCheckpointData()
    {
        if (currentCheckpoint != null)
        {
            try
            {
                var saveData = new CheckpointSaveData
                {
                    lastCheckpointID = currentCheckpoint.checkpointID,
                    lastPosition = currentCheckpoint.position,
                    lastRotation = currentCheckpoint.rotation,
                    lastSceneName = currentCheckpoint.sceneName,
                    lastActivationTime = currentCheckpoint.activationTime
                };

                await SaveLoadManager.SaveAsync(saveData);

                if (debugMode)
                {
                    Debug.Log($"[CheckpointManager] Checkpoint data saved: {currentCheckpoint.checkpointID}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CheckpointManager] Failed to save checkpoint data: {e.Message}");
            }
        }
    }

    private async void LoadCheckpointData()
    {
        try
        {
            var saveData = await SaveLoadManager.LoadAsync<CheckpointSaveData>();
            if (saveData != null && !string.IsNullOrEmpty(saveData.lastCheckpointID))
            {
                currentCheckpoint = new CheckpointData
                {
                    checkpointID = saveData.lastCheckpointID,
                    position = saveData.lastPosition,
                    rotation = saveData.lastRotation,
                    sceneName = saveData.lastSceneName,
                    isActivated = true,
                    activationTime = saveData.lastActivationTime
                };

                if (debugMode)
                {
                    Debug.Log($"[CheckpointManager] Loaded checkpoint: {currentCheckpoint.checkpointID}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CheckpointManager] Failed to load checkpoint data: {e.Message}");
        }
    }

    public void Cleanup()
    {
        checkpoints.Clear();
        currentCheckpoint = null;
        lastActivatedCheckpoint = null;

        if (debugMode)
        {
            Debug.Log("[CheckpointManager] Cleanup completed");
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}

/// <summary>
/// Interface untuk player health system (optional)
/// Implementasikan interface ini di player health script Anda
/// </summary>
public interface IPlayerHealth
{
    void ResetHealth();
    void TakeDamage(float amount);
    float GetCurrentHealth();
}
