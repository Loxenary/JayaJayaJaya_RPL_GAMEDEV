using System;
using UnityEngine;

public class InteractablePuzzle : Interactable
{

    private string _randomGUID = Guid.NewGuid().ToString();

    private CollectibleType type => CollectibleType.Puzzle;

    public override void InteractObject()
    {
        base.InteractObject();

        // Notify that puzzle was interacted with (for narrative timer)
        EventBus.Publish(new PuzzleInteracted()
        {
            puzzleId = _randomGUID
        });

        // Notify that puzzle was collected
        EventBus.Publish(type);
    }
}