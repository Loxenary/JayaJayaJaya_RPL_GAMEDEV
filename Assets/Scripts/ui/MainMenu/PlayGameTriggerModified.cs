using DG.Tweening;
using UnityEngine;

public class PlayGameTriggerModified : PlayGameTrigger
{
    [SerializeField] CanvasGroup currentCg;
    [SerializeField] CanvasGroup targetCg;
    [SerializeField] PlayNarrative playNarrative;
    protected override void Trigger()
    {
        currentCg.DOFade(0, .5f).OnComplete(() => {
            targetCg.gameObject.SetActive(true);
            targetCg.DOFade(1, .35f).OnComplete(() => { 
                playNarrative.TryNarrative();
            });
        });
    }
}
