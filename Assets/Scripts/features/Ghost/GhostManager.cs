using EnemyAI;
using UnityEngine;
using UnityEngine.Events;

public enum GhostSpawnType
{
  Trigger,    // Spawn via external script call
  AutoSpawn   // Auto spawn on Start (testing mode)
}

/// <summary>
/// Simple Ghost Manager - handles spawning and controlling a single ghost.
/// Spawn position is draggable via the SpawnPoint transform in editor.
/// Spawning can be triggered externally or auto-spawn for testing.
/// </summary>
public class GhostManager : MonoBehaviour
{
  [Header("Spawn Settings")]
  [Tooltip("Trigger = spawn via script call, AutoSpawn = spawn on Start (testing)")]
  [SerializeField] private GhostSpawnType spawnType = GhostSpawnType.Trigger;

  [Header("Ghost Prefab")]
  [Tooltip("The ghost prefab to spawn. If null, creates a basic ghost.")]
  [SerializeField] private GameObject ghostPrefab;

  [Header("Spawn Point")]
  [Tooltip("Drag this transform in Scene view to set spawn position")]
  [SerializeField] private Transform spawnPoint;
  [SerializeField] private Transform patrolPoint;


  [Header("Ghost Configuration")]
  [SerializeField] private bool activateOnSpawn = true;

  [Header("Events")]
  public UnityEvent OnGhostSpawned;
  public UnityEvent OnGhostDestroyed;

#if UNITY_EDITOR
  [Header("Debug Info")]
  [ReadOnly]
  [SerializeField] private GameObject ghostDetected => currentGhost;
  [ReadOnly]
  [SerializeField] private bool _hasGhost => hasGhost;
#endif
  private GameObject currentGhost;

  private bool hasGhost = false;

  /// <summary>
  /// Check if ghost exists in scene
  /// </summary>
  public bool HasGhost => currentGhost != null;

  public GhostSpawnType SpawnType => spawnType;

  /// <summary>
  /// Get current ghost instance
  /// </summary>
  public GameObject CurrentGhost => currentGhost;

  /// <summary>
  /// Get spawn position (from SpawnPoint transform or manager position)
  /// </summary>
  public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

  private void Awake()
  {
    // Create spawn point if not assigned
    if (spawnPoint == null)
    {
      GameObject spawnPointObj = new GameObject("GhostSpawnPoint");
      spawnPointObj.transform.SetParent(transform);
      spawnPointObj.transform.localPosition = Vector3.zero;
      spawnPoint = spawnPointObj.transform;
    }
  }

  private void OnEnable()
  {
    EventBus.Subscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
  }

  private void OnDisable()
  {
    EventBus.Unsubscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
  }

  private void OnFirstPuzzleEvent(FirstPuzzleEvent eventData)
  {
    var ghost = SpawnGhost();
    if (ghost.TryGetComponent<BaseEnemyAI>(out var enemyAI))
    {
      enemyAI.Initialize(patrolPoint);
    }
  }

  private void Start()
  {
    // Auto spawn if in testing mode
    if (spawnType == GhostSpawnType.AutoSpawn)
    {
      SpawnGhost();
      Debug.Log("[GhostManager] Auto-spawned ghost for testing mode");
    }
  }

  private void Update()
  {
    // Update debug info
    hasGhost = currentGhost != null;
  }

  /// <summary>
  /// Spawn the ghost at the designated spawn point.
  /// Call this from other scripts to trigger spawn.
  /// </summary>
  public GameObject SpawnGhost()
  {
    // Only allow one ghost at a time
    if (currentGhost != null)
    {
      Debug.LogWarning("[GhostManager] Ghost already exists! Destroy it first or use RespawnGhost().");
      return currentGhost;
    }

    return CreateGhost(SpawnPosition);
  }

  /// <summary>
  /// Spawn ghost at a custom position (overrides spawn point)
  /// </summary>
  public GameObject SpawnGhostAt(Vector3 position)
  {
    if (currentGhost != null)
    {
      Debug.LogWarning("[GhostManager] Ghost already exists!");
      return currentGhost;
    }

    return CreateGhost(position);
  }

  /// <summary>
  /// Destroy current ghost and spawn a new one
  /// </summary>
  public GameObject RespawnGhost()
  {
    DestroyGhost();
    return SpawnGhost();
  }

  /// <summary>
  /// Destroy the current ghost
  /// </summary>
  public void DestroyGhost()
  {
    if (currentGhost != null)
    {
      Destroy(currentGhost);
      currentGhost = null;
      hasGhost = false;
      OnGhostDestroyed?.Invoke();
    }
  }

  /// <summary>
  /// Teleport ghost to spawn point
  /// </summary>
  public void TeleportGhostToSpawnPoint()
  {
    if (currentGhost != null)
    {
      currentGhost.transform.position = SpawnPosition;
    }
  }

  /// <summary>
  /// Set ghost active state
  /// </summary>
  public void SetGhostActive(bool active)
  {
    if (currentGhost != null)
    {
      Ghost ghost = currentGhost.GetComponent<Ghost>();
      if (ghost != null)
      {
        ghost.SetActive(active);
      }
    }
  }

  /// <summary>
  /// Internal method to create ghost
  /// </summary>
  private GameObject CreateGhost(Vector3 position)
  {
    GameObject ghost;

    if (ghostPrefab != null)
    {
      ghost = Instantiate(ghostPrefab, position, Quaternion.identity);
    }
    else
    {
      ghost = CreateBasicGhost(position);
    }

    // Move to same scene as manager
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ghost, gameObject.scene);

    // Configure ghost
    Ghost ghostComponent = ghost.GetComponent<Ghost>();
    if (ghostComponent != null && activateOnSpawn)
    {
      ghostComponent.SetActive(true);
    }

    currentGhost = ghost;
    hasGhost = true;
    OnGhostSpawned?.Invoke();

    return ghost;
  }

  /// <summary>
  /// Create a basic ghost when no prefab is assigned
  /// </summary>
  private GameObject CreateBasicGhost(Vector3 position)
  {
    GameObject ghost = new GameObject("Ghost");
    ghost.transform.position = position;
    ghost.layer = LayerMask.NameToLayer("Default");

    // Visual (simple cube)
    GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
    visual.name = "Visual";
    visual.transform.SetParent(ghost.transform);
    visual.transform.localPosition = Vector3.zero;
    visual.transform.localScale = new Vector3(1f, 2f, 1f);

    // Remove collider from visual
    Collider visualCollider = visual.GetComponent<Collider>();
    if (visualCollider != null) DestroyImmediate(visualCollider);

    // Add NON-TRIGGER collider for physics (gravity, collision)
    BoxCollider physicsCollider = ghost.AddComponent<BoxCollider>();
    physicsCollider.isTrigger = false;
    physicsCollider.size = new Vector3(1f, 2f, 1f);
    physicsCollider.center = Vector3.zero;

    // Add TRIGGER collider for damage detection
    BoxCollider triggerCollider = ghost.AddComponent<BoxCollider>();
    triggerCollider.isTrigger = true;
    triggerCollider.size = new Vector3(1.5f, 2f, 1.5f);
    triggerCollider.center = Vector3.zero;

    // Add Rigidbody for physics
    Rigidbody rb = ghost.AddComponent<Rigidbody>();
    rb.mass = 1f;
    rb.useGravity = true;
    rb.isKinematic = false;
    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

    // Add Ghost component
    ghost.AddComponent<Ghost>();

    return ghost;
  }

  // ================== EDITOR VISUALIZATION ==================

  private void OnDrawGizmos()
  {
    // Draw spawn point
    Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

    // Ghost shape outline
    Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f); // Purple
    Gizmos.DrawWireCube(spawnPos + Vector3.up, new Vector3(1f, 2f, 1f));

    // Spawn point indicator
    Gizmos.color = Color.magenta;
    Gizmos.DrawWireSphere(spawnPos, 0.3f);

    // Label
#if UNITY_EDITOR
    UnityEditor.Handles.color = Color.magenta;
    UnityEditor.Handles.Label(spawnPos + Vector3.up * 2.5f, "Ghost Spawn");
#endif
  }

  private void OnDrawGizmosSelected()
  {
    Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

    // More visible when selected
    Gizmos.color = Color.red;
    Gizmos.DrawCube(spawnPos, Vector3.one * 0.2f);

    // Direction indicator
    Gizmos.color = Color.blue;
    Gizmos.DrawRay(spawnPos, Vector3.forward * 2f);
  }
}
