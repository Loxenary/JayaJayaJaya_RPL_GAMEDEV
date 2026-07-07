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

            // Indexer, bukan Add — puzzleId lama tidak boleh melempar ArgumentException
            mapping[evt.puzzleId] = currentTrackedCollectible;
            currentTrackedCollectible++;
            EventBus.Publish(new InteractedPuzzleCount()
            {
                puzzleCount = currentTrackedCollectible
            });

            // Journal hanya terbuka pada interaksi pertama puzzle ini,
            // bukan setiap kali pemain menekan interact ulang.
            HandleJournal(mapping[evt.puzzleId]);
        }
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
        ShowGuideDialog(evt.guideData);
    }

    private void ShowGuideDialog(GuideData guideData)
    {
        // Show the dialog immediately for the new interaction
        EventBus.Publish(new DialogNarrativeUI.OpenDialogNarrtiveUI
        {
            content = guideData.Content
        });
    }

    private void HandleJournal(int key)
    {
        // Journals menghasilkan array baru tiap akses — cache sekali
        var journals = journalDatabaseConfig != null ? journalDatabaseConfig.Journals : null;

        if (journals == null || key < 0 || key >= journals.Length)
        {
            Debug.LogError($"[NarrativeSystem] Journal index {key} di luar batas (jumlah journal: {journals?.Length ?? 0}). Tambah entri di JournalDatabaseConfig atau periksa alur puzzle.", this);
            return;
        }

        EventBus.Publish(new JournalUI.OpenJournalUI()
        {
            content = journals[key]
        });
    }


    private void ResetPuzzleTimer(ResetNarrativeTimer evt)
    {
        currentTrackedCollectible = 0;
        interactedPuzzle.Clear();
        // mapping WAJIB ikut di-reset — kalau tidak, interaksi puzzle yang sama
        // setelah restart menulis ulang key lama dan alur narasi rusak (B-02).
        mapping.Clear();
    }

    public void Restart()
    {
        ResetPuzzleTimer(new());
        interactedGuides.Clear();
    }
}