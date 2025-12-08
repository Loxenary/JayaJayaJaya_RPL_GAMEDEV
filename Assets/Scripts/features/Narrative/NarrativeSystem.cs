using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class NarrativeSystem : ServiceBase<NarrativeSystem>
{
    // Call This every time an item is taken
    public struct ResetNarrativeTimer
    {

    }

    // Event to signal puzzle interaction (not collected yet)
    public struct PuzzleInteracted
    {

    }

    [Header("Configuration")]
    [SerializeField] private NarrativeTimerConfig narrativeTimerConfig;

    private int currentTrackedCollectible = 0;
    private Coroutine currentTimerCoroutine;
    private readonly HashSet<int> interactedPuzzleIndices = new();

    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleType>(ListenToPuzzleCollected);
        EventBus.Subscribe<PuzzleInteracted>(OnPuzzleInteracted);
        EventBus.Subscribe<ResetNarrativeTimer>(ResetPuzzleTimer);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleType>(ListenToPuzzleCollected);
        EventBus.Unsubscribe<PuzzleInteracted>(OnPuzzleInteracted);
        EventBus.Unsubscribe<ResetNarrativeTimer>(ResetPuzzleTimer);
    }

    protected override void Awake()
    {
        base.Awake();
        Assert.IsNotNull(narrativeTimerConfig, "NarrativeTimerConfig is not assigned in NarrativeSystem");
    }

    // Called when player actually collects a puzzle piece
    private void ListenToPuzzleCollected(CollectibleType type)
    {
        if (type == CollectibleType.Key) return;

        // Stop any running timer since the puzzle was collected
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
            currentTimerCoroutine = null;
        }

        // Move to next puzzle tracking
        currentTrackedCollectible++;
        interactedPuzzleIndices.Clear();
    }

    // Called when player interacts with a puzzle (but hasn't collected it yet)
    private void OnPuzzleInteracted(PuzzleInteracted evt)
    {
        // Check if this is a new interaction for the current puzzle index
        if (!interactedPuzzleIndices.Contains(currentTrackedCollectible))
        {
            interactedPuzzleIndices.Add(currentTrackedCollectible);

            // Stop any existing timer
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
            }

            // Start new timer
            currentTimerCoroutine = StartCoroutine(StartPuzzleTimerCoroutine());
        }
    }

    private IEnumerator StartPuzzleTimerCoroutine()
    {
        if (narrativeTimerConfig.TimerContentRecords == null ||
            currentTrackedCollectible >= narrativeTimerConfig.TimerContentRecords.Length)
        {
            Debug.LogWarning($"No timer configuration found for puzzle index {currentTrackedCollectible}");
            yield break;
        }

        TimerContentRecord currentRecord = narrativeTimerConfig.TimerContentRecords[currentTrackedCollectible];

        Debug.Log($"Starting narrative timer for {currentRecord.timer} seconds");

        // Wait for the specified time
        yield return new WaitForSeconds(currentRecord.timer);

        // Timer reached 0, show the narrative hint
        Debug.Log($"Timer expired! Showing narrative hint: {currentRecord.content}");

        EventBus.Publish(new DialogNarrativeUI.OpenDialogNarrtiveUI
        {
            content = currentRecord.content
        });

        currentTimerCoroutine = null;
    }

    private void ResetPuzzleTimer(ResetNarrativeTimer evt)
    {
        // Stop any running timer
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
            currentTimerCoroutine = null;
        }

        currentTrackedCollectible = 0;
        interactedPuzzleIndices.Clear();
    }
}