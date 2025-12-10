using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using static InteractableLockedDoor;

public class InteractableLockedDoor : InteractableDoor, IHighlight
{
    [Header("Locked Door Section")]
    [SerializeField] int keyID;
    [SerializeField] HiglightObjectPlus highlight;
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
        {
            base.InteractObject();
            if (highlight != null)
            {
                highlight.UnHighlight();
                highlight.SetDisable();
            }

        }
        else
        {
            OnWrongKeys?.Invoke();
            onWrongKey?.Invoke();
            Debug.Log("Masih Terkunci");
        }
    }


    public void Highlight()
    {
        if (isInteract) return;
        if (isUnlock)
        {
            highlight.Highlight();
        }
    }

    public void UnHighlight()
    {
        if (isInteract) return;
        if (isUnlock)
        {
            highlight.UnHighlight();
        }
    }
}
