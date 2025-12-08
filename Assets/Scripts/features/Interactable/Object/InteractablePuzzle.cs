using UnityEngine;

public class InteractablePuzzle : Interactable
{

    private CollectibleType type => CollectibleType.Puzzle;

    public override void InteractObject()
    {
        base.InteractObject();

        // Notify that puzzle was interacted with (for narrative timer)
        EventBus.Publish(new NarrativeSystem.PuzzleInteracted());

        // Notify that puzzle was collected
        EventBus.Publish(type);
    }
}