using System;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public CollectibleCount counting;

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
    }

}


