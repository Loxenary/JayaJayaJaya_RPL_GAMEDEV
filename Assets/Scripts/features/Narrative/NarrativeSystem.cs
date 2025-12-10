using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class NarrativeSystem : ServiceBase<NarrativeSystem>, IRestartable
{
    // Call This every time an item is taken
    public struct ResetNarrativeTimer
    {

    }

    [Header("Configuration")]

    [SerializeField] private JournalDatabaseConfig journalDatabaseConfig;
    [SerializeField] private int guideTimerDuration = 25;


    private int currentTrackedCollectible = 0;
    private Coroutine currentTimerCoroutine;
    private readonly HashSet<string> interactedPuzzle = new();
    private readonly HashSet<GuideData> interactedGuides = new();

    private GuideData lastGuideData = null;

    private void OnEnable()
    {
        EventBus.Subscribe<PuzzleInteracted>(OnPuzzleInteracted);
        EventBus.Subscribe<GuideInteracted>(OnGuideInteracted);
        EventBus.Subscribe<ResetNarrativeTimer>(ResetPuzzleTimer);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PuzzleInteracted>(OnPuzzleInteracted);
        EventBus.Unsubscribe<GuideInteracted>(OnGuideInteracted);
        EventBus.Unsubscribe<ResetNarrativeTimer>(ResetPuzzleTimer);
    }

    protected override void Awake()
    {
        base.Awake();
        RestartManager.Register(this);
    }

    // Called when player interacts with a puzzle (but hasn't collected it yet)
    private void OnPuzzleInteracted(PuzzleInteracted evt)
    {
        HandleJournal();

        if (!interactedPuzzle.Contains(evt.puzzleId))
        {
            interactedPuzzle.Add(evt.puzzleId);
            currentTrackedCollectible++;
            EventBus.Publish(new InteractedPuzzleCount()
            {
                puzzleCount = currentTrackedCollectible
            });
        }
    }

    // Called when player interacts with a guide interactable
    private void OnGuideInteracted(GuideInteracted evt)
    {

        // Check if this guide has already been interacted with
        if (!interactedGuides.Contains(evt.guideData))
        {
            interactedGuides.Add(evt.guideData);
            lastGuideData = evt.guideData;
            GuideTimerCoroutine(evt.guideData);
        }
    }

    private void GuideTimerCoroutine(GuideData guideData)
    {
        // Stop any existing timer
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
        }

        // Show the dialog immediately for the new interaction
        EventBus.Publish(new DialogNarrativeUI.OpenDialogNarrtiveUI
        {
            content = guideData.Content
        });

        // Start new timer for this guide (will loop)
        currentTimerCoroutine = StartCoroutine(StartGuideTimerCoroutine());
    }

    private void HandleJournal()
    {
        EventBus.Publish(new JournalUI.OpenJournalUI()
        {
            content = journalDatabaseConfig.Journals[currentTrackedCollectible]
        });
    }

    private IEnumerator StartGuideTimerCoroutine()
    {
        while (true)
        {
            // Wait for 25 seconds (20-30 range)
            yield return new WaitForSeconds(guideTimerDuration);

            // If we have a last guide, show its content again
            if (lastGuideData != null)
            {
                Debug.Log($"Timer expired! Showing last guide narrative again: {lastGuideData.Content}");
                EventBus.Publish(new DialogNarrativeUI.OpenDialogNarrtiveUI
                {
                    content = lastGuideData.Content
                });
            }
        }
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
        interactedPuzzle.Clear();
    }

    public void Restart()
    {
        ResetPuzzleTimer(new());
        currentTrackedCollectible = 0;
        interactedPuzzle.Clear();
        interactedGuides.Clear();
        lastGuideData = null;
    }
}