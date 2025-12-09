using System;
using UnityEngine;

public class InteractableGuide : Interactable
{
    [SerializeField] private GuideData guideData;

    public override void InteractObject()
    {
        base.InteractObject();

        // Notify that a guide interactable was interacted with
        EventBus.Publish(new GuideInteracted()
        {
            guideData = guideData
        });
    }

    public GuideData GuideData => guideData;
}
