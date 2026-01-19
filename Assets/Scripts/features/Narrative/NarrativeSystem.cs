using System.Collections.Generic;
using UnityEngine;

public class NarrativeSystem : ServiceBase<NarrativeSystem>, IRestartable
{
    // Call This every time an item is taken
    public struct ResetNarrativeTimer
    {

    }
    struct MappingJournal
    {
        public string Id;
        public int Number;
    }
    [Header("Configuration")]

    [SerializeField] private JournalDatabaseConfig journalDatabaseConfig;

    private int currentTrackedCollectible = 0;
    private Coroutine currentTimerCoroutine;
    private readonly HashSet<string> interactedPuzzle = new();
    private readonly HashSet<GuideData> interactedGuides = new();

    //List<MappingJournal> mapping = new List<MappingJournal>();
    Dictionary<string, int> mapping = new Dictionary<string, int>();

    private void OnEnable()
    {
        EventBus.Subscribe<PuzzleInteracted>(OnPuzzleInteracted);
        EventBus.Subscribe<GuideInteracted>(OnGuideInteracted);
        EventBus.Subscribe<ResetNarrativeTimer>(ResetPuzzleTimer);
        RestartManager.Register(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PuzzleInteracted>(OnPuzzleInteracted);
        EventBus.Unsubscribe<GuideInteracted>(OnGuideInteracted);
        EventBus.Unsubscribe<ResetNarrativeTimer>(ResetPuzzleTimer);
        RestartManager.Unregister(this);
    }

    protected override void Awake()
    {
        base.Awake();        
    }

    // Called when player interacts with a puzzle (but hasn't collected it yet)
    private void OnPuzzleInteracted(PuzzleInteracted evt)
    {
        if (!interactedPuzzle.Contains(evt.puzzleId))
        {
            interactedPuzzle.Add(evt.puzzleId);

            mapping.Add(evt.puzzleId, currentTrackedCollectible);
            currentTrackedCollectible++;
            EventBus.Publish(new InteractedPuzzleCount()
            {
                puzzleCount = currentTrackedCollectible
            });
        }
        HandleJournal(mapping[evt.puzzleId]);
    }

    // Called when player interacts with a guide interactable
    private void OnGuideInteracted(GuideInteracted evt)
    {
        // Handle no guide exists
        if (interactedGuides.Contains(evt.guideData))
        {
            Debug.LogError("Guide that are not exist is called", evt.guideData);
            return;
        }

        // Check if this guide has already been interacted wit    
        interactedGuides.Add(evt.guideData);
        GuideTimerCoroutine(evt.guideData);
    }

    private void GuideTimerCoroutine(GuideData guideData)
    {
        // Stop any existing timer
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
        }

        // Show the dialog immediately for the new nteraction
        EventBus.Publish(new DialogNarrativeUI.OpenDialogNarrtiveUI
        {
            content = guideData.Content
        });
    }

    private void HandleJournal(int key)
    {
        EventBus.Publish(new JournalUI.OpenJournalUI()
        {
            content = journalDatabaseConfig.Journals[key]
            //content = journalDatabaseConfig.Journals[currentTrackedCollectible]
        });
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
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
            currentTimerCoroutine = null;
        }
        interactedPuzzle.Clear();
        interactedGuides.Clear();
    }
}