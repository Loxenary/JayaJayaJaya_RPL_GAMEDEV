using UnityEngine;

public class InteractableKey : Interactable
{
    [Header("Key Section")]
    [SerializeField] int keyId;


    public delegate void OnFoundKey(int id);
    public static event OnFoundKey onFoundKey;
    public override void InteractObject()
    {
        base.InteractObject();


        onFoundKey?.Invoke(keyId);
    }
}
