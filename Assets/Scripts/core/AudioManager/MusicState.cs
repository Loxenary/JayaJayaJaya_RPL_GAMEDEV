using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ambience
{
    /// <summary>
    /// Represents a saved music state that can be restored later
    /// Used for returning to previous music after temporary interruptions
    /// </summary>
    [Serializable]
    public class MusicState
    {
        public MusicIdentifier? currentTrack;
        public float trackTime;
        public bool wasPlaying;
        public MusicEventType? eventType;

        // For sequence playback
        public List<MusicIdentifier> sequence;
        public int sequenceIndex;
        public bool isSequence;

        public MusicState()
        {
            currentTrack = null;
            trackTime = 0f;
            wasPlaying = false;
            eventType = null;
            sequence = null;
            sequenceIndex = 0;
            isSequence = false;
        }

        /// <summary>
        /// Create a state snapshot from an audio source
        /// </summary>
        public static MusicState Capture(
            AudioSource source,
            MusicIdentifier? track,
            MusicEventType? eventType = null,
            List<MusicIdentifier> sequence = null,
            int sequenceIndex = 0)
        {
            var state = new MusicState
            {
                currentTrack = track,
                trackTime = source != null && source.isPlaying ? source.time : 0f,
                wasPlaying = source != null && source.isPlaying,
                eventType = eventType,
                sequence = sequence != null ? new List<MusicIdentifier>(sequence) : null,
                sequenceIndex = sequenceIndex,
                isSequence = sequence != null && sequence.Count > 0
            };

            return state;
        }

        /// <summary>
        /// Check if this state has valid data
        /// </summary>
        public bool IsValid()
        {
            return currentTrack.HasValue || (isSequence && sequence != null && sequence.Count > 0);
        }

        /// <summary>
        /// Get a description of this state for debugging
        /// </summary>
        public override string ToString()
        {
            if (isSequence)
            {
                return $"Sequence[{sequenceIndex}/{sequence?.Count ?? 0}] Track: {currentTrack} Time: {trackTime:F1}s";
            }
            else
            {
                return $"Event: {eventType} Track: {currentTrack} Time: {trackTime:F1}s Playing: {wasPlaying}";
            }
        }
    }
}
