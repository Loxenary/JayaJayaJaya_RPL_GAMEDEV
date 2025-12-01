using UnityEngine;

public class InteractableLockedDoor : InteractableRotate
{
    [Header("Locked Door Section")]
    [SerializeField] int keyID;
    [ReadOnly]
    [SerializeField] bool isUnlock;

    private void OnEnable()
    {
        InteractableKey.onFoundKey += OnFoundKey;
    }
    private void OnDisable()
    {
        InteractableKey.onFoundKey -= OnFoundKey;        
    }
    private void OnFoundKey(int id)
    {
        if(id  == keyID)
            isUnlock = true;
    }

    public override void InteractObject()
    {
        if (isUnlock)
            base.InteractObject();
        else
            Debug.Log("Masih Terkunci");
    }
}
