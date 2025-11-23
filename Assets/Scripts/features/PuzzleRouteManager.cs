using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace JayaJayaJaya.Features
{
    /// <summary>
    /// Manager untuk mengatur puzzle conditions dan membuka route/jalur baru
    /// Akan mengecek kondisi puzzle secara periodik atau manual dan membuka obstacle jika syarat terpenuhi
    /// </summary>
    public class PuzzleRouteManager : MonoBehaviour
    {
        [Header("Puzzle Conditions")]
        [SerializeField] private List<MonoBehaviour> puzzleConditions = new List<MonoBehaviour>();
        [SerializeField] private bool requireAllConditions = true; // True = AND, False = OR
        
        [Header("Obstacles to Remove")]
        [SerializeField] private List<ObstacleController> obstacles = new List<ObstacleController>();
        
        [Header("Check Settings")]
        [SerializeField] private bool autoCheck = true;
        [SerializeField] private float checkInterval = 0.5f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onRouteUnlocked;
        [SerializeField] private UnityEvent onConditionsMet;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private bool routeUnlocked = false;
        private float nextCheckTime = 0f;
        private List<IPuzzleCondition> cachedConditions = new List<IPuzzleCondition>();
        
        private void Start()
        {
            InitializeConditions();
            
            if (showDebugInfo)
            {
                Debug.Log($"[PuzzleRouteManager] Initialized dengan {cachedConditions.Count} kondisi dan {obstacles.Count} obstacles.");
                Debug.Log($"[PuzzleRouteManager] Mode: {(requireAllConditions ? "Semua kondisi harus terpenuhi (AND)" : "Minimal satu kondisi terpenuhi (OR)")}");
            }
        }
        
        private void InitializeConditions()
        {
            cachedConditions.Clear();
            
            foreach (var condition in puzzleConditions)
            {
                if (condition is IPuzzleCondition puzzleCondition)
                {
                    cachedConditions.Add(puzzleCondition);
                }
                else if (condition != null)
                {
                    Debug.LogWarning($"[PuzzleRouteManager] Component '{condition.GetType().Name}' tidak implement IPuzzleCondition dan akan diabaikan.");
                }
            }
        }
        
        private void Update()
        {
            if (autoCheck && !routeUnlocked && Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                CheckConditionsAndUnlockRoute();
            }
        }
        
        /// <summary>
        /// Cek kondisi puzzle dan unlock route jika syarat terpenuhi
        /// </summary>
        public void CheckConditionsAndUnlockRoute()
        {
            if (routeUnlocked)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[PuzzleRouteManager] Route sudah terbuka.");
                }
                return;
            }
            
            if (cachedConditions.Count == 0)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("[PuzzleRouteManager] Tidak ada kondisi yang didefinisikan!");
                }
                return;
            }
            
            bool conditionsMet = CheckAllConditions();
            
            if (conditionsMet)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[PuzzleRouteManager] Semua kondisi terpenuhi! Membuka route...");
                }
                
                UnlockRoute();
            }
            else if (showDebugInfo)
            {
                LogConditionStatus();
            }
        }
        
        private bool CheckAllConditions()
        {
            if (requireAllConditions)
            {
                // Mode AND: Semua kondisi harus terpenuhi
                return cachedConditions.All(condition => condition.IsConditionMet());
            }
            else
            {
                // Mode OR: Minimal satu kondisi terpenuhi
                return cachedConditions.Any(condition => condition.IsConditionMet());
            }
        }
        
        private void LogConditionStatus()
        {
            Debug.Log($"[PuzzleRouteManager] Status kondisi puzzle:");
            foreach (var condition in cachedConditions)
            {
                string status = condition.IsConditionMet() ? "✓" : "✗";
                Debug.Log($"  {status} {condition.ConditionName}");
            }
        }
        
        private void UnlockRoute()
        {
            routeUnlocked = true;
            
            // Trigger event
            onConditionsMet?.Invoke();
            
            // Hapus semua obstacles
            foreach (var obstacle in obstacles)
            {
                if (obstacle != null)
                {
                    obstacle.RemoveObstacle();
                }
            }
            
            // Trigger event setelah route unlocked
            onRouteUnlocked?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log($"[PuzzleRouteManager] Route berhasil dibuka! {obstacles.Count} obstacle dihapus.");
            }
        }
        
        /// <summary>
        /// Force unlock route tanpa cek kondisi (untuk testing)
        /// </summary>
        [ContextMenu("Force Unlock Route")]
        public void ForceUnlockRoute()
        {
            if (showDebugInfo)
            {
                Debug.Log("[PuzzleRouteManager] Force unlock route (bypass kondisi)");
            }
            
            UnlockRoute();
        }
        
        /// <summary>
        /// Reset manager (untuk testing atau game restart)
        /// </summary>
        [ContextMenu("Reset Manager")]
        public void ResetManager()
        {
            routeUnlocked = false;
            
            // Reset semua location triggers
            foreach (var condition in puzzleConditions)
            {
                if (condition is LocationTrigger locationTrigger)
                {
                    locationTrigger.ResetTrigger();
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log("[PuzzleRouteManager] Manager direset.");
            }
        }
        
        /// <summary>
        /// Tambah kondisi puzzle secara runtime
        /// </summary>
        public void AddCondition(IPuzzleCondition condition)
        {
            if (condition != null && !cachedConditions.Contains(condition))
            {
                cachedConditions.Add(condition);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[PuzzleRouteManager] Kondisi '{condition.ConditionName}' ditambahkan.");
                }
            }
        }
        
        /// <summary>
        /// Tambah obstacle secara runtime
        /// </summary>
        public void AddObstacle(ObstacleController obstacle)
        {
            if (obstacle != null && !obstacles.Contains(obstacle))
            {
                obstacles.Add(obstacle);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[PuzzleRouteManager] Obstacle '{obstacle.gameObject.name}' ditambahkan.");
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            // Visualisasi koneksi antara manager dengan obstacles
            if (obstacles != null && obstacles.Count > 0)
            {
                Gizmos.color = routeUnlocked ? Color.green : Color.yellow;
                
                foreach (var obstacle in obstacles)
                {
                    if (obstacle != null)
                    {
                        Gizmos.DrawLine(transform.position, obstacle.transform.position);
                    }
                }
            }
        }
    }
}
