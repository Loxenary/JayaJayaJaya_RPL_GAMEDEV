using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableSelector : MonoBehaviour
{
    [Header("Basic Components")]
    [SerializeField] LayerMask targetLayer = -1;


    [Header("Debugging Paramenters")]
    [ReadOnly]
    [SerializeField] Interactable currentObject;



    InputSystem_Actions input;

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    private void Awake()
    {
        input = new InputSystem_Actions();

        input.Player.Interact.performed += OnPressInteract;
    }

    private void OnPressInteract(InputAction.CallbackContext context)
    {
        if (currentObject)
        {
            currentObject.InteractObject();
            EventBus.Publish(InteractEventState.OnInteract);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            currentObject = other.gameObject.GetComponent<Interactable>();

            Debug.Log("Trigger With Interactable Name : "+other.gameObject.name);

            EventBus.Publish(InteractEventState.OnEnter);
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<Interactable>() == currentObject) { 
            currentObject = null;
            EventBus.Publish(InteractEventState.OnExit);
        }
    }
}
    public enum InteractEventState
    {
        OnEnter,
        OnExit,
        OnInteract
    }
