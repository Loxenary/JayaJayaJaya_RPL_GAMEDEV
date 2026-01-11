using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(PlayNarrative))]
public class DialogNarrativeUIMultipleParagraph : DialogNarrativeUI
{
    [Header("Multiple Paragraph Section")]
    [SerializeField] PlayNarrative narrative;
    [SerializeField] float delayParagraph = 2f;
    [SerializeField] Image blackPanel;

    [SerializeField] UnityEvent OnDoneNarrative;

    protected override void OnDialogFinishedShowing()
    {
        // 1. CRITICAL: Reset the typing flag manually. 
        // If we don't do this, the system thinks we are still typing 
        // and will ignore the next TryNarrative() call.
        _isTyping = false;

        // 2. Check if we are using the Narrative System
        if (narrative != null)
        {
            // Use DOTween for the delay between paragraphs
            DOVirtual.DelayedCall(delayParagraph, () =>
            {
                if (narrative.IsDone())
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        blackPanel.DOFade(1, 0.35f).OnComplete(() =>
                        {
                            ServiceLocator.Get<FlowManager>().PlayGame();
                            OnDoneNarrative?.Invoke();
                        });
                    });
                }
                else
                {
                    // --- SCENARIO B: More paragraphs to go ---
                    // Trigger the next chunk of text.
                    // Since we set _isTyping = false above, this call will succeed.
                    _guideDatasQueue.Dequeue();
                    narrative.TryNarrative();
                }
            });
        }
        else
        {
            // --- SCENARIO C: No Narrative Component ---
            // Just behave like a normal dialog box (Wait -> Hide -> Check Queue)
            base.OnDialogFinishedShowing();
        }
    }
}