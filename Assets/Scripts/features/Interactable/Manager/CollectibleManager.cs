using System;
using UnityEngine;
using UnityEngine.Events;

public class CollectibleManager : MonoBehaviour
{
  public CollectibleCount counting;

  public UnityEvent firstPuzzlePickup;

    [Header("Event")]
    [SerializeField] int numberCollectible = 2;
    public UnityEvent callInSpesificCollectible;

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
                // Check if first puzzle pickup BEFORE incrementing
                if (counting.GetPuzzleCount() == 0)
                {
                  firstPuzzlePickup?.Invoke();
                  // Publish event for other systems (like SanityTimerSystem)
                  EventBus.Publish(new FirstPuzzleCollectedEvent());
                }        
                counting.IncrementPuzzle();

                if(numberCollectible == counting.GetPuzzleCount())
                {
                    callInSpesificCollectible?.Invoke();
                    Debug.Log("Open Door Again");
                }
                break;

                default:
                break;
        }
      }

  /// <summary>
  /// Event published when the first puzzle piece is collected
  /// </summary>
  public struct FirstPuzzleCollectedEvent { }

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
    public int GetPuzzleCount() { return puzzle; }
    public int GetKeyCount() { return key; }
  }

}


