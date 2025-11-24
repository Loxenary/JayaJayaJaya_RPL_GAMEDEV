using System;
using UnityEngine;

/// <summary>
/// AI controller untuk ghost dengan state machine sederhana
/// Ghost akan lebih agresif (bergerak lebih cepat) ketika sanity player rendah
/// </summary>
[RequireComponent(typeof(GhostAttack))]
public class GhostAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerSanity playerSanity;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float loseTargetRange = 25f;
    [SerializeField] private LayerMask obstructionMask;
    
    [Header("Movement - Base Speed")]
    [SerializeField] private float idleSpeed = 1f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    
    [Header("Sanity-Based Speed Multipliers")]
    [Tooltip("Multiplier ketika sanity player tinggi")]
    [SerializeField] private float highSanitySpeedMultiplier = 1f;
    [Tooltip("Multiplier ketika sanity player medium")]
    [SerializeField] private float mediumSanitySpeedMultiplier = 1.3f;
    [Tooltip("Multiplier ketika sanity player rendah")]
    [SerializeField] private float lowSanitySpeedMultiplier = 1.6f;
    [Tooltip("Multiplier ketika sanity player critical")]
    [SerializeField] private float criticalSanitySpeedMultiplier = 2f;
    
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointReachDistance = 1f;
    [SerializeField] private float patrolWaitTime = 2f;
    
    [Header("Behavior")]
    [SerializeField] private bool startActive = true;
    [SerializeField] private float stateCheckInterval = 0.2f;
    
    // State machine
    public enum GhostState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Stunned
    }
    
    private GhostState currentState = GhostState.Idle;
    private GhostAttack ghostAttack;
    private int currentPatrolIndex = 0;
    private float lastStateCheckTime;
    private float patrolWaitTimer;
    private bool isActive = true;
    private float currentSpeed;
    private Vector3 lastKnownPlayerPosition;
    
    // Events
    public event Action<GhostState> OnStateChanged;
    public event Action OnPlayerDetected;
    public event Action OnPlayerLost;
    
    // Properties
    public GhostState CurrentState => currentState;
    public float CurrentSpeed => currentSpeed;
    public bool IsActive => isActive;
    
    private void Awake()
    {
        ghostAttack = GetComponent<GhostAttack>();
        isActive = startActive;
    }
    
    private void Start()
    {
        // Auto-find player jika tidak diset
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Auto-find player sanity
        if (playerSanity == null && player != null)
        {
            playerSanity = player.GetComponent<PlayerSanity>();
        }
        
        if (isActive)
        {
            ChangeState(patrolPoints != null && patrolPoints.Length > 0 ? GhostState.Patrol : GhostState.Idle);
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Periodic state check untuk performa
        if (Time.time - lastStateCheckTime >= stateCheckInterval)
        {
            CheckStateTransitions();
            lastStateCheckTime = Time.time;
        }
        
        // Execute current state behavior
        ExecuteState();
    }
    
    private void CheckStateTransitions()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();
        
        switch (currentState)
        {
            case GhostState.Idle:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(GhostState.Chase);
                    OnPlayerDetected?.Invoke();
                }
                else if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    ChangeState(GhostState.Patrol);
                }
                break;
                
            case GhostState.Patrol:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(GhostState.Chase);
                    OnPlayerDetected?.Invoke();
                }
                break;
                
            case GhostState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(GhostState.Attack);
                }
                else if (distanceToPlayer > loseTargetRange || !canSeePlayer)
                {
                    ChangeState(GhostState.Patrol);
                    OnPlayerLost?.Invoke();
                }
                break;
                
            case GhostState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(GhostState.Chase);
                }
                break;
        }
    }
    
    private void ExecuteState()
    {
        switch (currentState)
        {
            case GhostState.Idle:
                ExecuteIdleState();
                break;
                
            case GhostState.Patrol:
                ExecutePatrolState();
                break;
                
            case GhostState.Chase:
                ExecuteChaseState();
                break;
                
            case GhostState.Attack:
                ExecuteAttackState();
                break;
                
            case GhostState.Stunned:
                // Do nothing, stunned
                break;
        }
    }
    
    private void ExecuteIdleState()
    {
        currentSpeed = idleSpeed * GetSanitySpeedMultiplier();
        // Idle behavior - bisa tambahkan random movement atau animation
    }
    
    private void ExecutePatrolState()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        
        currentSpeed = patrolSpeed * GetSanitySpeedMultiplier();
        
        // Wait timer
        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            return;
        }
        
        Transform targetWaypoint = patrolPoints[currentPatrolIndex];
        if (targetWaypoint == null) return;
        
        // Move towards waypoint
        MoveTowards(targetWaypoint.position);
        
        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) <= waypointReachDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            patrolWaitTimer = patrolWaitTime;
        }
    }
    
    private void ExecuteChaseState()
    {
        if (player == null) return;
        
        currentSpeed = chaseSpeed * GetSanitySpeedMultiplier();
        lastKnownPlayerPosition = player.position;
        
        // Move towards player
        MoveTowards(player.position);
        
        // Look at player
        LookAtTarget(player.position);
    }
    
    private void ExecuteAttackState()
    {
        if (player == null) return;
        
        // Stop moving, face player
        LookAtTarget(player.position);
        
        // Trigger attack jika ready
        if (ghostAttack != null && ghostAttack.CanAttack())
        {
            ghostAttack.Attack(player.gameObject);
        }
    }
    
    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
    
    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep on horizontal plane
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    private bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Check if in range
        if (distanceToPlayer > detectionRange) return false;
        
        // Raycast untuk check obstruction
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, distanceToPlayer, obstructionMask))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Get speed multiplier berdasarkan sanity level player
    /// Ghost lebih cepat ketika sanity player rendah
    /// </summary>
    private float GetSanitySpeedMultiplier()
    {
        if (playerSanity == null) return highSanitySpeedMultiplier;
        
        switch (playerSanity.CurrentSanityLevel)
        {
            case PlayerSanity.SanityLevel.High:
                return highSanitySpeedMultiplier;
                
            case PlayerSanity.SanityLevel.Medium:
                return mediumSanitySpeedMultiplier;
                
            case PlayerSanity.SanityLevel.Low:
                return lowSanitySpeedMultiplier;
                
            case PlayerSanity.SanityLevel.Critical:
                return criticalSanitySpeedMultiplier;
                
            default:
                return highSanitySpeedMultiplier;
        }
    }
    
    private void ChangeState(GhostState newState)
    {
        if (currentState == newState) return;
        
        GhostState previousState = currentState;
        currentState = newState;
        
        Debug.Log($"[GhostAI] State changed: {previousState} -> {newState}");
        OnStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// Activate or deactivate ghost AI
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            ChangeState(GhostState.Idle);
        }
    }
    
    /// <summary>
    /// Stun ghost untuk durasi tertentu
    /// </summary>
    public void Stun(float duration)
    {
        if (!isActive) return;
        
        ChangeState(GhostState.Stunned);
        Invoke(nameof(RecoverFromStun), duration);
    }
    
    private void RecoverFromStun()
    {
        ChangeState(GhostState.Patrol);
    }
    
    /// <summary>
    /// Teleport ghost ke posisi tertentu
    /// </summary>
    public void Teleport(Vector3 position)
    {
        transform.position = position;
    }
    
    // Gizmos untuk debug
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Lose target range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        
        // Patrol points
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.5f);
                }
            }
            
            // Draw patrol path
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null && patrolPoints[(i + 1) % patrolPoints.Length] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[(i + 1) % patrolPoints.Length].position);
                }
            }
        }
        
        // Line to player if detected
        if (player != null && currentState == GhostState.Chase)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
