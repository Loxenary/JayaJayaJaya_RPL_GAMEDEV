using UnityEngine;

/// <summary>
/// Lives in the gameplay UI scene. On scene start, checks whether the current
/// story's intro has been seen; if not, freezes the player and plays the intro
/// via StoryIntroPanel, then records it as seen.
/// Safe with editor direct-play: FlowManager.CurrentStory falls back to story 1.
/// </summary>
public class StoryIntroController : MonoBehaviour
{
    [SerializeField] private StoryIntroPanel introPanel;
    [Tooltip("Found automatically when left empty (the player lives in the gameplay scene).")]
    [SerializeField] private PlayerController player;

    private void Start()
    {
        if (introPanel == null)
        {
            Debug.LogWarning("[StoryIntroController] No StoryIntroPanel assigned.");
            return;
        }

        var flowManager = ServiceLocator.Get<FlowManager>();
        if (flowManager == null)
        {
            return;
        }

        var story = flowManager.CurrentStory;
        if (story == null || story.IntroParagraphs == null || story.IntroParagraphs.Length == 0)
        {
            return;
        }

        var progress = flowManager.GetProgress(story);
        if (progress == null || progress.hasSeenIntro)
        {
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
        player?.SetFrozen(true);

        introPanel.Play(story.IntroParagraphs, onDone: () =>
        {
            player?.SetFrozen(false);
            _ = flowManager.MarkIntroSeen(story);
        });
    }
}
