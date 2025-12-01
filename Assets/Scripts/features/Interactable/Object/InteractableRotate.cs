using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class InteractableRotate : Interactable
{
    [SerializeField] float timeRotate = 1.5f;
    [SerializeField] Transform rootObject;
    [SerializeField] Vector3 targetRotation;


    public UnityEvent OnDoneRotate;
    public override void InteractObject()
    {
        base.InteractObject();

        rootObject.DOLocalRotate(targetRotation, timeRotate).OnComplete(() => {
            OnDoneRotate?.Invoke();
        });
    }
}
