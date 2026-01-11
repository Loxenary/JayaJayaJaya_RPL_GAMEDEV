using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Special end game door that starts fully OPEN and can be closed/locked by an event.
/// Unlike InteractableLockedDoor which starts locked and opens with a key,
/// this door starts open and closes when triggered by a game event.
/// </summary>
public class InteractableEndGameDoor : InteractableLockedDoor, IRestartable
{
  [Tooltip("Play this sound when door closes and locks")]
  [SerializeField] private SfxClipData doorLockSound;

  [Tooltip("Reference to audio provider for playing sounds")]
  [SerializeField] private SpatialAudioProvider audioProvider;

  public UnityEvent OnDoorClosedAndLocked;

  // Event system for triggering door lock
  public delegate void EndGameDoorLockTrigger();
  public static event EndGameDoorLockTrigger onDoorLockTriggered;
  private bool isClosingInProgress = false;
  private bool _isEndGameLocked = false;

  private void Start()
  {
    // Initialize door as OPEN at start
    InitializeDoorAsOpen();
  }

  private void OnEnable()
  {


    EventBus.Subscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
    EventBus.Subscribe<InteractedPuzzleCount>(OnInteractedPuzzleCount);
  }

  private void OnDisable()
  {

    EventBus.Unsubscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
    EventBus.Unsubscribe<InteractedPuzzleCount>(OnInteractedPuzzleCount);
  }

  private void OnFirstPuzzleEvent(FirstPuzzleEvent evt)
  {
    CloseDoorAndLock();
    var audioManager = ServiceLocator.Get<AudioManager>();


    audioManager.PlaySfx(doorLockSound.SFXId);
  }

  /// <summary>
  /// Sets the door to open state initially
  /// </summary>
  private void InitializeDoorAsOpen()
  {
    // Mark as already interacted (opened)
    isInteract = true;
    _isEndGameLocked = false;

    // Set the door's rotation to the target (open) position immediately    

    Debug.Log($"[InteractableEndGameDoor] {gameObject.name} initialized as OPEN");
  }

  private void OnInteractedPuzzleCount(InteractedPuzzleCount evt)
  {
    if (evt.puzzleCount > 1)
    {
      UnlockDoor();
    }
  }

  /// <summary>
  /// Public method to trigger door closing and locking (can be called from UnityEvents)
  /// </summary>
  public void CloseDoorAndLock()
  {
    if (isClosingInProgress) return;

    Debug.Log($"[InteractableEndGameDoor] {gameObject.name} is closing and locking!");

    isClosingInProgress = true;

    // Close the door (rotate to closed position)
    DoRotate(Vector3.zero, false);

    // Play lock sound
    PlayLockSound();

    // Mark as locked     
    isInteract = false;
    _isEndGameLocked = true;

    // Invoke event
    OnDoorClosedAndLocked?.Invoke();
  }

  /// <summary>
  /// Static method to trigger all end game doors to lock
  /// </summary>
  public static void TriggerAllEndGameDoorsToLock()
  {
    onDoorLockTriggered?.Invoke();
    Debug.Log("[InteractableEndGameDoor] All end game doors triggered to lock!");
  }

  /// <summary>
  /// Override InteractObject to prevent opening when permanently locked
  /// </summary>
  public override void InteractObject()
  {
    if (_isEndGameLocked)
    {
      // Door is permanently locked, cannot open
      OnWrongKeys?.Invoke();
      Debug.Log($"[InteractableEndGameDoor] {gameObject.name} is permanently locked!");
      return;
    }

    // Otherwise, use the normal locked door behavior (key-based)
    Rotate();

  }

  /// <summary>
  /// Plays the door lock sound
  /// </summary>
  private void PlayLockSound()
  {
    if (audioProvider != null && doorLockSound != null)
    {
      audioProvider.PlaySfx(doorLockSound);
    }
  }

  /// <summary>
  /// Public method to unlock the door (for testing or special scenarios)
  /// </summary>
  public void UnlockDoor()
  {
    _isEndGameLocked = false;
  }

  public void Restart()
  {
    _isEndGameLocked = false;
  }
}
