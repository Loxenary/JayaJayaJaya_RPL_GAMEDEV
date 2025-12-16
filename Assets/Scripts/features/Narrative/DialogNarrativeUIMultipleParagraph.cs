using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;


[RequireComponent (typeof(PlayNarrative))]
public class DialogNarrativeUIMultipleParagraph : DialogNarrativeUI
{
    [Header("Multiple Paragraph Section")]
    [SerializeField] PlayNarrative narrative;
    [SerializeField] float delayParagraph = 2f;
    [SerializeField] Image blackPanel;

    [SerializeField] UnityEvent OnDoneNarrative;
    public void Logging()
    {
        Debug.Log("Testing");
    }
    protected override void OnDialogFinishedShowing()
    {
        DOVirtual.DelayedCall(delayParagraph, () =>
        {
            if (narrative.IsDone())
            {
                //base.OnDialogFinishedShowing();
                DOVirtual.DelayedCall(delayParagraph + .5f, () => {
                    blackPanel.DOFade(1, .35f).OnComplete(() => {
                        ServiceLocator.Get<FlowManager>().PlayGame();
                    });
                });                
            }
            else
            {
                narrative.TryNarrative();
            }
        });
    }
}
