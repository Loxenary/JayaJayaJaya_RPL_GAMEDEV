using UnityEngine;
using UnityEngine.Events;

public class InteractableHintObject : InteractableRotate
{
    [Header("Interactable Hint Object Section")]
    public UnityEvent OnSecondInteract;

    public override void InteractObject()
    {

        if (wait) return;


        onInteract?.Invoke();
        if (isInteract)
        {
            OnSecondInteract?.Invoke();
            return;
        }

        else
        {
            wait = true;
            DoRotate(targetRotation);
        }

        isInteract = true;

    }
}
