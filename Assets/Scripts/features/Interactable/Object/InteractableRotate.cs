using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class InteractableRotate : Interactable
{
  [SerializeField] protected float timeRotate = 1.5f;
  [SerializeField] protected Transform rootObject;
  [SerializeField] protected Vector3 targetRotation;
  [SerializeField] protected Transform playerReference; // optional; falls back to Camera.main

  public UnityEvent OnDoneRotate;

  private Tweener rotateTween;

  public override void InteractObject()
  {
    // Cancel any ongoing tween so interaction can be interrupted/reversed
    if (rotateTween != null && rotateTween.IsActive())
    {
      rotateTween.Kill();
    }

    onInteract?.Invoke();

    bool wantOpen = !isInteract;
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

    float signed = Mathf.Sign(Vector3.SignedAngle(rootObject.forward, toPlayer, Vector3.up));
    // signed >=0 : player in front (relative to forward); open away by reversing sign
    float pushSign = signed >= 0f ? -1f : 1f;

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
        });
  }
}
