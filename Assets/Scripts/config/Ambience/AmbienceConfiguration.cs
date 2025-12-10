using System;
using System.Linq;
using UnityEngine;

namespace Ambience
{
    /// <summary>
    /// ScriptableObject configuration for the Ambience Provider system.
    /// Defines the ambient music sequence and condition-to-music mappings.
    /// </summary>
    [CreateAssetMenu(fileName = "AmbienceConfiguration", menuName = "Config/Ambience/Configuration")]
    public class AmbienceConfiguration : ScriptableObject
    {
        [Header("Ambient Sequence")]
        [Tooltip("Music tracks that play in sequence when no specific conditions are active")]
        [SerializeField] private MusicIdentifier[] ambienceSequence;

        [Header("Condition Mappings")]
        [Tooltip("Map game conditions to music tracks with priority values")]
        [SerializeField] private AmbienceConditionRecord[] conditionRecords;

        /// <summary>
        /// Gets the ambient music sequence array.
        /// </summary>
        public MusicIdentifier[] AmbienceSequence => ambienceSequence;

        /// <summary>
        /// Gets the condition records array.
        /// </summary>
        public AmbienceConditionRecord[] ConditionRecords => conditionRecords;

        /// <summary>
        /// Find a specific condition record by condition type.
        /// </summary>
        public AmbienceConditionRecord? GetConditionRecord(ConditionType condition)
        {
            foreach (var record in conditionRecords)
            {
                if (record.conditionType == condition)
                {
                    return record;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all condition records sorted by priority (highest first).
        /// </summary>
        public AmbienceConditionRecord[] GetSortedByPriority()
        {
            return conditionRecords.OrderByDescending(r => r.priority).ToArray();
        }

        /// <summary>
        /// Check if a specific condition type is configured.
        /// </summary>
        public bool HasCondition(ConditionType condition)
        {
            foreach (var record in conditionRecords)
            {
                if (record.conditionType == condition)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnValidate()
        {
            // Validate ambient sequence
            if (ambienceSequence != null && ambienceSequence.Length == 0)
            {
                Debug.LogWarning($"[{name}] Ambience sequence is empty! At least one music track should be configured.");
            }

            // Validate no duplicate conditions
            if (conditionRecords != null && conditionRecords.Length > 0)
            {
                var duplicates = conditionRecords
                    .GroupBy(r => r.conditionType)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                foreach (var duplicate in duplicates)
                {
                    Debug.LogWarning($"[{name}] Duplicate condition found: {duplicate}. Each condition should only be mapped once.");
                }

                // Validate priorities match enum values for consistency
                foreach (var record in conditionRecords)
                {
                    int enumValue = (int)record.conditionType;
                    if (record.priority != enumValue && enumValue != 0)
                    {
                        Debug.LogWarning($"[{name}] Condition {record.conditionType} has priority {record.priority} but enum value is {enumValue}. Consider matching them for consistency.");
                    }
                }
            }
        }
    }
}
