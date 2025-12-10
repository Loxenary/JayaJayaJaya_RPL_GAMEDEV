using UnityEngine;
using Ambience;
using EnemyAI;

/// <summary>
/// Listens to Enemy Angry System events and triggers appropriate ambience conditions.
/// Can be extended to listen to other game systems as needed.
/// </summary>
public class AmbienceListener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AmbienceProvider ambienceProvider;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // Subscribe to Enemy Angry System
        EnemyAngrySystem.OnGlobalAngryLevelChanged += OnEnemyLevelChanged;

        // Can subscribe to other game events here
        EventBus.Subscribe<InteractedPuzzleCount>(OnPuzzleCollected);

        Log("Subscribed to game events");
    }

    private void UnsubscribeFromEvents()
    {
        EnemyAngrySystem.OnGlobalAngryLevelChanged -= OnEnemyLevelChanged;

        EventBus.Unsubscribe<InteractedPuzzleCount>(OnPuzzleCollected);

        Log("Unsubscribed from game events");
    }

    private void OnEnemyLevelChanged(EnemyLevel level)
    {
        if (ambienceProvider == null) return;

        // Map EnemyLevel enum to ConditionType
        ConditionType condition = level switch
        {
            EnemyLevel.FIRST => ConditionType.NoniLevel1,
            EnemyLevel.SECOND => ConditionType.NoniLevel2,
            EnemyLevel.THIRD => ConditionType.NoniLevel3,
            EnemyLevel.FOURTH => ConditionType.NoniLevel4,
            _ => ConditionType.None
        };

        Log($"Enemy level changed to {level} → Condition: {condition}");

        // Clear all previous Noni level conditions
        ambienceProvider.DeactivateCondition(ConditionType.NoniLevel1);
        ambienceProvider.DeactivateCondition(ConditionType.NoniLevel2);
        ambienceProvider.DeactivateCondition(ConditionType.NoniLevel3);
        ambienceProvider.DeactivateCondition(ConditionType.NoniLevel4);

        // Activate the new Noni level condition
        if (condition != ConditionType.None)
        {
            ambienceProvider.ActivateCondition(condition);
        }
    }

    private void OnPuzzleCollected(InteractedPuzzleCount evt)
    {
        Log($"Puzzle collected: {evt.puzzleCount} total");
        // Can trigger specific music based on puzzle count if needed
        // For example: if (evt.puzzleCount == 5) ambienceProvider.PlayMusicDirect(MusicIdentifier.Opening);
    }

    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[AmbienceListener] {message}");
        }
    }
}
