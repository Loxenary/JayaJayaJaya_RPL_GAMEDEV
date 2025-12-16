using UnityEngine;

namespace Ambience
{
    /// <summary>
    /// Example controller for your specific music flow:
    /// Start: Silence
    /// Collectible 1: Main Theme → Silence 15s → Mansion Empty loop
    /// Collectible 2: Small Hallway
    /// Collectible 3: Time to Get Out
    /// Enemy Seen: Chase music (Noni Level 1) → Returns to previous when chase ends
    /// </summary>
    public class CollectibleMusicController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameMusicManager musicManager;

        private int collectiblesCollected = 0;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            if (musicManager == null)
            {
                musicManager = FindAnyObjectByType<GameMusicManager>();
            }

            // Start with silence (nothing plays)
            Log("Game started - music will remain silent until first collectible");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InteractedPuzzleCount>(OnCollectibleCollected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractedPuzzleCount>(OnCollectibleCollected);
        }

        #region Collectible Events

        /// <summary>
        /// Call this when player collects a collectible
        /// </summary>
        public void OnCollectibleCollected(InteractedPuzzleCount evt)
        {
            collectiblesCollected = evt.puzzleCount;
            Log($"Collectible collected! Total: {collectiblesCollected}");

            switch (collectiblesCollected)
            {
                case 1:
                    OnFirstCollectible();
                    break;

                case 2:
                    OnSecondCollectible();
                    break;

                case 3:
                    OnThirdCollectible();
                    break;

                default:
                    Log($"Already collected {collectiblesCollected} collectibles");
                    break;
            }
        }

        private void OnFirstCollectible()
        {
            Log("First collectible! Triggering Main Theme");
            // This will play the Main Theme, then silence, then start Mansion Empty loop
            EventBus.Publish(new MusicEventRequest(MusicEventType.FirstCollectible));
        }

        private void OnSecondCollectible()
        {
            Log("Second collectible! Triggering Small Hallway music");
            EventBus.Publish(new MusicEventRequest(MusicEventType.SecondCollectible));
        }

        private void OnThirdCollectible()
        {
            Log("Third collectible! Triggering Time to Get Out");
            EventBus.Publish(new MusicEventRequest(MusicEventType.ThirdCollectible));
        }

        #endregion

        #region Enemy Chase Events

        /// <summary>
        /// Call this when enemy sees/detects player
        /// </summary>
        public void OnEnemySeen()
        {
            Log("Enemy detected player! Starting chase music");
            // This will interrupt current music and play chase theme
            EventBus.Publish(new MusicEventRequest(MusicEventType.Enemy_Seen));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get current collectible count
        /// </summary>
        public int GetCollectibleCount()
        {
            return collectiblesCollected;
        }

        /// <summary>
        /// Reset collectible count (useful for testing)
        /// </summary>
        public void ResetCollectibles()
        {
            collectiblesCollected = 0;
            musicManager?.ClearAllEvents();
            Log("Collectibles reset, music cleared");
        }

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[CollectibleMusicController] {message}");
            }
        }

        #endregion
    }
}
