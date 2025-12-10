using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class ForceRotate : MonoBehaviour
{
    [SerializeField] Transform targetTf;
    [SerializeField] Vector3 rotationTarget;


    public UnityEvent OnForceRotateCall;

    public void ForceRotateObject()
    {
        targetTf.DORotate(rotationTarget, .1f);
        OnForceRotateCall?.Invoke();
    }
}
