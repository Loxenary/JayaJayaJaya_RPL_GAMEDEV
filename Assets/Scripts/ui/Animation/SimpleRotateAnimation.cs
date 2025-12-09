using DG.Tweening;
using UnityEngine;

public class SimpleRotateAnimation : MonoBehaviour
{
    [SerializeField] Transform targetTf;
    [SerializeField] float timeDuration = 3.5f;
    [SerializeField] Vector3 targetValue;

    private void Start()
    {
        if (targetTf)
        {
            targetTf.DORotate(targetValue, timeDuration)
            .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
