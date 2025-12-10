using UnityEngine;

/// <summary>
/// Position-based guide trigger that activates when the player enters a trigger zone.
/// Only triggers once per game session. Updates the NarrativeSystem's lastGuideData.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PosGuide : MonoBehaviour
{
    [Header("Guide Configuration")]
    [Tooltip("The guide data to show when player enters this zone")]
    [SerializeField] protected GuideData guideData;

    [Header("Debug")]
    [SerializeField] protected bool hasTriggered = false;

    private Collider triggerCollider;

    private void Awake()
    {
        SetupTrigger();
    }

    private void OnValidate()
    {
        SetupTrigger();
    }

    private void SetupTrigger()
    {
        // Get collider reference
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }

        // Ensure collider is set as trigger
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
            Debug.LogWarning($"[PosGuide] {gameObject.name}: Collider was not set as trigger. Auto-corrected to trigger.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger once
        if (hasTriggered)
            return;

        // Check if player entered
        if (!other.CompareTag("Player"))
            return;

        // Validate guide data
        if (guideData == null)
        {
            Debug.LogError($"[PosGuide] {gameObject.name}: GuideData is not assigned!", this);
            return;
        }
        PublishGuide();
    }

    protected virtual void PublishGuide()
    {
        // Mark as triggered (once only)
        hasTriggered = true;

        // Publish guide event - this will update NarrativeSystem's lastGuideData
        EventBus.Publish(new GuideInteracted
        {
            guideData = guideData
        });

        Debug.Log($"[PosGuide] Player entered guide zone: {gameObject.name}. Guide content: {guideData.Content}");

    }

    /// <summary>
    /// Reset the trigger state (useful for testing or game restart)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    public bool HasTriggered => hasTriggered;
    public GuideData GuideData => guideData;
}