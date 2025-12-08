using UnityEngine;

namespace EnemyAI
{
    /// <summary>
    /// Handles enemy detection logic including line of sight and range checks.
    /// Detection settings are controlled by EnemyStats - this component just handles the logic.
    /// </summary>
    public class EnemyDetection : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform detectionOrigin;

        [Header("Obstacles (Optional)")]
        [Tooltip("Layer mask for obstacles that block line of sight (walls, etc.)")]
        [SerializeField] private LayerMask obstacleLayer;

        private Transform currentTarget;
        private float detectionRange;
        private float detectionAngle;
        private LayerMask detectionLayer;

        public Transform CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null;

        private void Awake()
        {
            if (detectionOrigin == null)
                detectionOrigin = transform;
        }

        /// <summary>
        /// Initialize detection with settings from EnemyStats.
        /// Must be called before using detection!
        /// </summary>
        public void Initialize(float range, float angle, LayerMask layer)
        {
            detectionRange = range;
            detectionAngle = angle;
            detectionLayer = layer;
        }

        /// <summary>
        /// Scan for targets within detection range and FOV.
        /// </summary>
        public Transform ScanForTarget()
        {
            Collider[] targetsInRange = Physics.OverlapSphere(detectionOrigin.position, detectionRange, detectionLayer);

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider targetCollider in targetsInRange)
            {
                Transform target = targetCollider.transform;

                // Skip if this is our own collider or a child of this GameObject
                if (target == transform || target.IsChildOf(transform) || transform.IsChildOf(target))
                {
                    continue;
                }

                Vector3 directionToTarget = (target.position - detectionOrigin.position).normalized;

                // Check if target is within FOV
                if (Vector3.Angle(detectionOrigin.forward, directionToTarget) < detectionAngle / 2f)
                {
                    float distanceToTarget = Vector3.Distance(detectionOrigin.position, target.position);

                    // Line of sight check
                    if (HasLineOfSight(target))
                    {
                        if (distanceToTarget < closestDistance)
                        {
                            closestDistance = distanceToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }

            currentTarget = closestTarget;
            return closestTarget;
        }

        /// <summary>
        /// Check if there's a clear line of sight to the target.
        /// </summary>
        public bool HasLineOfSight(Transform target)
        {
            if (target == null) return false;

            Vector3 directionToTarget = target.position - detectionOrigin.position;
            float distanceToTarget = directionToTarget.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(detectionOrigin.position, directionToTarget.normalized, out hit, distanceToTarget, obstacleLayer))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if target is within detection range.
        /// </summary>
        public bool IsTargetInRange(Transform target)
        {
            if (target == null) return false;
            return Vector3.Distance(detectionOrigin.position, target.position) <= detectionRange;
        }

        /// <summary>
        /// Get distance to current target.
        /// </summary>
        public float GetDistanceToTarget()
        {
            if (currentTarget == null) return Mathf.Infinity;
            return Vector3.Distance(detectionOrigin.position, currentTarget.position);
        }

        /// <summary>
        /// Clear current target.
        /// </summary>
        public void ClearTarget()
        {
            currentTarget = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionOrigin == null) detectionOrigin = transform;

            // Draw detection range sphere (faded)
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawWireSphere(detectionOrigin.position, detectionRange);

            // Draw FOV cone
            Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle / 2f, 0) * detectionOrigin.forward * detectionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle / 2f, 0) * detectionOrigin.forward * detectionRange;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(detectionOrigin.position, detectionOrigin.position + leftBoundary);
            Gizmos.DrawLine(detectionOrigin.position, detectionOrigin.position + rightBoundary);

            // Draw cone arc
            int segments = 20;
            Vector3 previousPoint = detectionOrigin.position + leftBoundary;
            for (int i = 1; i <= segments; i++)
            {
                float angle = -detectionAngle / 2f + (detectionAngle / segments) * i;
                Vector3 point = detectionOrigin.position + Quaternion.Euler(0, angle, 0) * detectionOrigin.forward * detectionRange;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            // Draw line to current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(detectionOrigin.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
            }
        }
    }
}
