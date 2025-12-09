using UnityEngine;
using UnityEngine.Events;

public class InteractableDoor : InteractableRotate
{
    [Header("Interactable Door Section")]
    public UnityAction OnRoolback;

    public override void InteractObject()
    {
        base.InteractObject();
    }
}
