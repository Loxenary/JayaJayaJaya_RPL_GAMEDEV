using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class InteractableRotate : Interactable
{
    [SerializeField] float timeRotate = 1.5f;
    [SerializeField] Transform rootObject;
    [SerializeField] Vector3 targetRotation;


    public UnityEvent OnDoneRotate;


    [ReadOnly]
    [SerializeField] bool wait;
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
    void DoRotate(Vector3 target)
    {
        rootObject.DOLocalRotate(target, timeRotate).OnComplete(() => {
            OnDoneRotate?.Invoke();
            wait = false;
        });
    }
}