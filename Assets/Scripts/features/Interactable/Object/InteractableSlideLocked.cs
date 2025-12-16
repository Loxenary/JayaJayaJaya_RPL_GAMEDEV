using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class InteractableSlideLocked : InteractableLockedDoor
{
    //public override bool IsInteractable => true;

    //[Header("Slide Settings")]
    //[SerializeField] private float timeMove = 1.25f;
    //[SerializeField] private Transform rootObject;
    //[SerializeField] private Vector3 slideOffset = new Vector3(0.6f, 0f, 0f);
    //[SerializeField] private bool useLocalSpace = true;

    //public UnityEvent OnDoneSlide;

    //private Tweener moveTween;
    //private Vector3 closedPosition;
    //private Vector3 openPosition;
    //private bool targetIsOpen;
    //private bool initialized;

    //private void Awake()
    //{
    //    EnsureInitialized();
    //}

    //private void OnDisable()
    //{
    //    moveTween?.Kill();
    //}

    //public override void InteractObject()
    //{
    //    EnsureInitialized();

    //    // Cancel any ongoing tween so interaction can be interrupted/reversed
    //    if (moveTween != null && moveTween.IsActive())
    //    {
    //        moveTween.Kill();
    //    }

    //    onInteract?.Invoke();

    //    // Toggle desired state regardless of current tween progress
    //    bool wantOpen = !targetIsOpen;
    //    targetIsOpen = wantOpen;

    //    Vector3 target = wantOpen ? openPosition : closedPosition;
    //    DoMove(target, wantOpen);
    //}

    //private void EnsureInitialized()
    //{
    //    if (rootObject == null)
    //        rootObject = transform;

    //    if (initialized)
    //        return;

    //    closedPosition = useLocalSpace ? rootObject.localPosition : rootObject.position;
    //    openPosition = closedPosition + slideOffset;
    //    targetIsOpen = false;
    //    initialized = true;
    //}

    //private void DoMove(Vector3 target, bool openState)
    //{
    //    moveTween = useLocalSpace
    //        ? rootObject.DOLocalMove(target, timeMove)
    //        : rootObject.DOMove(target, timeMove);

    //    moveTween
    //        .SetUpdate(true)
    //        .OnComplete(() =>
    //        {
    //            isInteract = openState;
    //            OnDoneSlide?.Invoke();
    //        });
    //}
}
