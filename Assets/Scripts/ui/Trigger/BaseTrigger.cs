using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class BaseTrigger : MonoBehaviour
{
    protected virtual void Awake()
    {
        if (TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(Trigger);
        }
    }
    protected abstract void Trigger();
}