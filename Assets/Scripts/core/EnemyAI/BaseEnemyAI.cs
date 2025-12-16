using UnityEngine;
using MonsterLove.StateMachine;
using Pathfinding;
using System.Collections.Generic;
using Ambience;
using System;

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

        public enum AudioPlayType
        {
            Random,
            Sequential,
        }

        [Serializable]
        public class AudioStateRecords
        {
            public AudioPlayType playType = AudioPlayType.Random;
            public bool loop = false;
            public SfxClipData[] audioClips;

            public int Length => audioClips.Length;

            public void PlayRandom(EnemyAudioProvider audioProvider)
            {
                if (loop)
                {
                    audioProvider.PlayRandomSfxLooping(audioClips);
                }
            }

            public void PlaySFX(EnemyAudioProvider audioProvider)
            {
                switch (playType)
                {
                    case AudioPlayType.Random:
                        if (loop)
                        {
                            audioProvider.PlayRandomSfxLooping(audioClips);
                        }
                        else
                        {
                            audioProvider.PlayRandomSfxOnce(audioClips);
                        }
                        break;
                    case AudioPlayType.Sequential:
                        if (loop)
                        {
                            audioProvider.PlaySequentialSfxLooping(audioClips);
                        }
                        else
                        {
                            // For non-looping sequential, just play the first one
                            audioProvider.PlayRandomSfxOnce(audioClips);
                        }
                        break;
                }
            }


        }

        [Header("Configuration")]
        [SerializeField] protected EnemyStats stats;

        [Header("Angry System Integration (Optional)")]
        [Tooltip("Reference to the global EnemyAngrySystem. If set, enemy will use dynamic stats based on anger level.")]
        [SerializeField] protected EnemyAngrySystem angrySystem;
        [Tooltip("If true, enemy stats will automatically update when anger level changes")]
        [SerializeField] protected bool useDynamicAngryStats = false;

        [Header("Patrol Points (Optional)")]

        [SerializeField] protected Transform patrolGroups;

#if UNITY_EDITOR
        [SerializeField, ReadOnly] protected List<Transform> _patrolPoints => patrolPoints;
#endif

        protected List<Transform> patrolPoints = new();

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

        [Header("Audio Config")]
        [SerializeField] private EnemyAudioProvider audioProvider;
        [SerializeField] private AudioStateRecords attackSfxs;
        [SerializeField] private AudioStateRecords chasingSfxs;
        [SerializeField] private AudioStateRecords seenSfxs;
        [SerializeField] private AudioStateRecords patrolSfxs;

        [SerializeField] private AudioStateRecords fleeSfxs;
        [SerializeField] private AudioStateRecords lostSfxs;

        [Header("Animation Settings")]
        [Tooltip("Name of the walk animation state")]
        [SerializeField] protected string walkAnimationName = "Walk";
        [Tooltip("Name of the attack animation state")]
        [SerializeField] protected string attackAnimationName = "Attack";
        [Tooltip("Use crossfade for smoother transitions")]
        [SerializeField] protected bool useAnimationCrossFade = true;
        [Tooltip("Crossfade duration in seconds")]
        [SerializeField] protected float animationCrossFadeDuration = 0.15f;


        private GameMusicManager _musicManager;

        protected StateMachine<EnemyState, StateDriverUnity> fsm;
        protected AIPath aiPath;

        protected AIDestinationSetter destinationSetter;
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

        // Patrol improvement fields
        protected float patrolPointReachedDistance = 1.5f; // Distance threshold to consider patrol point reached
        protected float patrolStuckTimer = 0f;
        protected float patrolStuckThreshold = 5f; // Time before considering the AI stuck
        protected Vector3 lastPatrolPosition;
        protected float patrolPositionCheckInterval = 0.5f; // How often to check if position changed
        protected float patrolPositionCheckTimer = 0f;
        protected bool isStuckAtPatrolPoint = false;

        public EnemyState CurrentState
        {
            get
            {
                if (fsm == null) return EnemyState.Idle;
                try
                {
                    return fsm.State;
                }
                catch
                {
                    return EnemyState.Idle;
                }
            }
        }
        public float HealthPercentage => currentHealth / stats.maxHealth;
        public bool IsAlive => currentHealth > 0;


        [Header("CURRENT STATE (Read Only)")]
        private string _currentStateDisplay = "Not Started";
        private string _targetInfo = "No Target";
        private float _visionTimer = 0f;
        private float _fleeTimer = 0f;


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
        }

        public void Initialize(Transform patrolPoint)
        {
            this.patrolGroups = patrolPoint;
            PatrolGroupRework();
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

        private void PatrolGroupRework()
        {
            patrolPoints.Clear();

            if (patrolGroups == null)
            {
                Debug.LogWarning($"[{gameObject.name}] PatrolGroups is null! Cannot initialize patrol points.");
                return;
            }

            // Get all child transforms from patrolGroups
            foreach (Transform child in patrolGroups)
            {
                // Skip the parent itself, only add actual patrol point children
                if (child != patrolGroups)
                {
                    patrolPoints.Add(child);
                }
            }

            if (patrolPoints.Count == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] No patrol points found in patrolGroups!");
            }
            else
            {
                Log($"Initialized {patrolPoints.Count} patrol points");
            }
        }

        protected virtual void ChooseRandomBehavior()
        {
            if (patrolPoints != null && patrolPoints.Count > 0 && UnityEngine.Random.value <= patrolChance)
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
            currentIdleWaitTime = UnityEngine.Random.Range(idleWaitTimeMin, idleWaitTimeMax);
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

            // Reset patrol tracking variables
            patrolStuckTimer = 0f;
            patrolPositionCheckTimer = 0f;
            lastPatrolPosition = transform.position;
            isStuckAtPatrolPoint = false;

            // Initialize patrol points if not already done
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                PatrolGroupRework();
            }

            SetNextPatrolPoint();

            // Play walk animation
            PlayWalkAnimation();

            // Play patrol sound
            PlayPatrolSound();
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

            // Validate patrol points exist
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                Log("No patrol points available, switching to idle");
                ChooseRandomBehavior();
                return;
            }

            // Ensure current patrol index is valid
            if (currentPatrolIndex >= patrolPoints.Count)
            {
                currentPatrolIndex = 0;
            }

            Transform currentPatrolPoint = patrolPoints[currentPatrolIndex];
            if (currentPatrolPoint == null)
            {
                Log($"Patrol point {currentPatrolIndex} is null, moving to next");
                MoveToNextPatrolPoint();
                return;
            }

            // Calculate distance to current patrol point
            float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint.position);

            // Check if reached patrol point using distance threshold
            bool reachedByDistance = distanceToPatrolPoint <= patrolPointReachedDistance;
            bool reachedByPathfinding = aiPath.reachedEndOfPath;

            // Update stuck detection timer
            patrolPositionCheckTimer += Time.deltaTime;
            if (patrolPositionCheckTimer >= patrolPositionCheckInterval)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastPatrolPosition);

                // Check if AI is stuck (hasn't moved significantly)
                if (distanceMoved < 0.1f)
                {
                    patrolStuckTimer += patrolPositionCheckTimer;

                    if (patrolStuckTimer >= patrolStuckThreshold)
                    {
                        if (!isStuckAtPatrolPoint)
                        {
                            Log($"AI stuck at patrol point {currentPatrolIndex} for {patrolStuckTimer:F1}s. Forcing move to next point.");
                            isStuckAtPatrolPoint = true;
                        }

                        // Force move to next patrol point after being stuck
                        MoveToNextPatrolPoint();
                        patrolStuckTimer = 0f;
                    }
                }
                else
                {
                    // AI is moving, reset stuck timer
                    patrolStuckTimer = 0f;
                    isStuckAtPatrolPoint = false;
                }

                lastPatrolPosition = transform.position;
                patrolPositionCheckTimer = 0f;
            }

            // Check if reached patrol point (by either method)
            if (reachedByDistance || reachedByPathfinding)
            {
                Log($"Reached patrol point {currentPatrolIndex} (Distance: {distanceToPatrolPoint:F2}, ByPath: {reachedByPathfinding})");
                MoveToNextPatrolPoint();
            }
        }

        protected virtual void Patrol_Exit()
        {
            Log("Exiting Patrol state");
        }

        protected virtual void SetNextPatrolPoint()
        {
            if (patrolPoints != null && patrolPoints.Count > 0 && currentPatrolIndex < patrolPoints.Count)
            {
                aiPath.destination = patrolPoints[currentPatrolIndex].position;
                Log($"Moving to patrol point {currentPatrolIndex}");

                // Reset stuck detection when setting new patrol point
                patrolStuckTimer = 0f;
                isStuckAtPatrolPoint = false;
                lastPatrolPosition = transform.position;
            }
        }

        protected virtual void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                Log("Cannot move to next patrol point: no patrol points available");
                return;
            }

            currentPatrolIndex++;

            // Handle end of patrol points list
            if (currentPatrolIndex >= patrolPoints.Count)
            {
                if (loopPatrol)
                {
                    Log("Reached end of patrol points, looping back to start");
                    currentPatrolIndex = 0;
                    SetNextPatrolPoint();

                    // Play patrol sound when reaching a new patrol point
                    PlayPatrolSound();
                }
                else
                {
                    Log("Reached end of patrol points, choosing random behavior");
                    ChooseRandomBehavior();
                }
            }
            else
            {
                SetNextPatrolPoint();

                // Play patrol sound when reaching a new patrol point
                PlayPatrolSound();
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

            // Play walk animation
            PlayWalkAnimation();

            // Play seen sound
            PlaySeenSound();
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
                    PlayLostSound();
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
                PlayLostSound();
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
            aiPath.maxSpeed = stats.maxSpeed * stats.chaseSpeedMultiplier * stats.chaseSpeedMultiplier;
            targetLostTime = 0f;

            Log($"Chase speed: {aiPath.maxSpeed:F1} (base: {stats.maxSpeed}, multiplier: {stats.chaseSpeedMultiplier}x)");

            // Initialize last known position if we have a target
            if (currentTarget != null)
            {
                lastKnownTargetPosition = currentTarget.position;
            }

            // Play walk animation
            PlayWalkAnimation();

            // Play looping chasing sound
            PlayChasingSound();
        }

        protected virtual void Chase_Update()
        {
            if (currentTarget == null)
            {
                ChooseRandomBehavior();
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            // Check if target is too far away (give up chase)
            if (distanceToTarget > chaseRadius * 1.5f)
            {
                Log("Target moved too far outside chase zone, giving up");
                PlayLostSound();
                currentTarget = null;
                ChooseRandomBehavior();
                return;
            }

            // Check if within attack range
            if (distanceToTarget <= stats.attackRange)
            {
                fsm.ChangeState(EnemyState.Attack);
                return;
            }

            // Check line of sight
            bool hasLOS = detection.HasLineOfSight(currentTarget);

            if (hasLOS)
            {
                // Has line of sight - chase directly
                lastKnownTargetPosition = currentTarget.position;
                aiPath.destination = currentTarget.position;
                targetLostTime = 0f;
            }
            else
            {
                // Lost line of sight but still in range
                targetLostTime += Time.deltaTime;

                // Continue moving to last known position
                aiPath.destination = lastKnownTargetPosition;

                // Only give up if lost sight for too long AND target is far from last known position
                if (targetLostTime >= visionLostCountdown)
                {
                    float distanceToLastKnown = Vector3.Distance(transform.position, lastKnownTargetPosition);

                    // If we reached last known position and still no sight, give up
                    if (distanceToLastKnown < 2f)
                    {
                        Log($"Lost vision of target for {visionLostCountdown}s and reached last known position, giving up");
                        PlayLostSound();
                        currentTarget = null;
                        ChooseRandomBehavior();
                        return;
                    }
                }

                Log($"Lost sight but continuing to last known position ({targetLostTime:F1}s)");
            }
        }

        protected virtual void Chase_Exit()
        {
            Log("Exiting Chase state");

            // Stop looping chasing sound
            StopChasingSound();
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

            // Play attack animation
            PlayAttackAnimation();

            EventBus.Publish(new DamageVisualFeedback.TriggerPulseEvent());

            // Play attack sound
            PlayAttackSound();

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

            // Play flee sound
            PlayFleeSound();
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
                    PlayLostSound();
                    currentTarget = null;
                    ChooseRandomBehavior();
                    return;
                }
                else if (!detection.HasLineOfSight(currentTarget))
                {
                    Log("Cooldown complete, lost sight of target");
                    PlayLostSound();
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

        #region Audio Methods

        /// <summary>
        /// Plays attack sound based on configuration (random or sequential, looping or once).
        /// </summary>
        protected virtual void PlayAttackSound()
        {
            if (audioProvider != null && attackSfxs != null && attackSfxs.Length > 0)
            {
                attackSfxs.PlaySFX(audioProvider);
                Log("Playing attack sound");
            }
        }

        /// <summary>
        /// Plays chasing sound based on configuration (random or sequential, looping or once).
        /// </summary>
        protected virtual void PlayChasingSound()
        {
            if (audioProvider != null && chasingSfxs != null && chasingSfxs.Length > 0)
            {
                // Only start if not already playing
                if (!audioProvider.IsLoopingPlaying() && !audioProvider.IsSequentialPlaying())
                {
                    chasingSfxs.PlaySFX(audioProvider);
                    Log("Playing chasing sound");
                }
            }
        }

        /// <summary>
        /// Stops the looping chasing sound.
        /// </summary>
        protected virtual void StopChasingSound()
        {
            if (audioProvider != null)
            {
                audioProvider.StopLoopingSfx();
                Log("Stopped chasing sound");
            }
        }

        /// <summary>
        /// Plays seen sound based on configuration (random or sequential, looping or once).
        /// </summary>
        protected virtual void PlaySeenSound()
        {
            if (audioProvider != null && seenSfxs != null && seenSfxs.Length > 0)
            {
                seenSfxs.PlaySFX(audioProvider);
                Log("Playing seen sound");
            }
        }

        /// <summary>
        /// Plays patrol sound based on configuration (random or sequential, looping or once).
        /// </summary>
        protected virtual void PlayPatrolSound()
        {
            if (audioProvider != null && patrolSfxs != null && patrolSfxs.Length > 0)
            {
                patrolSfxs.PlaySFX(audioProvider);
                Log("Playing patrol sound");
            }
        }

        /// <summary>
        /// Plays flee sound based on configuration (random or sequential, looping or once).
        /// </summary>
        protected virtual void PlayFleeSound()
        {
            if (audioProvider != null && fleeSfxs != null && fleeSfxs.Length > 0)
            {
                fleeSfxs.PlaySFX(audioProvider);
                Log("Playing flee sound");
            }
        }

        /// <summary>
        /// Plays lost sound based on configuration (random or sequential, looping or once).
        /// </summary>
        protected virtual void PlayLostSound()
        {
            if (audioProvider != null && lostSfxs != null && lostSfxs.Length > 0)
            {
                lostSfxs.PlaySFX(audioProvider);
                Log("Playing lost sound");
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Plays the walk animation.
        /// </summary>
        protected virtual void PlayWalkAnimation()
        {
            if (string.IsNullOrEmpty(walkAnimationName))
            {
                return;
            }

            if (useAnimationCrossFade)
            {
                EventBus.Publish(EnemyAnimationPlay.WithCrossFade(walkAnimationName, animationCrossFadeDuration));
            }
            else
            {
                EventBus.Publish(EnemyAnimationPlay.Simple(walkAnimationName));
            }

            Log($"Playing walk animation: {walkAnimationName}");
        }

        /// <summary>
        /// Plays the attack animation.
        /// </summary>
        protected virtual void PlayAttackAnimation()
        {
            if (string.IsNullOrEmpty(attackAnimationName))
            {
                return;
            }

            if (useAnimationCrossFade)
            {
                EventBus.Publish(EnemyAnimationPlay.WithCrossFade(attackAnimationName, animationCrossFadeDuration));
            }
            else
            {
                EventBus.Publish(EnemyAnimationPlay.Simple(attackAnimationName));
            }

            Log($"Playing attack animation: {attackAnimationName}");
        }

        /// <summary>
        /// Sets a bool parameter on the animator.
        /// </summary>
        protected virtual void SetAnimationBool(string parameterName, bool value)
        {
            EventBus.Publish(EnemyAnimationBool.Create(parameterName, value));
            Log($"Set animation bool: {parameterName} = {value}");
        }

        /// <summary>
        /// Sets a float parameter on the animator (e.g., speed).
        /// </summary>
        protected virtual void SetAnimationFloat(string parameterName, float value)
        {
            EventBus.Publish(EnemyAnimationFloat.Create(parameterName, value));
            Log($"Set animation float: {parameterName} = {value}");
        }

        /// <summary>
        /// Triggers an animator trigger.
        /// </summary>
        protected virtual void TriggerAnimation(string triggerName)
        {
            EventBus.Publish(EnemyAnimationTrigger.Create(triggerName));
            Log($"Triggered animation: {triggerName}");
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

            if (patrolPoints != null && patrolPoints.Count > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < patrolPoints.Count - 1; i++)
                {
                    if (patrolPoints[i] != null && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }

                if (loopPatrol && patrolPoints[0] != null && patrolPoints[patrolPoints.Count - 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[patrolPoints.Count - 1].position, patrolPoints[0].position);
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
