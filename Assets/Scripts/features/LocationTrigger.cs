using UnityEngine;

namespace JayaJayaJaya.Features
{
    /// <summary>
    /// Trigger berdasarkan lokasi player sebagai dummy implementation untuk puzzle condition
    /// Kondisi terpenuhi ketika player memasuki trigger area
    /// </summary>
    public class LocationTrigger : MonoBehaviour, IPuzzleCondition
    {
        [Header("Trigger Settings")]
        [SerializeField] private string triggerTag = "Player";
        [SerializeField] private string conditionName = "Location Trigger";

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private bool isTriggered = false;

        public string ConditionName => conditionName;

        public bool IsConditionMet()
        {
            return isTriggered;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(triggerTag))
            {
                isTriggered = true;

                if (showDebugInfo)
                {
                    Debug.Log($"[LocationTrigger] {conditionName} - Kondisi terpenuhi! Player memasuki area trigger.");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(triggerTag))
            {
                // Optional: bisa direset jika player keluar
                // isTriggered = false;

                if (showDebugInfo)
                {
                    Debug.Log($"[LocationTrigger] {conditionName} - Player keluar dari area trigger.");
                }
            }
        }

        /// <summary>
        /// Reset trigger state (untuk testing atau game restart)
        /// </summary>
        public void ResetTrigger()
        {
            isTriggered = false;

            if (showDebugInfo)
            {
                Debug.Log($"[LocationTrigger] {conditionName} - Trigger direset.");
            }
        }

        private void OnDrawGizmos()
        {
            // Visualisasi trigger area di editor
            Gizmos.color = isTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
