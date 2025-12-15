using UnityEngine;
using static DialogNarrativeUI;

public class PlayNarrative : MonoBehaviour
{
    
    [SerializeField,TextArea(5,10)] string[] contents;

#if UNITY_EDITOR
    [ReadOnly]
#endif
    [SerializeField] int counterNarrative;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [ContextMenu("Try Narrative")]
    public void TryNarrative()
    {
        if (counterNarrative == contents.Length)
            return;

        EventBus.Publish(new DialogNarrativeUI.OpenDialogNarrtiveUI
        {
            content = contents[counterNarrative]
        });
        counterNarrative++;
    }

    public bool IsDone()
    {
        bool isdone = counterNarrative == contents.Length;

        if (!isdone)
        {
            TryNarrative();
        }
        return isdone;
    }
}
