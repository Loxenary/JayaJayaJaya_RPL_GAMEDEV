using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class InteractableRotate : Interactable
{
  public override bool IsInteractable => true;
  [SerializeField] protected float timeRotate = 1.5f;
  [SerializeField] protected Transform rootObject;
  [SerializeField] protected Vector3 targetRotation;
  [SerializeField] protected Transform playerReference; // optional; falls back to Camera.main

  protected bool wait = false;

  public UnityEvent OnDoneRotate;

  private Tweener rotateTween;
  private bool targetIsOpen = false; // tracks desired end state even while tweening

  public override void InteractObject()
  {
    Rotate();
  }

  protected void Rotate()
  {
    // Cancel any ongoing tween so interaction can be interrupted/reversed
    if (rotateTween != null && rotateTween.IsActive())
    {
      rotateTween.Kill();
    }

    onInteract?.Invoke();

    // Toggle desired state regardless of current tween progress
    bool wantOpen = !targetIsOpen;
    targetIsOpen = wantOpen;

    Vector3 target = wantOpen ? GetDirectionalTargetRotation() : Vector3.zero;

    DoRotate(target, wantOpen);
  }

  private Vector3 GetDirectionalTargetRotation()
  {
    // Determine sign based on player position relative to door forward (push away from player)
    Transform player = playerReference != null ? playerReference : Camera.main != null ? Camera.main.transform : null;
    if (player == null || rootObject == null)
    {
      return targetRotation; // fallback
    }

    Vector3 toPlayer = (player.position - rootObject.position);
    toPlayer.y = 0f;
    if (toPlayer.sqrMagnitude < 0.0001f)
      return targetRotation;

    float facingDot = Vector3.Dot(rootObject.forward, toPlayer);
    // Player in front of forward vector => pushSign negative so the door swings away
    float pushSign = facingDot >= 0f ? -1f : 1f;

    return new Vector3(targetRotation.x * pushSign, targetRotation.y * pushSign, targetRotation.z * pushSign);
  }

  protected void DoRotate(Vector3 target, bool openState)
  {
    if (rootObject == null)
      rootObject = transform;

    rotateTween = rootObject.DOLocalRotate(target, timeRotate)
        .SetUpdate(true)
        .OnComplete(() =>
        {
          isInteract = openState;
          OnDoneRotate?.Invoke();
          wait = false;
        });
  }
  protected void DoRotate(Vector3 target)
  {
    rotateTween = rootObject.DOLocalRotate(target, timeRotate).OnComplete(() =>
    {
      OnDoneRotate?.Invoke();
      wait = false;
    });
  }
}
