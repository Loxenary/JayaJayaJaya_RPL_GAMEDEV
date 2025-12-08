using DG.Tweening;
using UnityEngine;

public class LoopingAnimatedRotation : MonoBehaviour
{
    [SerializeField] float time = 1.5f;
    [SerializeField] Transform targetTf;
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 endPos;

    private void Awake()
    {
        targetTf.DOLocalRotate(startPos, 0.01f);
    }
    private void Start()
    {
        Tween();
    }
    void Tween()
    {
        targetTf.DOLocalRotate(endPos, time).SetEase(Ease.Linear).OnComplete(() =>
        {
            targetTf.DOLocalRotate(startPos, time).SetEase(Ease.Linear).OnComplete(() => {
                Tween();
            });
        });
    }
}
