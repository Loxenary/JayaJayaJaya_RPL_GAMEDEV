using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents a climbable ladder zone.
/// When player enters the trigger zone, they can climb up/down using vertical movement.
/// Player horizontal movement is restricted while climbing.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LadderZone : MonoBehaviour
{
    [Header("Ladder Settings")]
    [Tooltip("Speed of climbing (units per second)")]
    [SerializeField] private float climbSpeed = 3f;

    [Tooltip("Top exit point of the ladder")]
    [SerializeField] private Transform topExitPoint;

    [Tooltip("Bottom exit point of the ladder")]
    [SerializeField] private Transform bottomExitPoint;

    [Tooltip("Lock player X/Z position to ladder center while climbing")]
    [SerializeField] private bool lockToLadderCenter = true;

    [Header("Events")]
    public UnityEvent OnPlayerEnterLadder;
    public UnityEvent OnPlayerExitLadder;

#if UNITY_EDITOR
    [Header("Debug")]
    [ReadOnly]
    [SerializeField] private bool _isPlayerOnLadder => isPlayerOnLadder;
#endif

    private bool isPlayerOnLadder = false;
    private Transform playerTransform;
    private CharacterController playerCharacterController;
    private PlayerInputHandler playerInputHandler;

    // Store ladder bounds for climbing constraints
    private float ladderMinY;
    private float ladderMaxY;
    private Vector3 ladderCenterXZ;

    // Static flag to track if any ladder is being climbed
    public static bool IsAnyLadderActive { get; private set; } = false;
    public static LadderZone CurrentLadder { get; private set; } = null;

    private void Start()
    {
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Calculate ladder Y bounds and center
        CalculateLadderBounds();
    }

    private void CalculateLadderBounds()
    {
        Collider col = GetComponent<Collider>();

        if (bottomExitPoint != null && topExitPoint != null)
        {
            ladderMinY = bottomExitPoint.position.y;
            ladderMaxY = topExitPoint.position.y;
        }
        else if (col != null)
        {
            ladderMinY = col.bounds.min.y;
            ladderMaxY = col.bounds.max.y;
        }

        // Store ladder center XZ position
        if (col != null)
        {
            ladderCenterXZ = new Vector3(col.bounds.center.x, 0, col.bounds.center.z);
        }
        else
        {
            ladderCenterXZ = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }

    private void Update()
    {
        if (!isPlayerOnLadder || playerInputHandler == null || playerCharacterController == null)
            return;

        HandleClimbing();
    }

    private void HandleClimbing()
    {
        // Get vertical input (W/S or Up/Down)
        Vector2 moveInput = playerInputHandler.MoveInput;
        float verticalInput = moveInput.y;

        // Keep player locked to ladder center XZ
        if (lockToLadderCenter)
        {
            Vector3 lockedPosition = new Vector3(ladderCenterXZ.x, playerTransform.position.y, ladderCenterXZ.z);
            if (Vector3.Distance(new Vector3(playerTransform.position.x, 0, playerTransform.position.z), 
                                 new Vector3(ladderCenterXZ.x, 0, ladderCenterXZ.z)) > 0.01f)
            {
                playerCharacterController.enabled = false;
                playerTransform.position = lockedPosition;
                playerCharacterController.enabled = true;
            }
        }

        if (Mathf.Abs(verticalInput) < 0.1f)
            return;

        // Calculate climb movement
        float climbAmount = verticalInput * climbSpeed * Time.deltaTime;
        float nextY = playerTransform.position.y + climbAmount;

        // Exit at top
        if (nextY >= ladderMaxY && verticalInput > 0)
        {
            ExitLadderAtTop();
            return;
        }

        // Exit at bottom
        if (nextY <= ladderMinY && verticalInput < 0)
        {
            ExitLadderAtBottom();
            return;
        }

        // Move player along ladder (vertical only)
        Vector3 climbMovement = new Vector3(0f, climbAmount, 0f);
        playerCharacterController.Move(climbMovement);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Don't enter if already on another ladder
        if (IsAnyLadderActive && CurrentLadder != this)
            return;

        // Get player components
        playerTransform = other.transform;
        playerCharacterController = other.GetComponent<CharacterController>();
        playerInputHandler = other.GetComponent<PlayerInputHandler>();

        if (playerCharacterController == null || playerInputHandler == null)
        {
            Debug.LogWarning("[LadderZone] Player missing required components!");
            return;
        }

        EnterLadder();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isPlayerOnLadder)
        {
            ExitLadder();
        }
    }

    private void EnterLadder()
    {
        if (isPlayerOnLadder) return;

        isPlayerOnLadder = true;
        IsAnyLadderActive = true;
        CurrentLadder = this;

        // Notify listeners
        OnPlayerEnterLadder?.Invoke();

        // Publish event for other systems
        EventBus.Publish(new LadderStateChangedEvent { isClimbing = true, ladder = this });

        Debug.Log("[LadderZone] Player entered ladder");
    }

    private void ExitLadder()
    {
        if (!isPlayerOnLadder) return;

        isPlayerOnLadder = false;
        IsAnyLadderActive = false;
        CurrentLadder = null;

        // Notify listeners
        OnPlayerExitLadder?.Invoke();

        // Publish event for other systems
        EventBus.Publish(new LadderStateChangedEvent { isClimbing = false, ladder = this });

        // Clear references
        playerTransform = null;
        playerCharacterController = null;
        playerInputHandler = null;

        Debug.Log("[LadderZone] Player exited ladder");
    }

    private void ExitLadderAtTop()
    {
        if (topExitPoint != null)
        {
            // Teleport player to top exit point
            playerCharacterController.enabled = false;
            playerTransform.position = topExitPoint.position;
            playerCharacterController.enabled = true;
        }

        ExitLadder();
        Debug.Log("[LadderZone] Player exited at top");
    }

    private void ExitLadderAtBottom()
    {
        if (bottomExitPoint != null)
        {
            // Teleport player to bottom exit point
            playerCharacterController.enabled = false;
            playerTransform.position = bottomExitPoint.position;
            playerCharacterController.enabled = true;
        }

        ExitLadder();
        Debug.Log("[LadderZone] Player exited at bottom");
    }

    /// <summary>
    /// Force player off ladder (for external use)
    /// </summary>
    public void ForceExitLadder()
    {
        ExitLadder();
    }

    /// <summary>
    /// Check if player is currently on this ladder
    /// </summary>
    public bool IsPlayerClimbing()
    {
        return isPlayerOnLadder;
    }

    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        // Draw ladder zone
        Gizmos.color = Color.green;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        // Draw exit points
        if (topExitPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(topExitPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, topExitPoint.position);
        }

        if (bottomExitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(bottomExitPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, bottomExitPoint.position);
        }
    }
}

/// <summary>
/// Event published when player enters or exits a ladder
/// </summary>
public struct LadderStateChangedEvent
{
    public bool isClimbing;
    public LadderZone ladder;
}
