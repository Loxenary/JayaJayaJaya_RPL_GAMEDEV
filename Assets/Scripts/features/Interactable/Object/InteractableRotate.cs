using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class InteractableRotate : Interactable
{
    [SerializeField] protected float timeRotate = 1.5f;
    [SerializeField] protected Transform rootObject;
    [SerializeField] protected Vector3 targetRotation;


    public UnityEvent OnDoneRotate;


    [ReadOnly]
    [SerializeField] protected bool wait;
    public override void InteractObject()
    {
        if (wait) return;

        wait = true;
        
        onInteract?.Invoke();
        if (isInteract)
            DoRotate(Vector3.zero);
        else
            DoRotate(targetRotation);


        isInteract = !isInteract;

    }
    protected void DoRotate(Vector3 target)
    {
        rootObject.DOLocalRotate(target, timeRotate).OnComplete(() => {
            OnDoneRotate?.Invoke();
            wait = false;
        });
    }
}