using UnityEngine;
using UnityEngine.Events;

public class InteractableHintObject : InteractableRotate
{
  [Header("Interactable Hint Object Section")]
  public UnityEvent OnSecondInteract;

  public override void InteractObject()
  {
    // If already opened once, trigger secondary action only
    if (isInteract)
    {
      OnSecondInteract?.Invoke();
      return;
    }

    // Use base rotate logic (interruptible) to open
    base.InteractObject();
  }
}
