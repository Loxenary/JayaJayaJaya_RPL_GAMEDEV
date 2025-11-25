using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class Interactable : MonoBehaviour
{
    [Header("Event")]
    public UnityEvent onInteract;


    [Header("Debugging Section")]
    [ReadOnly]
    [SerializeField] protected bool isInteract;
    [ReadOnly]
    [SerializeField] protected BoxCollider collider;

    private void OnDrawGizmos()
    {
        if (collider == null || collider.isTrigger == false)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            collider = GetComponent<BoxCollider>();

            collider.isTrigger = true;
        }
    }

    public virtual void InteractObject()
    {
        if (isInteract)
            return;

        isInteract = true;
        Debug.Log(gameObject.name + " Interact By Player");
        onInteract?.Invoke();
    }    
}
