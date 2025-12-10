using UnityEngine;

public class InteractableFirstPuzzle : InteractablePuzzle, IRestartable
{

    private bool _isFirstPuzzle = true;
    public override void InteractObject()
    {
        base.InteractObject();
        // Close

    }

    public void TriggerFirstPuzzle()
    {
        if (_isFirstPuzzle)
        {
            _isFirstPuzzle = false;
            EventBus.Publish(new FirstPuzzleEvent());
        }

    }

    private void Awake()
    {
        RestartManager.Register(this);
    }

    public void Restart()
    {
        _isFirstPuzzle = true;
    }
}