using UnityEngine;

/// <summary>
/// Contoh implementasi Ghost system dalam gameplay.
/// Script ini menunjukkan berbagai cara menggunakan Ghost untuk game horror.
/// </summary>
public class GhostGameplayExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GhostSpawner ghostSpawner;
    [SerializeField] private Transform player;

    [Header("Gameplay Events")]
    [SerializeField] private bool spawnGhostOnPlayerEnter = true;
    [SerializeField] private float triggerRadius = 5f;

    private bool hasTriggered = false;
    private GameObject spawnedGhost;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        // Example: Spawn ghost when player enters area
        if (spawnGhostOnPlayerEnter && !hasTriggered && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= triggerRadius)
            {
                OnPlayerEnterArea();
                hasTriggered = true;
            }
        }
    }

    /// <summary>
    /// Contoh: Spawn ghost saat player masuk area
    /// </summary>
    private void OnPlayerEnterArea()
    {
        Debug.Log("Player entered ghost trigger area!");

        if (ghostSpawner != null)
        {
            spawnedGhost = ghostSpawner.SpawnGhostAtRandomPoint();
        }
        else
        {
            // Spawn manual tanpa spawner
            Vector3 spawnPos = transform.position + transform.forward * 5f;
            spawnedGhost = GhostSetupHelper.CreateGhostFromScratch(spawnPos);
        }
    }

    /// <summary>
    /// Contoh: Destroy ghost saat puzzle solved
    /// </summary>
    public void OnPuzzleSolved()
    {
        Debug.Log("Puzzle solved! Removing ghost threat.");

        if (ghostSpawner != null)
        {
            ghostSpawner.DestroyAllGhosts();
        }
        else if (spawnedGhost != null)
        {
            Destroy(spawnedGhost);
        }
    }

    /// <summary>
    /// Contoh: Disable ghosts temporarily (safe zone)
    /// </summary>
    public void EnterSafeZone()
    {
        Debug.Log("Entered safe zone - ghosts deactivated");

        if (ghostSpawner != null)
        {
            ghostSpawner.SetAllGhostsActive(false);
        }
    }

    /// <summary>
    /// Contoh: Re-enable ghosts (exit safe zone)
    /// </summary>
    public void ExitSafeZone()
    {
        Debug.Log("Exited safe zone - ghosts reactivated");

        if (ghostSpawner != null)
        {
            ghostSpawner.SetAllGhostsActive(true);
        }
    }

    /// <summary>
    /// Contoh: Spawn wave of ghosts (boss fight, climax scene)
    /// </summary>
    public void SpawnGhostWave(int count)
    {
        Debug.Log($"Spawning ghost wave: {count} ghosts");

        if (ghostSpawner != null)
        {
            for (int i = 0; i < count; i++)
            {
                ghostSpawner.SpawnGhostAtRandomPoint();
            }
        }
    }

    // Gizmos untuk visualisasi trigger area
    private void OnDrawGizmosSelected()
    {
        if (spawnGhostOnPlayerEnter)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, triggerRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
