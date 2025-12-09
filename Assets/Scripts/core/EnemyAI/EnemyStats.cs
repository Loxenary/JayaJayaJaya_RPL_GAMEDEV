using UnityEngine;

namespace EnemyAI
{
    /// <summary>
    /// ScriptableObject for configuring enemy stats.
    /// This allows data-driven design and easy tweaking without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy AI/Enemy Stats")]
    public class EnemyStats : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Maximum movement speed")]
        public float maxSpeed = 3.5f;

        [Tooltip("How quickly the enemy accelerates")]
        public float acceleration = 8f;

        [Tooltip("Rotation speed in degrees per second")]
        public float rotationSpeed = 360f;

        [Header("Detection")]
        [Tooltip("How far the enemy can detect the player")]
        public float detectionRange = 10f;

        [Tooltip("Field of view angle in degrees (360 = all directions)")]
        public float detectionAngle = 120f;

        [Tooltip("Layer mask for detection raycast")]
        public LayerMask detectionLayer;

        [Header("Combat")]
        [Tooltip("Attack range")]
        public float attackRange = 2f;

        [Tooltip("Time between attacks")]
        public float attackCooldown = 1.5f;

        [Tooltip("Attack damage")]
        public float attackDamage = 10f;

        [Header("Health")]
        [Tooltip("Maximum health")]
        public float maxHealth = 100f;

        [Header("Patrol (Optional)")]
        [Tooltip("Enable patrol behavior")]
        public bool enablePatrol = false;

        [Tooltip("Time to wait at each patrol point")]
        public float patrolWaitTime = 2f;

        [Tooltip("Distance threshold to consider patrol point reached")]
        public float patrolPointReachedDistance = 0.5f;

        [Header("Chase")]
        [Range(1f, 3f)]
        [Tooltip("Speed multiplier during chase (1.5 = 50% faster)")]
        public float chaseSpeedMultiplier = 1.5f;

        [Tooltip("Distance before giving up chase")]
        public float maxChaseDistance = 20f;

        [Tooltip("Time to wait before giving up chase when losing sight")]
        public float loseTargetTime = 3f;
    }
}
