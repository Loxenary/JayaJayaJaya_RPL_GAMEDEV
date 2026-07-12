using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class to handle the game flow: world selection map, entering/leaving stories,
/// and story progression persistence (intro seen, completed, achieved ending).
/// </summary>
public class FlowManager : ServiceBase<FlowManager>, IInitializableService
{
<<<<<<< Updated upstream
    // Entry point tetap void agar kompatibel UnityEvent/button;
    // exception async tertangkap via Forget (B-15).
    public void PlayGame() => PlayGameAsync().Forget(nameof(PlayGame));

    private async Task PlayGameAsync()
    {
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
        await ServiceLocator.Get<SceneService>().LoadScene(SceneEnum.IN_GAME, true);
=======
    [Tooltip("Ordered list of all stories. Assign on the Services/FlowManager prefab.")]
    [SerializeField] private StoryDatabase storyDatabase;

    private StoryDefinition _currentStory;
    private StoryProgressSaveData _progress = new();

    public ServicePriority InitializationPriority => ServicePriority.SECONDARY;

    public StoryDatabase StoryDatabase => storyDatabase;

    /// <summary>
    /// Story that is currently being played. Falls back to the first story in the
    /// database so direct-playing a gameplay scene from the editor (SceneBootstrapper)
    /// still gives story context.
    /// </summary>
    public StoryDefinition CurrentStory
    {
        get
        {
            if (_currentStory != null)
            {
                return _currentStory;
            }
            if (storyDatabase != null && storyDatabase.StoriesInOrder.Count > 0)
            {
                return storyDatabase.StoriesInOrder[0];
            }
            return null;
        }
    }

    /// <summary>
    /// Set when returning from a story so the selection scene can start its camera
    /// at that building and animate a zoom-out. Cleared by the selection scene.
    /// </summary>
    public StoryDefinition PendingZoomOutStory { get; private set; }

    private void OnEnable()
    {
        EventBus.Subscribe<StoryCompletedEvent>(OnStoryCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<StoryCompletedEvent>(OnStoryCompleted);
    }

    public async Task Initialize()
    {
        _progress = await SaveLoadManager.LoadAsync<StoryProgressSaveData>() ?? new StoryProgressSaveData();
    }

    /// <summary>
    /// Legacy entry point (old main-menu narrative flow). Plays the first story directly.
    /// New code should go through OpenSelection/PlayStory instead.
    /// </summary>
    public async void PlayGame()
    {
        await PlayStory(CurrentStory);
>>>>>>> Stashed changes
    }

    /// <summary>Opens the world selection map.</summary>
    public async Task OpenSelection()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (!sceneService.HasRecord(SceneEnum.SELECTION))
        {
            // Selection scene not wired yet (e.g. teammate branch without the new prefab record).
            Debug.LogWarning("[FlowManager] No SELECTION scene group registered. Falling back to first story.");
            await PlayStory(CurrentStory);
            return;
        }
        await sceneService.LoadScene(SceneEnum.SELECTION, true);
    }

    /// <summary>Enters a story: loads its gameplay scene group behind a fade.</summary>
    public async Task PlayStory(StoryDefinition story)
    {
        if (story == null || !story.IsPlayable)
        {
            Debug.LogWarning($"[FlowManager] PlayStory called with a null or non-playable story ({(story != null ? story.name : "null")}).");
            return;
        }

        _currentStory = story;
        PendingZoomOutStory = null;

        // Fresh entry from the map: clear any lingering checkpoint from a previous run
        // so PlayerRespawn does not teleport the player away from the spawn point.
        ClearCheckpointState();

        await ServiceLocator.Get<SceneService>().LoadSceneGroup(story.GameplayGroup, true);
    }

    /// <summary>
    /// Returns to the world selection map (walked out the door, or finished the story).
    /// In-building progress is discarded: the gameplay scenes get unloaded and
    /// restartable systems are reset.
    /// </summary>
    public async Task ReturnToSelection()
    {
        PendingZoomOutStory = _currentStory;
        RestartManager.Restart();
        ClearCheckpointState();
        await OpenSelectionOrMenu();
    }

    /// <summary>Reloads the current story's scenes (used after death). Keeps checkpoints.</summary>
    public async Task RestartCurrentStory()
    {
        RestartManager.Restart();
        await ServiceLocator.Get<SceneService>().ReloadScene(true);
    }

    /// <summary>Called by the selection scene once its zoom-out animation has started.</summary>
    public void ClearPendingZoomOut()
    {
        PendingZoomOutStory = null;
    }

    // ---------------------------------------------------------------------
    // Progression
    // ---------------------------------------------------------------------

    /// <summary>The first story is always unlocked; each next one needs the previous completed.</summary>
    public bool IsStoryUnlocked(StoryDefinition story)
    {
        if (storyDatabase == null || story == null)
        {
            return false;
        }

        int index = storyDatabase.IndexOf(story);
        if (index < 0)
        {
            return false;
        }
        if (index == 0)
        {
            return true;
        }

        var previous = storyDatabase.StoriesInOrder[index - 1];
        var previousEntry = previous != null ? _progress.GetEntry(previous.StoryId) : null;
        return previousEntry != null && previousEntry.completed;
    }

    public StoryProgressEntry GetProgress(StoryDefinition story)
    {
        if (story == null)
        {
            return null;
        }
        return _progress.GetOrCreate(story.StoryId);
    }

    public async Task MarkIntroSeen(StoryDefinition story)
    {
        if (story == null)
        {
            return;
        }
        var entry = _progress.GetOrCreate(story.StoryId);
        if (entry.hasSeenIntro)
        {
            return;
        }
        entry.hasSeenIntro = true;
        await SaveLoadManager.SaveAsync(_progress);
    }

    public async Task MarkCompleted(StoryDefinition story, string endingId)
    {
        if (story == null)
        {
            return;
        }
        var entry = _progress.GetOrCreate(story.StoryId);
        entry.completed = true;
        entry.endingId = endingId;
        await SaveLoadManager.SaveAsync(_progress);
    }

    // ---------------------------------------------------------------------

    private async void OnStoryCompleted(StoryCompletedEvent evt)
    {
        await MarkCompleted(CurrentStory, evt.endingId);
    }

    private async Task OpenSelectionOrMenu()
    {
        var sceneService = ServiceLocator.Get<SceneService>();
        if (sceneService.HasRecord(SceneEnum.SELECTION))
        {
            await sceneService.LoadScene(SceneEnum.SELECTION, true);
        }
        else
        {
            await sceneService.LoadScene(SceneEnum.MAIN_MENU, true);
        }
    }

    private void ClearCheckpointState()
    {
        var checkpointManager = ServiceLocator.Get<CheckpointManager>();
        checkpointManager?.Cleanup();
        SaveLoadManager.Delete<CheckpointSaveData>();
    }
}
