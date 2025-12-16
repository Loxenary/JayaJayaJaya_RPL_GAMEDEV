using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class InteractableHintObject : InteractableRotate
{
  [Header("Interactable Hint Object Section")]
  public UnityEvent OnSecondInteract;

  public delegate void CheckIsFirstPuzzle();
  public static event CheckIsFirstPuzzle onCheckIsFirst;

  //[SerializeField]
  //float delayCollider = 2.5f;

  //public UnityEvent onDelayedCall;
  public override void InteractObject()
  {
    if (wait) return;

    //onInteract?.Invoke();
    if (isInteract)
    {



            OnSecondInteract?.Invoke();
      return;
    }
    else
    {
            isInteract = true;
            onCheckIsFirst?.Invoke();
            onInteract?.Invoke();
            wait = true;
            DoRotate(targetRotation);
        }

 

    //DOTween.Sequence().SetDelay(delayCollider).OnComplete(() => {
    //    onDelayedCall?.Invoke();
    //});
  }

  // Use base rotate logic (interruptible) to open


}
