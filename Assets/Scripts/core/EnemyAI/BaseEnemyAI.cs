using UnityEngine;
using System.Collections;
using MonsterLove.StateMachine;
using Pathfinding;

namespace EnemyAI
{
    /// <summary>
    /// Base Enemy AI controller that combines MonsterLove FSM with Astar Pathfinding.
    /// This is designed to be scalable and extended for different enemy types.
    ///
    /// States:
    /// - Idle: Standing still, scanning for targets with cone vision
    /// - Patrol: Moving between patrol points, scanning for targets
    /// - Seen: Target spotted! Moving slowly towards target before full chase
    /// - Chase: Actively pursuing detected target
    /// - Attack: Attacking the target
    /// - Flee: Retreating after attack, waiting for cooldown before re-engaging
    /// </summary>
    [RequireComponent(typeof(AIPath))]
    [RequireComponent(typeof(EnemyDetection))]
    public class BaseEnemyAI : MonoBehaviour
    {
        public enum EnemyState
        {
            Idle,
            Patrol,
            Seen,
            Chase,
            Attack,
            Flee,
        }

        [Header("Configuration")]
        [SerializeField] protected EnemyStats stats;

        [Header("Angry System Integration (Optional)")]
        [Tooltip("Reference to the global EnemyAngrySystem. If set, enemy will use dynamic stats based on anger level.")]
        [SerializeField] protected EnemyAngrySystem angrySystem;
        [Tooltip("If true, enemy stats will automatically update when anger level changes")]
        [SerializeField] protected bool useDynamicAngryStats = false;

        [Header("Patrol Points (Optional)")]
        [SerializeField] protected Transform[] patrolPoints;
        [SerializeField] protected bool loopPatrol = true;

        [Header("Behavior Settings")]
        [SerializeField][Range(0f, 1f)] protected float patrolChance = 0.5f;
        [SerializeField] protected float idleWaitTimeMin = 2f;
        [SerializeField] protected float idleWaitTimeMax = 5f;
        [SerializeField] protected float chaseRadius = 15f;
        [SerializeField] protected float visionLostCountdown = 2f;
        [SerializeField] protected float seenStateTimeout = 5f;

        [Header("Seen State - Slow Approach")]
        [Tooltip("Duration to move slowly when first seeing target")]
        [SerializeField] protected float seenSlowMoveDuration = 1.5f;
        [Tooltip("Speed multiplier during slow approach (0.5 = 50% speed)")]
        [SerializeField][Range(0.1f, 1f)] protected float seenSlowSpeedMultiplier = 0.5f;

        [Header("Flee State")]
        [Tooltip("Time to stay in flee state after attack (usually matches attack cooldown)")]
        [SerializeField] protected float fleeDuration = 1.5f;

        [Header("Debug")]
        [SerializeField] protected bool showDebugLogs = false;
        [SerializeField] protected bool showStateInInspector = true;

        protected StateMachine<EnemyState, StateDriverUnity> fsm;
        protected AIPath aiPath;
        protected EnemyDetection detection;

        protected float currentHealth;
        protected Transform currentTarget;
        protected int currentPatrolIndex = 0;
        protected float lastAttackTime;
        protected float targetLostTime;
        protected Vector3 lastKnownTargetPosition;
        protected float idleWaitTimer;
        protected float currentIdleWaitTime;
        protected float seenStateTimer;
        protected float fleeTimer;
        protected bool seenSlowPhaseComplete;

        public EnemyState CurrentState => fsm != null ? fsm.State : EnemyState.Idle;
        public float HealthPercentage => currentHealth / stats.maxHealth;
        public bool IsAlive => currentHealth > 0;
    
        [Header("CURRENT STATE (Read Only)")]
        [SerializeField][ReadOnly] private string _currentStateDisplay = "Not Started";
        [SerializeField][ReadOnly] private string _targetInfo = "No Target";
        [SerializeField][ReadOnly] private float _visionTimer = 0f;
        [SerializeField][ReadOnly] private float _fleeTimer = 0f;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            aiPath = GetComponent<AIPath>();
            detection = GetComponent<EnemyDetection>();

            if (stats == null)
            {
                Debug.LogError($"EnemyStats not assigned on {gameObject.name}!");
                enabled = false;
                return;
            }

            InitializeComponents();
            InitializeFSM();
            InitializeAngrySystem();
        }

        protected virtual void Start()
        {
            currentHealth = stats.maxHealth;
            ChooseRandomBehavior();
        }

        protected virtual void OnEnable()
        {
            if (useDynamicAngryStats && angrySystem != null)
            {
                EnemyAngrySystem.OnGlobalAngryLevelChanged += OnAngryLevelChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (useDynamicAngryStats && angrySystem != null)
            {
                EnemyAngrySystem.OnGlobalAngryLevelChanged -= OnAngryLevelChanged;
            }
        }

        protected virtual void ChooseRandomBehavior()
        {
            if (patrolPoints != null && patrolPoints.Length > 0 && Random.value <= patrolChance)
            {
                fsm.ChangeState(EnemyState.Patrol);
            }
            else
            {
                fsm.ChangeState(EnemyState.Idle);
            }
        }

        protected virtual void Update()
        {
            if (fsm != null)
            {
                fsm.Driver.Update.Invoke();
                UpdateDebugDisplay();
            }
        }

        protected virtual void UpdateDebugDisplay()
        {
            if (!showStateInInspector) return;

            _currentStateDisplay = $">>> {CurrentState.ToString().ToUpper()} <<<";

            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);
                _targetInfo = $"Target: {currentTarget.name} ({distance:F1}m)";
            }
            else
            {
                _targetInfo = "No Target";
            }

            _visionTimer = targetLostTime;
            _fleeTimer = fleeTimer;
        }

        #endregion

        #region Initialization

        protected virtual void InitializeComponents()
        {
            aiPath.maxSpeed = stats.maxSpeed;
            aiPath.maxAcceleration = stats.acceleration;
            aiPath.rotationSpeed = stats.rotationSpeed;
            aiPath.canMove = true;

            detection.Initialize(stats.detectionRange, stats.detectionAngle, stats.detectionLayer);
        }

        protected virtual void InitializeFSM()
        {
            fsm = new StateMachine<EnemyState, StateDriverUnity>(this);
        }

        protected virtual void InitializeAngrySystem()
        {
            if (angrySystem == null)
            {
                // Try to find angry system in scene if not assigned
                angrySystem = FindAnyObjectByType<EnemyAngrySystem>();
            }

            if (useDynamicAngryStats && angrySystem != null)
            {
                Log($"Angry System integration enabled. Will use dynamic stats based on anger level.");
                // Apply initial stats from current angry level
                UpdateStatsFromAngryLevel(angrySystem.CurrentLevel);
            }
            else if (useDynamicAngryStats && angrySystem == null)
            {
                Debug.LogWarning($"[{gameObject.name}] useDynamicAngryStats is enabled but no EnemyAngrySystem found in scene!");
            }
        }

        protected virtual void OnAngryLevelChanged(EnemyLevel newLevel)
        {
            if (!useDynamicAngryStats || angrySystem == null) return;

            Log($"Angry level changed to {newLevel}. Updating enemy stats...");
            UpdateStatsFromAngryLevel(newLevel);
        }

        protected virtual void UpdateStatsFromAngryLevel(EnemyLevel level)
        {
            if (angrySystem == null) return;

            EnemyStats newStats = angrySystem.GetStatsForLevel(level);

            if (newStats == null)
            {
                Debug.LogWarning($"[{gameObject.name}] No stats found for angry level {level}!");
                return;
            }

            // Update stats reference
            stats = newStats;

            // Update AI components with new stats
            aiPath.maxSpeed = stats.maxSpeed;
            aiPath.maxAcceleration = stats.acceleration;
            aiPath.rotationSpeed = stats.rotationSpeed;

            detection.Initialize(stats.detectionRange, stats.detectionAngle, stats.detectionLayer);

            Log($"Stats updated for level {level}: Speed={stats.maxSpeed}, DetectionRange={stats.detectionRange}, Damage={stats.attackDamage}");

            // Optional: Trigger visual effects on level change
            OnAngryLevelVisualUpdate(level);
        }

        /// <summary>
        /// Override this method to add visual effects when angry level changes
        /// (e.g., change color, add particle effects, play sound)
        /// </summary>
        protected virtual void OnAngryLevelVisualUpdate(EnemyLevel level)
        {
            // Override in derived classes to add visual feedback
        }

        #endregion

        #region State: Idle

        protected virtual void Idle_Enter()
        {
            Log("Entering Idle state");
            aiPath.canMove = false;
            currentTarget = null;
            currentIdleWaitTime = Random.Range(idleWaitTimeMin, idleWaitTimeMax);
            idleWaitTimer = 0f;
        }

        protected virtual void Idle_Update()
        {
            Transform detectedTarget = detection.ScanForTarget();
            if (detectedTarget != null)
            {
                currentTarget = detectedTarget;
                fsm.ChangeState(EnemyState.Seen);
                return;
            }

            idleWaitTimer += Time.deltaTime;
            if (idleWaitTimer >= currentIdleWaitTime)
            {
                ChooseRandomBehavior();
            }
        }

        protected virtual void Idle_Exit()
        {
            Log("Exiting Idle state");
        }

        #endregion

        #region State: Patrol

        protected virtual void Patrol_Enter()
        {
            Log("Entering Patrol state");
            aiPath.canMove = true;
            SetNextPatrolPoint();
        }

        protected virtual void Patrol_Update()
        {
            Transform detectedTarget = detection.ScanForTarget();
            if (detectedTarget != null)
            {
                currentTarget = detectedTarget;
                fsm.ChangeState(EnemyState.Seen);
                return;
            }

            if (aiPath.reachedEndOfPath)
            {
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    currentPatrolIndex++;
                    if (currentPatrolIndex >= patrolPoints.Length)
                    {
                        if (loopPatrol)
                        {
                            currentPatrolIndex = 0;
                            SetNextPatrolPoint();
                        }
                        else
                        {
                            ChooseRandomBehavior();
                            return;
                        }
                    }
                    else
                    {
                        SetNextPatrolPoint();
                    }
                }
            }
        }

        protected virtual void Patrol_Exit()
        {
            Log("Exiting Patrol state");
        }

        protected virtual void SetNextPatrolPoint()
        {
            if (patrolPoints != null && patrolPoints.Length > 0 && currentPatrolIndex < patrolPoints.Length)
            {
                aiPath.destination = patrolPoints[currentPatrolIndex].position;
                Log($"Moving to patrol point {currentPatrolIndex}");
            }
        }

        #endregion

        #region State: Seen

        protected virtual void Seen_Enter()
        {
            Log("Target SEEN! Moving slowly towards target...");
            aiPath.canMove = true;
            aiPath.maxSpeed = stats.maxSpeed * seenSlowSpeedMultiplier;
            targetLostTime = 0f;
            seenStateTimer = 0f;
            seenSlowPhaseComplete = false;
        }

        protected virtual void Seen_Update()
        {
            if (currentTarget == null)
            {
                ChooseRandomBehavior();
                return;
            }

            seenStateTimer += Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (!detection.HasLineOfSight(currentTarget))
            {
                targetLostTime += Time.deltaTime;
                if (targetLostTime >= visionLostCountdown)
                {
                    Log("Lost sight of target in SEEN state, returning to previous behavior");
                    currentTarget = null;
                    ChooseRandomBehavior();
                }
                return;
            }
            else
            {
                targetLostTime = 0f;
            }

            // Move slowly towards target during initial phase
            if (!seenSlowPhaseComplete)
            {
                aiPath.destination = currentTarget.position;
                LookAtTarget(currentTarget);

                if (seenStateTimer >= seenSlowMoveDuration)
                {
                    seenSlowPhaseComplete = true;
                    Log("Slow approach complete, evaluating chase...");
                }
            }

            // After slow phase, check if target is within chase radius
            if (seenSlowPhaseComplete && distanceToTarget <= chaseRadius)
            {
                Log("Target within chase radius, initiating full chase!");
                fsm.ChangeState(EnemyState.Chase);
                return;
            }

            if (seenStateTimer >= seenStateTimeout)
            {
                Log($"Target too far for too long ({distanceToTarget:F1}m > {chaseRadius}m), giving up");
                currentTarget = null;
                ChooseRandomBehavior();
                return;
            }

            if (!seenSlowPhaseComplete)
            {
                Log($"Moving slowly towards target... ({seenStateTimer:F1}s/{seenSlowMoveDuration}s)");
            }
            else
            {
                Log($"Target spotted but outside chase radius ({distanceToTarget:F1}m > {chaseRadius}m)");
            }
        }

        protected virtual void Seen_Exit()
        {
            Log("Exiting Seen state");
        }

        #endregion

        #region State: Chase

        protected virtual void Chase_Enter()
        {
            Log("Entering Chase state");
            aiPath.canMove = true;
            // Apply chase speed multiplier for faster pursuit
            aiPath.maxSpeed = stats.maxSpeed * stats.chaseSpeedMultiplier;
            targetLostTime = 0f;

            Log($"Chase speed: {aiPath.maxSpeed:F1} (base: {stats.maxSpeed}, multiplier: {stats.chaseSpeedMultiplier}x)");
        }

        protected virtual void Chase_Update()
        {
            if (currentTarget == null)
            {
                ChooseRandomBehavior();
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= stats.attackRange)
            {
                fsm.ChangeState(EnemyState.Attack);
                return;
            }

            if (!detection.HasLineOfSight(currentTarget))
            {
                targetLostTime += Time.deltaTime;
                aiPath.destination = lastKnownTargetPosition;

                if (targetLostTime >= visionLostCountdown)
                {
                    Log($"Lost vision of target for {visionLostCountdown} seconds, returning to previous behavior");
                    currentTarget = null;
                    ChooseRandomBehavior();
                }
                return;
            }
            else
            {
                lastKnownTargetPosition = currentTarget.position;
                aiPath.destination = currentTarget.position;
                targetLostTime = 0f;
            }

            if (distanceToTarget > chaseRadius * 1.5f)
            {
                Log("Target moved too far outside chase zone, giving up");
                currentTarget = null;
                ChooseRandomBehavior();
                return;
            }
        }

        protected virtual void Chase_Exit()
        {
            Log("Exiting Chase state");
            // Restore normal speed when exiting chase
            aiPath.maxSpeed = stats.maxSpeed;
        }

        #endregion

        #region State: Attack

        protected virtual void Attack_Enter()
        {
            Log("Entering Attack state");
            aiPath.canMove = false;
            lastAttackTime = -stats.attackCooldown;
        }

        protected virtual void Attack_Update()
        {
            if (currentTarget == null)
            {
                ChooseRandomBehavior();
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (!detection.HasLineOfSight(currentTarget))
            {
                Log("Lost sight of target during attack");
                fsm.ChangeState(EnemyState.Chase);
                return;
            }

            if (distanceToTarget > stats.attackRange * 1.2f)
            {
                fsm.ChangeState(EnemyState.Chase);
                return;
            }

            LookAtTarget(currentTarget);

            if (Time.time - lastAttackTime >= stats.attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;

                // Transition to Flee state after attacking
                Log("Attack complete, entering flee state");
                fsm.ChangeState(EnemyState.Flee);
            }
        }

        protected virtual void Attack_Exit()
        {
            Log("Exiting Attack state");
        }

        protected virtual void PerformAttack()
        {
            Log($"Attacking target for {stats.attackDamage} damage!");

            // Try to damage the target if it implements IDamageable
            if (currentTarget != null)
            {
                IDamageable damageable = currentTarget.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Convert damage to int and apply to sanity
                    int damageAmount = Mathf.RoundToInt(stats.attackDamage);
                    damageable.TakeDamage(AttributesType.Sanity, damageAmount);
                    Log($"Applied {damageAmount} sanity damage to {currentTarget.name}");
                }
                else
                {
                    Log($"Target {currentTarget.name} is not damageable (no IDamageable component)");
                }
            }

            OnAttackPerformed();
        }

        /// <summary>
        /// Override this to add custom attack effects (animations, sounds, particles, etc.)
        /// </summary>
        protected virtual void OnAttackPerformed()
        {
            // Override in derived classes for custom attack effects
        }

        #endregion

        #region State: Flee

        protected virtual void Flee_Enter()
        {
            Log("Entering Flee state - waiting for cooldown");
            aiPath.canMove = false;
            fleeTimer = 0f;
        }

        protected virtual void Flee_Update()
        {
            if (currentTarget == null)
            {
                ChooseRandomBehavior();
                return;
            }

            fleeTimer += Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            // Stay silent and look at target during cooldown
            LookAtTarget(currentTarget);

            // After cooldown, check if target is still in range
            if (fleeTimer >= fleeDuration)
            {
                // Check if target is within chase radius
                if (distanceToTarget <= chaseRadius && detection.HasLineOfSight(currentTarget))
                {
                    Log($"Cooldown complete, target still in range ({distanceToTarget:F1}m), re-engaging!");
                    fsm.ChangeState(EnemyState.Chase);
                    return;
                }
                else if (distanceToTarget > chaseRadius)
                {
                    Log($"Cooldown complete, target too far ({distanceToTarget:F1}m > {chaseRadius}m), giving up");
                    currentTarget = null;
                    ChooseRandomBehavior();
                    return;
                }
                else if (!detection.HasLineOfSight(currentTarget))
                {
                    Log("Cooldown complete, lost sight of target");
                    currentTarget = null;
                    ChooseRandomBehavior();
                    return;
                }
            }

            Log($"Fleeing... cooldown: {fleeTimer:F1}s/{fleeDuration}s");
        }

        protected virtual void Flee_Exit()
        {
            Log("Exiting Flee state");
        }

        #endregion


        #region Utility Methods

        protected virtual void LookAtTarget(Transform target)
        {
            if (target == null) return;

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * stats.rotationSpeed / 60f);
            }
        }

        protected void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] {message}");
            }
        }

        #endregion

        #region Debug

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying || !showStateInInspector) return;

            // Draw state label above enemy
            Vector3 labelPosition = transform.position + Vector3.up * 2.5f;

#if UNITY_EDITOR
            UnityEngine.GUIStyle style = new UnityEngine.GUIStyle();
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            // Color based on state
            switch (CurrentState)
            {
                case EnemyState.Idle:
                    style.normal.textColor = Color.white;
                    break;
                case EnemyState.Patrol:
                    style.normal.textColor = Color.cyan;
                    break;
                case EnemyState.Seen:
                    style.normal.textColor = Color.yellow;
                    break;
                case EnemyState.Chase:
                    style.normal.textColor = new Color(1f, 0.5f, 0f); // Orange
                    break;
                case EnemyState.Attack:
                    style.normal.textColor = Color.red;
                    break;
                case EnemyState.Flee:
                    style.normal.textColor = Color.magenta;
                    break;
            }

            string label = CurrentState.ToString().ToUpper();
            if (CurrentState == EnemyState.Seen && seenStateTimer > 0)
            {
                if (!seenSlowPhaseComplete)
                {
                    label += $"\n(Slow: {seenStateTimer:F1}s/{seenSlowMoveDuration}s)";
                }
                else
                {
                    label += $"\n(Wait: {seenStateTimer:F1}s/{seenStateTimeout}s)";
                }
            }
            else if (CurrentState == EnemyState.Flee && fleeTimer > 0)
            {
                label += $"\n(CD: {fleeTimer:F1}s/{fleeDuration}s)";
            }
            else if (targetLostTime > 0)
            {
                label += $"\n(Lost: {targetLostTime:F1}s)";
            }

            UnityEditor.Handles.Label(labelPosition, label, style);
#endif
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (stats == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.detectionRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);

            if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < patrolPoints.Length - 1; i++)
                {
                    if (patrolPoints[i] != null && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }

                if (loopPatrol && patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
                }
            }

            if (Application.isPlaying && currentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
        }

        #endregion
    }
}
