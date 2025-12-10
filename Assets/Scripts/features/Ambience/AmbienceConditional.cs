using UnityEngine;
using Ambience;

/// <summary>
/// Handles conditional logic for triggering ambience based on player state.
/// Subscribes to player events (sanity, death) and triggers appropriate conditions in AmbienceProvider.
/// </summary>
public class AmbienceConditional : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AmbienceProvider ambienceProvider;

    [Header("Sanity Thresholds")]
    [Tooltip("Sanity threshold for LowSanity condition (normalized 0-1)")]
    [SerializeField][Range(0f, 1f)] private float lowSanityThreshold = 0.3f;
    [Tooltip("Sanity threshold for VeryLowSanity condition (normalized 0-1)")]
    [SerializeField][Range(0f, 1f)] private float veryLowSanityThreshold = 0.15f;

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
        PlayerAttributes.onSanityUpdate += OnSanityChanged;
        PlayerAttributes.onPlayerDead += OnPlayerDead;

        EventBus.Subscribe<EndGame.OpenEndGameUI>(OnEndGame);

        Log("Subscribed to player events");
    }

    private void UnsubscribeFromEvents()
    {
        PlayerAttributes.onSanityUpdate -= OnSanityChanged;
        PlayerAttributes.onPlayerDead -= OnPlayerDead;

        EventBus.Unsubscribe<EndGame.OpenEndGameUI>(OnEndGame);

        Log("Unsubscribed from player events");
    }

    private void OnSanityChanged(float normalized)
    {
        if (ambienceProvider == null) return;

        // normalized: 1.0 = full sanity, 0.0 = dead
        bool isVeryLowSanity = normalized < veryLowSanityThreshold;
        bool isLowSanity = normalized < lowSanityThreshold && !isVeryLowSanity;

        Log($"Sanity changed: {normalized:F2} (LowSanity: {isLowSanity}, VeryLowSanity: {isVeryLowSanity})");

        // Handle VeryLowSanity
        if (isVeryLowSanity)
        {
            ambienceProvider.ActivateCondition(ConditionType.VeryLowSanity);
            ambienceProvider.DeactivateCondition(ConditionType.LowSanity);
        }
        // Handle LowSanity
        else if (isLowSanity)
        {
            ambienceProvider.ActivateCondition(ConditionType.LowSanity);
            ambienceProvider.DeactivateCondition(ConditionType.VeryLowSanity);
        }
        // Normal sanity
        else
        {
            ambienceProvider.DeactivateCondition(ConditionType.LowSanity);
            ambienceProvider.DeactivateCondition(ConditionType.VeryLowSanity);
        }
    }

    private void OnPlayerDead()
    {
        if (ambienceProvider == null) return;

        Log("Player died - activating death condition");
        ambienceProvider.ActivateCondition(ConditionType.PlayerDead);
    }

    private void OnEndGame(EndGame.OpenEndGameUI evt)
    {
        if (ambienceProvider == null) return;

        Log($"End game triggered: {evt.content}");
        ambienceProvider.ActivateCondition(ConditionType.Ending);
    }

    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[AmbienceConditional] {message}");
        }
    }
}
