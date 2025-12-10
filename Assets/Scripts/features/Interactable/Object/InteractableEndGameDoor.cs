using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Special end game door that starts fully OPEN and can be closed/locked by an event.
/// Unlike InteractableLockedDoor which starts locked and opens with a key,
/// this door starts open and closes when triggered by a game event.
/// </summary>
public class InteractableEndGameDoor : InteractableLockedDoor
{
    [Header("End Game Door Section")]
    [Tooltip("If true, the door is permanently locked and cannot be opened")]
    [SerializeField] private bool isPermanentlyLocked = false;

    [Tooltip("Play this sound when door closes and locks")]
    [SerializeField] private SfxClipData doorLockSound;

    [Tooltip("Reference to audio provider for playing sounds")]
    [SerializeField] private SpatialAudioProvider audioProvider;

    public UnityEvent OnDoorClosedAndLocked;

    // Event system for triggering door lock
    public delegate void EndGameDoorLockTrigger();
    public static event EndGameDoorLockTrigger onDoorLockTriggered;

    private bool isClosingInProgress = false;



    private void Start()
    {
        // Initialize door as OPEN at start
        InitializeDoorAsOpen();


    }

    private void OnEnable()
    {

        // Listen for the door lock trigger event
        onDoorLockTriggered += OnDoorLockTriggered;

        EventBus.Subscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
    }

    private void OnDisable()
    {

        onDoorLockTriggered -= OnDoorLockTriggered;

        EventBus.Unsubscribe<FirstPuzzleEvent>(OnFirstPuzzleEvent);
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

        // Set the door's rotation to the target (open) position immediately
        if (rootObject != null)
        {
            rootObject.localRotation = Quaternion.Euler(targetRotation);
        }

        Debug.Log($"[InteractableEndGameDoor] {gameObject.name} initialized as OPEN");
    }

    /// <summary>
    /// Called when the door lock event is triggered
    /// </summary>
    private void OnDoorLockTriggered()
    {
        if (!isClosingInProgress && !isPermanentlyLocked)
        {
            CloseDoorAndLock();
        }
    }

    /// <summary>
    /// Public method to trigger door closing and locking (can be called from UnityEvents)
    /// </summary>
    public void CloseDoorAndLock()
    {
        if (isClosingInProgress || isPermanentlyLocked) return;

        Debug.Log($"[InteractableEndGameDoor] {gameObject.name} is closing and locking!");

        isClosingInProgress = true;

        // Close the door (rotate to closed position)
        DoRotate(Vector3.zero);

        // Play lock sound
        PlayLockSound();

        // Mark as locked permanently
        isPermanentlyLocked = true;
        isInteract = false;

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
        if (isPermanentlyLocked)
        {
            // Door is permanently locked, cannot open
            OnWrongKeys?.Invoke();
            Debug.Log($"[InteractableEndGameDoor] {gameObject.name} is permanently locked!");
            return;
        }

        // Otherwise, use the normal locked door behavior (key-based)
        base.InteractObject();
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
    /// Public method to manually lock the door without closing animation
    /// </summary>
    public void LockDoorImmediately()
    {
        isPermanentlyLocked = true;
        isInteract = false;
        Debug.Log($"[InteractableEndGameDoor] {gameObject.name} locked immediately!");
    }

    /// <summary>
    /// Public method to unlock the door (for testing or special scenarios)
    /// </summary>
    public void UnlockDoor()
    {
        isPermanentlyLocked = false;
        Debug.Log($"[InteractableEndGameDoor] {gameObject.name} unlocked!");
    }
}