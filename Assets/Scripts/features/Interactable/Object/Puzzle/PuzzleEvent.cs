using System;
using UnityEngine;

public class PuzzleEvent : MonoBehaviour
{
    [SerializeField]
    private string _randomGUID = Guid.NewGuid().ToString();

    [ContextMenu("Force Open Journal")]
    public void OpenJournal()
    {
        // Notify that puzzle was interacted with (for narrative timer)
        EventBus.Publish(new PuzzleInteracted()
        {
            puzzleId = _randomGUID
        });

        //// Notify that puzzle was collected
        //EventBus.Publish(type);
    }

}
