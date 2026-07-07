using UnityEngine;

/// <summary>
/// System that tracks time and starts a sanity drain after a threshold.
/// Timer starts when first puzzle collectible is picked up.
///
/// PENTING (B-04): sistem ini TIDAK mengurangi sanity sendiri — ia hanya
/// menyetel laju drain eksternal via PlayerAttributes.SetExternalDrain().
/// PlayerAttributes adalah satu-satunya mutator sanity, sehingga laju total
/// selalu deterministik dan bisa di-tune dari satu tempat.
/// </summary>
public class SanityTimerSystem : MonoBehaviour
{
  [Header("Timer Settings")]
  [SerializeField] private float sanityThreshold = 30f; // Time after first collectible to start sanity drain
  [SerializeField] private float sanityDrainRate = 10f; // Sanity points per second (1% = 10 points)
  [SerializeField] private bool autoStart = false; // No longer auto-start, wait for collectible

  private float currentTime = 0f;
  private bool isRunning = false;
  private bool isDrainingStarted = false;
  private bool hasFirstCollectible = false;

  private PlayerAttributes playerAttributes;

  // Events
  public delegate void OnTimerTick(float time);
  public static event OnTimerTick onTimerTick;

  public delegate void OnSanityDrainStart();
  public static event OnSanityDrainStart onSanityDrainStart;

  private void Awake()
  {
    playerAttributes = GetComponent<PlayerAttributes>();
    if (playerAttributes == null)
    {
      Debug.LogError("[SanityTimer] PlayerAttributes not found!");
    }
  }

  private void OnEnable()
  {
    // Subscribe to first puzzle collectible event
    EventBus.Subscribe<CollectibleManager.FirstPuzzleCollectedEvent>(OnFirstPuzzleCollected);
  }

  private void OnDisable()
  {
    EventBus.Unsubscribe<CollectibleManager.FirstPuzzleCollectedEvent>(OnFirstPuzzleCollected);
    // Jangan tinggalkan drain menyala kalau sistem ini mati/di-destroy
    SetDrainActive(false);
  }

  private void Start()
  {
    if (autoStart)
    {
      StartTimer();
    }
  }

  /// <summary>
  /// Called when the first puzzle piece is collected
  /// </summary>
  private void OnFirstPuzzleCollected(CollectibleManager.FirstPuzzleCollectedEvent evt)
  {
    if (hasFirstCollectible) return; // Only trigger once

    hasFirstCollectible = true;
    StartTimer();
    Debug.Log("[SanityTimer] First puzzle collected! Timer started. Sanity drain will begin in " + sanityThreshold + " seconds.");
  }

  private void Update()
  {
    if (!isRunning) return;

    // Count up
    currentTime += Time.deltaTime;
    onTimerTick?.Invoke(currentTime);

    // Start sanity drain after threshold — delegasikan ke PlayerAttributes
    // sebagai modifier, bukan menguras sendiri (B-04)
    if (currentTime >= sanityThreshold && !isDrainingStarted)
    {
      isDrainingStarted = true;
      SetDrainActive(true);
      onSanityDrainStart?.Invoke();
      Debug.Log($"[SanityTimer] Threshold reached! Starting sanity drain at {sanityDrainRate}/sec");
    }
  }

  /// <summary>
  /// Nyalakan/matikan kontribusi drain sistem ini di PlayerAttributes.
  /// </summary>
  private void SetDrainActive(bool active)
  {
    if (playerAttributes != null)
    {
      playerAttributes.SetExternalDrain(active ? sanityDrainRate : 0f);
    }
  }

  /// <summary>
  /// Start the timer
  /// </summary>
  public void StartTimer()
  {
    isRunning = true;
    currentTime = 0f;
    isDrainingStarted = false;
    SetDrainActive(false);
    Debug.Log("[SanityTimer] Timer started");
  }

  /// <summary>
  /// Stop the timer
  /// </summary>
  public void StopTimer()
  {
    isRunning = false;
    SetDrainActive(false);
    Debug.Log("[SanityTimer] Timer stopped");
  }

  /// <summary>
  /// Pause the timer
  /// </summary>
  public void PauseTimer()
  {
    isRunning = false;
    SetDrainActive(false);
  }

  /// <summary>
  /// Resume the timer
  /// </summary>
  public void ResumeTimer()
  {
    isRunning = true;
    // Kalau threshold sudah lewat sebelum pause, drain lanjut lagi
    SetDrainActive(isDrainingStarted);
  }

  /// <summary>
  /// Reset the timer to 0
  /// </summary>
  public void ResetTimer()
  {
    currentTime = 0f;
    isDrainingStarted = false;
    SetDrainActive(false);
    Debug.Log("[SanityTimer] Timer reset");
  }

  /// <summary>
  /// Get current time
  /// </summary>
  public float GetCurrentTime() => currentTime;

  /// <summary>
  /// Check if timer is running
  /// </summary>
  public bool IsRunning() => isRunning;

  /// <summary>
  /// Check if sanity drain has started
  /// </summary>
  public bool IsDraining() => isDrainingStarted;
}
