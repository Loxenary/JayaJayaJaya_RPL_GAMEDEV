using UnityEngine;
using UnityEngine.Events;
using static InteractableLockedDoor;

public class InteractableLockedDoor : InteractableDoor
{
    [Header("Locked Door Section")]
    [SerializeField] int keyID;

#if UNITY_EDITOR
    [ReadOnly]
    [SerializeField] bool isKeyUnlock => isUnlock;
#endif

    bool isUnlock;

    public UnityEvent OnWrongKeys;

    public delegate void WrongKey();
    public static event WrongKey onWrongKey;
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
        if (id == keyID)
            isUnlock = true;
    }

    public override void InteractObject()
    {
        if (isUnlock)
            base.InteractObject();
        else
        {
            OnWrongKeys?.Invoke();
            onWrongKey?.Invoke();
            Debug.Log("Masih Terkunci");
        }
    }
}
