using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manager untuk mengelola spawn, tracking, dan kontrol multiple ghosts di scene.
/// </summary>
public class GhostSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxGhostsInScene = 5;
    [SerializeField] private float spawnInterval = 30f;
    [SerializeField] private bool autoSpawn = false;

    [Header("Ghost Configuration")]
    [SerializeField] private int defaultFearDamage = 10;

    [Header("Events")]
    public UnityEvent<GameObject> OnGhostSpawned;
    public UnityEvent<GameObject> OnGhostDestroyed;
    public UnityEvent OnMaxGhostsReached;

    [Header("Debug Info")]
    [ReadOnly]
    [SerializeField] private int currentGhostCount = 0;
    [ReadOnly]
    [SerializeField] private List<GameObject> activeGhosts = new List<GameObject>();

    private float spawnTimer = 0f;

    private void Start()
    {
        if (autoSpawn && spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnTimer = spawnInterval;
        }
    }

    private void Update()
    {
        if (autoSpawn && currentGhostCount < maxGhostsInScene)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnGhostAtRandomPoint();
                spawnTimer = spawnInterval;
            }
        }

        // Clean up null references
        activeGhosts.RemoveAll(ghost => ghost == null);
        currentGhostCount = activeGhosts.Count;
    }

    /// <summary>
    /// Spawn ghost di spawn point yang ditentukan
    /// </summary>
    public GameObject SpawnGhost(Vector3 position, Quaternion rotation)
    {
        if (currentGhostCount >= maxGhostsInScene)
        {
            Debug.LogWarning("Max ghosts reached! Cannot spawn more.");
            OnMaxGhostsReached?.Invoke();
            return null;
        }

        GameObject ghost;

        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab, position, rotation);
        }
        else
        {
            ghost = GhostSetupHelper.CreateGhostFromScratch(position);
        }

        // Configure ghost
        ConfigureGhost(ghost);

        // Track ghost
        activeGhosts.Add(ghost);
        currentGhostCount = activeGhosts.Count;

        OnGhostSpawned?.Invoke(ghost);
        Debug.Log($"Ghost spawned at {position}. Total ghosts: {currentGhostCount}");

        return ghost;
    }

    /// <summary>
    /// Spawn ghost di spawn point random
    /// </summary>
    public GameObject SpawnGhostAtRandomPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned!");
            return null;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point at index {randomIndex} is null!");
            return null;
        }

        return SpawnGhost(spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    /// Spawn ghost di spawn point tertentu berdasarkan index
    /// </summary>
    public GameObject SpawnGhostAtPoint(int spawnPointIndex)
    {
        if (spawnPoints == null || spawnPointIndex >= spawnPoints.Length)
        {
            Debug.LogWarning($"Invalid spawn point index: {spawnPointIndex}");
            return null;
        }

        Transform spawnPoint = spawnPoints[spawnPointIndex];
        return SpawnGhost(spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    /// Configure ghost dengan default settings
    /// </summary>
    private void ConfigureGhost(GameObject ghostObj)
    {
        Ghost ghost = ghostObj.GetComponent<Ghost>();
        if (ghost != null)
        {
            ghost.SetActive(true);
        }
    }

    /// <summary>
    /// Destroy specific ghost
    /// </summary>
    public void DestroyGhost(GameObject ghost)
    {
        if (ghost != null && activeGhosts.Contains(ghost))
        {
            activeGhosts.Remove(ghost);
            currentGhostCount = activeGhosts.Count;
            OnGhostDestroyed?.Invoke(ghost);
            Destroy(ghost);
            Debug.Log($"Ghost destroyed. Remaining ghosts: {currentGhostCount}");
        }
    }

    /// <summary>
    /// Destroy all ghosts
    /// </summary>
    public void DestroyAllGhosts()
    {
        foreach (GameObject ghost in activeGhosts)
        {
            if (ghost != null)
            {
                Destroy(ghost);
            }
        }

        activeGhosts.Clear();
        currentGhostCount = 0;
        Debug.Log("All ghosts destroyed");
    }

    /// <summary>
    /// Activate/Deactivate all ghosts
    /// </summary>
    public void SetAllGhostsActive(bool active)
    {
        foreach (GameObject ghostObj in activeGhosts)
        {
            if (ghostObj != null)
            {
                Ghost ghost = ghostObj.GetComponent<Ghost>();
                if (ghost != null)
                {
                    ghost.SetActive(active);
                }
            }
        }
        Debug.Log($"Set all ghosts active: {active}");
    }

    /// <summary>
    /// Get ghost terdekat dari posisi tertentu
    /// </summary>
    public GameObject GetNearestGhost(Vector3 position)
    {
        GameObject nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject ghost in activeGhosts)
        {
            if (ghost != null)
            {
                float distance = Vector3.Distance(position, ghost.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = ghost;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// Get all ghosts dalam radius tertentu dari posisi
    /// </summary>
    public List<GameObject> GetGhostsInRadius(Vector3 position, float radius)
    {
        List<GameObject> ghostsInRadius = new List<GameObject>();

        foreach (GameObject ghost in activeGhosts)
        {
            if (ghost != null)
            {
                float distance = Vector3.Distance(position, ghost.transform.position);
                if (distance <= radius)
                {
                    ghostsInRadius.Add(ghost);
                }
            }
        }

        return ghostsInRadius;
    }

    // Context menu untuk testing di editor
    [ContextMenu("Spawn Test Ghost")]
    private void SpawnTestGhost()
    {
        SpawnGhostAtRandomPoint();
    }

    [ContextMenu("Destroy All Ghosts")]
    private void DestroyAllGhostsMenu()
    {
        DestroyAllGhosts();
    }

    // Gizmos untuk visualisasi spawn points
    private void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 2f);
                }
            }
        }
    }
}
