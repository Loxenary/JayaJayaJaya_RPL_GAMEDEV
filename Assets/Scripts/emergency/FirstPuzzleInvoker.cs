using UnityEngine;
using UnityEngine.Events;

public class FirstPuzzleInvoker : MonoBehaviour
{
#if UNITY_EDITOR
    [ReadOnly, SerializeField]
#endif
    bool firstPuzzle;

    public UnityEvent OnPickupedFirstItem;

    private void OnEnable()
    {
        InteractableHintObject.onCheckIsFirst += CheckIsFirst;
    }
    private void OnDisable()
    {
        InteractableHintObject.onCheckIsFirst -= CheckIsFirst;        
    }
    private void CheckIsFirst()
    {
        if (!firstPuzzle)
        {
            firstPuzzle = true;

            EventBus.Publish(new FirstPuzzleEvent());

            OnPickupedFirstItem?.Invoke();
        }
    }


}
