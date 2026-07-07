using System;
using UnityEngine;
using UnityEngine.Events;

public class CollectibleManager : MonoBehaviour, IRestartable
{
  public CollectibleCount counting;

  public UnityEvent firstPuzzlePickup;

  [Header("Event")]
  [SerializeField] int numberCollectible = 2;
  public UnityEvent callInSpesificCollectible;

  private void OnEnable()
  {
    EventBus.Subscribe<CollectibleType>(ListenerCollectible);
    RestartManager.Register(this);
  }
  private void OnDisable()
  {
    EventBus.Unsubscribe<CollectibleType>(ListenerCollectible);
    RestartManager.Unregister(this);
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
          EventBus.Publish(new FirstPuzzleEvent());

        }
        counting.IncrementPuzzle();


        if (numberCollectible == counting.GetPuzzleCount())
        {
          callInSpesificCollectible?.Invoke();
          Debug.Log("Open Door Again");
        }
        break;

      default:
        break;
    }
  }

  public void Restart()
  {
    counting.Restart();
  }

  /// <summary>
  /// Event published when the first puzzle piece is collected.
  ///
  /// JANGAN TERTUKAR dengan FirstPuzzleEvent (B-17):
  /// - Event ini = momen COLLECT pertama, hanya dari CollectibleManager;
  ///   dipakai SanityTimerSystem untuk memulai timer drain.
  /// - FirstPuzzleEvent = trigger reaksi dunia (ghost spawn, lighting,
  ///   endgame door) dan punya beberapa publisher lain.
  /// </summary>
  public struct FirstPuzzleCollectedEvent { }

  [Serializable]
  public class CollectibleCount : IRestartable
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

    public void Restart()
    {
      key = 0;
      puzzle = 0;
    }
  }

}


