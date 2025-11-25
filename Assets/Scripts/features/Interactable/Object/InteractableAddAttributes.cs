using UnityEngine;

public class InteractableAddAttributes : Interactable
{
    [Header("Add Attributes Player")]
    [SerializeField] AttributesType type;
    [Range(0,100)]
    [SerializeField] int value;


    public delegate void InteractableAddAttributesDelegate(AttributesType type, int value);
    public static event InteractableAddAttributesDelegate onInteractAddAttribute;


    public override void InteractObject()
    {
        base.InteractObject();


        onInteractAddAttribute?.Invoke(type, value);
    }

}

