using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableDamager : Interactable
{
    [Header("Interactable Damage Section")]
    [SerializeField] int damage = 5;
    [SerializeField] int hitCount = 3;
    [Range(0f, 60f)]
    [SerializeField] float resetTime = 30f;

    [ReadOnly]
    [SerializeField] int currentCount = 0;


    public delegate void InteractableDamagerDelegate(int damage);
    public static event InteractableDamagerDelegate onInteractDamager;
    public override void InteractObject()
    {
        if (isInteract)
            return;


        currentCount++;

        if(currentCount == hitCount)
        {
            isInteract = true;
            currentCount = 0;
            BaseInteract();
            //EventBus.Publish<InteractableDamager>(damage);
            onInteractDamager?.Invoke(damage);

            Debug.Log("Damage !!!");
            DOTween.Sequence().SetDelay(resetTime).OnComplete(() => {
                isInteract= false;
            });
        }
        
    }
    void BaseInteract()
    {
        Debug.Log(gameObject.name + " Interact By Player");
        onInteract?.Invoke();
    }
}
