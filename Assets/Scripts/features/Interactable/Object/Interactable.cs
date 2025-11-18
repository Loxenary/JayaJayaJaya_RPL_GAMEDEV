using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class Interactable : MonoBehaviour
{
    BoxCollider collider;

    [Header("Debugging Section")]
    [ReadOnly]
    [SerializeField] bool isInteract;

    [Header("Event")]
    [SerializeField] UnityEvent onInteract;
    private void OnDrawGizmos()
    {
        if (collider == null || collider.isTrigger == false)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            collider = GetComponent<BoxCollider>();

            collider.isTrigger = true;
        }
    }

    public void InteractObject()
    {
        Debug.Log(gameObject.name + " Interact By Player");
        onInteract?.Invoke();
    }    
}
