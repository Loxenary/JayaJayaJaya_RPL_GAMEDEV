using System;
using UnityEngine;
using UnityEngine.Events;

public class CollectibleManager : MonoBehaviour
{
    [ReadOnly]
    public CollectibleCount counting;

    public UnityEvent firstPuzzlePickup;

    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleType>(ListenerCollectible);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleType>(ListenerCollectible);
        
    }

    void ListenerCollectible(CollectibleType type)
    {
        switch (type)
        {
            case CollectibleType.Key:
                counting.IncrementKey();
                break;
            case CollectibleType.Puzzle:
                counting.IncrementPuzzle();
                if(counting.GetPuzzleCount() == 0)
                {
                    firstPuzzlePickup?.Invoke();
                }
                break;
            default:
                break;
        }
    }
    [Serializable]
    public class CollectibleCount
    {
        [SerializeField]
        int key;
        [SerializeField]
        int puzzle;

        public void IncrementKey()
        {
            key++;
        }
        public void IncrementPuzzle()
        {
            puzzle++;
        }
        public int GetPuzzleCount() {  return key; }
    }

}


