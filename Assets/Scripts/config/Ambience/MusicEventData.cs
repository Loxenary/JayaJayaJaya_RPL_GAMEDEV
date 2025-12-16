using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ambience
{
    /// <summary>
    /// Configuration for a single music event, defining what music plays when triggered
    /// </summary>
    [Serializable]
    public struct MusicEventConfig
    {
        [Tooltip("The event type that triggers this music")]
        public MusicEventType eventType;

        [Tooltip("Priority level (higher = more important). Used when multiple events are active.")]
        [Range(0, 100)]
        public int priority;

        [Tooltip("List of music tracks to play for this event (randomly selected if multiple)")]
        public List<MusicIdentifier> musicTracks;

        [Tooltip("Should this music loop until the event ends?")]
        public bool loopMusic;

        [Tooltip("Should this event interrupt currently playing music?")]
        public bool interruptCurrent;

        [Tooltip("Custom fade-in duration for this event (0 = use default)")]
        [Range(0f, 10f)]
        public float customFadeInDuration;

        [Tooltip("Custom fade-out duration for this event (0 = use default)")]
        [Range(0f, 10f)]
        public float customFadeOutDuration;
    }

    /// <summary>
    /// ScriptableObject containing all music event configurations
    /// </summary>
    [CreateAssetMenu(fileName = "MusicEventData", menuName = "Audio/Music Event Data", order = 1)]
    public class MusicEventData : ScriptableObject
    {
        [Header("Event Configurations")]
        [Tooltip("List of all music event configurations")]
        [SerializeField] private List<MusicEventConfig> eventConfigs = new List<MusicEventConfig>();

        [Header("Default Settings")]
        [Tooltip("Default music sequence to play when no events are active")]
        [SerializeField] private List<MusicIdentifier> defaultMusicSequence = new List<MusicIdentifier>();

        [Tooltip("Should default music loop through the sequence?")]
        [SerializeField] private bool loopDefaultSequence = true;

        [Tooltip("Should default music shuffle?")]
        [SerializeField] private bool shuffleDefaultSequence = false;

        #region Properties

        public List<MusicEventConfig> EventConfigs => eventConfigs;
        public List<MusicIdentifier> DefaultMusicSequence => defaultMusicSequence;
        public bool LoopDefaultSequence => loopDefaultSequence;
        public bool ShuffleDefaultSequence => shuffleDefaultSequence;

        #endregion

        #region Query Methods

        /// <summary>
        /// Get music event configuration by event type
        /// </summary>
        public MusicEventConfig? GetEventConfig(MusicEventType eventType)
        {
            foreach (var config in eventConfigs)
            {
                if (config.eventType == eventType)
                {
                    return config;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all event configs sorted by priority (highest first)
        /// </summary>
        public List<MusicEventConfig> GetSortedByPriority()
        {
            var sorted = new List<MusicEventConfig>(eventConfigs);
            sorted.Sort((a, b) => b.priority.CompareTo(a.priority));
            return sorted;
        }

        /// <summary>
        /// Get a random music track from an event configuration
        /// </summary>
        public MusicIdentifier? GetRandomMusicForEvent(MusicEventType eventType)
        {
            var config = GetEventConfig(eventType);
            if (!config.HasValue || config.Value.musicTracks == null || config.Value.musicTracks.Count == 0)
            {
                return null;
            }

            int randomIndex = UnityEngine.Random.Range(0, config.Value.musicTracks.Count);
            return config.Value.musicTracks[randomIndex];
        }

        /// <summary>
        /// Check if an event type exists in this configuration
        /// </summary>
        public bool HasEventConfig(MusicEventType eventType)
        {
            return GetEventConfig(eventType).HasValue;
        }

        #endregion

        #region Editor Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure no duplicate event types
            var eventTypes = new HashSet<MusicEventType>();
            foreach (var config in eventConfigs)
            {
                if (config.eventType != MusicEventType.None && eventTypes.Contains(config.eventType))
                {
                    Debug.LogWarning($"[MusicEventData] Duplicate event type found: {config.eventType}");
                }
                eventTypes.Add(config.eventType);
            }
        }
#endif

        #endregion
    }
}
