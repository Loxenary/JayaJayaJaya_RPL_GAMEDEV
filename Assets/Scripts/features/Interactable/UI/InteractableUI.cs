using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractableUI : MonoBehaviour
{

    [Header("Event")]
    [SerializeField] UnityEvent OnInteract;
    [SerializeField] UnityEvent OnInteractEnter;
    [SerializeField] UnityEvent OnInteractExit;


    private void OnEnable()
    {
        EventBus.Subscribe<InteractEventState>(InteracEventListener);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<InteractEventState>(InteracEventListener);
    }
    private void InteracEventListener(InteractEventState state)
    {
        switch (state) { 
            case InteractEventState.OnInteract:
                Interact();
                break;
            case InteractEventState.OnEnter:
                Enter();
                break;
            case InteractEventState.OnExit:
                Exit();
                break;

        }  
    }
    void Interact()
    {
        OnInteractEnter.Invoke();
        //Write Your Code
    }
    void Enter()
    {
        OnInteractEnter.Invoke();
        //Write Your Code
    }
    void Exit()
    {
        OnInteractExit.Invoke();
        //Write Your Code
    }

}
