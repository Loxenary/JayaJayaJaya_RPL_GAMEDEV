using UnityEngine;

public class InteractableFirstPuzzle : InteractablePuzzle
{
    public override void InteractObject()
    {
        base.InteractObject();
        // Close

        EventBus.Publish(new FirstPuzzleEvent());
    }
}