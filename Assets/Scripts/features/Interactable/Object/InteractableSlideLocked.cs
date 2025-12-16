using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class InteractableSlideLocked : InteractableLockedDoor
{

    public override void InteractObject()
    {
        if (isUnlock)
        {
            isInteract = true;
            Slide();
        }
        else
        {
            OnWrongKey();
        }
    }
    void Slide()
    {
        rootObject.DOLocalMove(targetRotation, timeRotate).OnComplete(() => {
            OnDoneRotate?.Invoke();
        });
    }
}
