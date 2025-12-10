using System;
using UnityEngine;

namespace Ambience
{
    /// <summary>
    /// Defines the different types of game conditions that can trigger specific music.
    /// Priority values embedded in enum for clear hierarchy.
    /// </summary>
    public enum ConditionType
    {
        None = 0,

        // Enemy (Noni) Levels - Highest base priority (100-103)
        NoniLevel1 = 100,
        NoniLevel2 = 101,
        NoniLevel3 = 102,
        NoniLevel4 = 103,

        // Sanity Conditions - Medium priority (50-51)
        LowSanity = 50,
        VeryLowSanity = 51,

        // Critical States - Override priority (200-300)
        PlayerDead = 200,
        Ending = 300
    }

    /// <summary>
    /// Maps a game condition to a music track with priority weighting.
    /// Used by AmbienceConfiguration to determine which music plays when.
    /// </summary>
    [Serializable]
    public struct AmbienceConditionRecord
    {
        [Tooltip("Type of game condition that triggers this music")]
        public ConditionType conditionType;

        [Tooltip("Music track to play when this condition is active")]
        public MusicIdentifier musicTrack;

        [Tooltip("Priority value - higher = more important. Typically matches ConditionType enum value.")]
        [Range(0, 300)]
        public int priority;

        public AmbienceConditionRecord(ConditionType type, MusicIdentifier music, int priority)
        {
            this.conditionType = type;
            this.musicTrack = music;
            this.priority = priority;
        }
    }
}
