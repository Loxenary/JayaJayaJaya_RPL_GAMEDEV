using DG.Tweening;
using UnityEngine;

public class PlayGameTriggerModified : PlayGameTrigger
{
    [SerializeField] CanvasGroup currentCg;
    [SerializeField] CanvasGroup targetCg;
    [SerializeField] PlayNarrative playNarrative;
    protected override void Trigger()
    {
        // The old menu-opening narrative now lives as Story 1's first-visit intro
        // (StoryIntroController); Play goes straight to the world selection map.
        if (currentCg != null)
        {
            currentCg.interactable = false;
            currentCg.DOFade(0, .5f).OnComplete(() =>
            {
                _ = ServiceLocator.Get<FlowManager>().OpenSelection();
            });
        }
        else
        {
            _ = ServiceLocator.Get<FlowManager>().OpenSelection();
        }
    }
}
