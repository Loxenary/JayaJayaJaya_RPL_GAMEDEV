using UnityEngine;

public class InteractableCollectible : Interactable
{
    [SerializeField] CollectibleType type;

    public override void InteractObject()
    {
        base.InteractObject();

        EventBus.Publish(type);

        gameObject.SetActive(false);
    }
}
