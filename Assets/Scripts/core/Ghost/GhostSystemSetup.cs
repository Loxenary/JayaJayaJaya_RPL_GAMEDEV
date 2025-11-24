using UnityEngine;

/// <summary>
/// Script untuk setup ghost system secara otomatis di scene
/// Attach ke empty GameObject dan jalankan untuk quick setup
/// </summary>
public class GhostSystemSetup : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool autoSetupOnStart = false;
    [SerializeField] private bool createPatrolPoints = true;
    [SerializeField] private int numberOfPatrolPoints = 4;
    [SerializeField] private float patrolRadius = 10f;

    [Header("Ghost Settings")]
    [SerializeField] private Vector3 ghostSpawnPosition = Vector3.zero;
    [SerializeField] private bool addVisualPlaceholder = true;
    [SerializeField] private bool addDebugController = true;

    [Header("References (Optional)")]
    [SerializeField] private GameObject playerObject;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGhostSystem();
        }
    }

    [ContextMenu("Setup Ghost System")]
    public void SetupGhostSystem()
    {
        Debug.Log("[GhostSystemSetup] Starting ghost system setup...");

        // 1. Find or validate player
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject == null)
        {
            Debug.LogError("[GhostSystemSetup] Player not found! Make sure player is tagged as 'Player'");
            return;
        }

        // 2. Setup player components
        SetupPlayerComponents();

        // 3. Create ghost
        GameObject ghost = CreateGhost();

        // 4. Create patrol points if needed
        if (createPatrolPoints)
        {
            Transform[] patrolPoints = CreatePatrolPoints(ghost.transform);
            AssignPatrolPointsToGhost(ghost, patrolPoints);
        }

        Debug.Log("[GhostSystemSetup] Ghost system setup complete!");
    }

    private void SetupPlayerComponents()
    {
        Debug.Log("[GhostSystemSetup] Setting up player components...");

        // Add PlayerSanity if not exists
        PlayerSanity sanity = playerObject.GetComponent<PlayerSanity>();
        if (sanity == null)
        {
            sanity = playerObject.AddComponent<PlayerSanity>();
            Debug.Log("[GhostSystemSetup] Added PlayerSanity component");
        }

        // Add PlayerHealth if not exists
        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = playerObject.AddComponent<PlayerHealth>();
            Debug.Log("[GhostSystemSetup] Added PlayerHealth component");
        }

        // Check for PlayerRespawn
        PlayerRespawn respawn = playerObject.GetComponent<PlayerRespawn>();
        if (respawn == null)
        {
            Debug.LogWarning("[GhostSystemSetup] PlayerRespawn not found. Player death might not work properly.");
        }
    }

    private GameObject CreateGhost()
    {
        Debug.Log("[GhostSystemSetup] Creating ghost...");

        // Create ghost GameObject
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.position = ghostSpawnPosition;

        // Add GhostAttack first (karena GhostAI requires it)
        GhostAttack ghostAttack = ghost.AddComponent<GhostAttack>();

        // Add GhostAI
        GhostAI ghostAI = ghost.AddComponent<GhostAI>();

        // Add visual placeholder if needed
        if (addVisualPlaceholder)
        {
            ghost.AddComponent<GhostVisualPlaceholder>();
            Debug.Log("[GhostSystemSetup] Added visual placeholder");
        }

        // Add debug controller if needed
        if (addDebugController)
        {
            ghost.AddComponent<GhostDebugController>();
            Debug.Log("[GhostSystemSetup] Added debug controller");
        }

        Debug.Log($"[GhostSystemSetup] Ghost created at {ghostSpawnPosition}");
        return ghost;
    }

    private Transform[] CreatePatrolPoints(Transform ghostTransform)
    {
        Debug.Log($"[GhostSystemSetup] Creating {numberOfPatrolPoints} patrol points...");

        GameObject patrolParent = new GameObject("PatrolPoints");
        patrolParent.transform.position = ghostTransform.position;

        Transform[] points = new Transform[numberOfPatrolPoints];

        for (int i = 0; i < numberOfPatrolPoints; i++)
        {
            GameObject point = new GameObject($"PatrolPoint_{i + 1}");
            point.transform.SetParent(patrolParent.transform);

            // Arrange in circle
            float angle = (360f / numberOfPatrolPoints) * i;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * patrolRadius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * patrolRadius;

            point.transform.position = ghostTransform.position + new Vector3(x, 0, z);

            // Add gizmo visual
            AddPatrolPointGizmo(point);

            points[i] = point.transform;
        }

        Debug.Log($"[GhostSystemSetup] Created {numberOfPatrolPoints} patrol points in circular pattern");
        return points;
    }

    private void AddPatrolPointGizmo(GameObject patrolPoint)
    {
        // Add a component to visualize patrol points in scene
        var gizmo = patrolPoint.AddComponent<PatrolPointGizmo>();
    }

    private void AssignPatrolPointsToGhost(GameObject ghost, Transform[] patrolPoints)
    {
        GhostAI ghostAI = ghost.GetComponent<GhostAI>();
        if (ghostAI != null)
        {
            // Using reflection to set private field (karena patrol points adalah SerializeField)
            var field = typeof(GhostAI).GetField("patrolPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(ghostAI, patrolPoints);
                Debug.Log($"[GhostSystemSetup] Assigned {patrolPoints.Length} patrol points to ghost");
            }
        }
    }

    [ContextMenu("Clear Ghost System")]
    public void ClearGhostSystem()
    {
        // Clean up created objects
        GameObject ghost = GameObject.Find("Ghost");
        if (ghost != null)
        {
            DestroyImmediate(ghost);
            Debug.Log("[GhostSystemSetup] Removed ghost");
        }

        GameObject patrolPoints = GameObject.Find("PatrolPoints");
        if (patrolPoints != null)
        {
            DestroyImmediate(patrolPoints);
            Debug.Log("[GhostSystemSetup] Removed patrol points");
        }
    }
}

/// <summary>
/// Simple component to draw gizmos for patrol points
/// </summary>
public class PatrolPointGizmo : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}
