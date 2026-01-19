
using UnityEngine;

public class PosGuideAfterPuzzle : PosGuide, IRestartable
{
    private bool _isPuzzlePlayed = false;

    private void OnEnable()
    {
        EventBus.Subscribe<FirstPuzzleEvent>(OnPuzzleSolved);
        RestartManager.Register(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<FirstPuzzleEvent>(OnPuzzleSolved);
        RestartManager.Unregister(this);
    }

    private void OnPuzzleSolved(FirstPuzzleEvent evt)
    {
        _isPuzzlePlayed = true;
    }

    protected override void PublishGuide()
    {
        if (_isPuzzlePlayed)
        {
            // Mark as triggered (once only)
            hasTriggered = true;

            // Publish guide event - this will update NarrativeSystem's lastGuideData
            EventBus.Publish(new GuideInteracted
            {
                guideData = guideData,

            });

            Debug.Log($"[PosGuide] Player entered guide zone: {gameObject.name}. Guide content: {guideData.Content}.");
        }
    }

    public void Restart()
    {
        throw new System.NotImplementedException();
    }
}