using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class InteractablePos : Interactable
{
    [SerializeField] InteractableCollectible prefabCollectible;

    [SerializeField] float timeMove = 1.5f;
    [SerializeField] Transform targetObject;
    [SerializeField] Vector3 targetPosition;

    public UnityEvent OnDoneMoveObject;
    public override void InteractObject()
    {
        base.InteractObject();

        targetObject.DOLocalMove(targetPosition, timeMove).OnComplete(() => {
            OnDoneMoveObject?.Invoke();
        });
    }
}
