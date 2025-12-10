using UnityEngine;

/// <summary>
/// System that tracks time and reduces player sanity after a threshold.
/// Timer starts at 0 and counts up. After 60 seconds, sanity decreases.
/// </summary>
public class SanityTimerSystem : MonoBehaviour
{
  [Header("Timer Settings")]
  [SerializeField] private float sanityThreshold = 60f; // When to start sanity drain
  [SerializeField] private float sanityDrainRate = 10f; // Sanity points per second (1% = 10 points)
  [SerializeField] private bool autoStart = true; // Auto start for testing

# if UNITY_EDITOR
  [Header("Debug")]
  [ReadOnly]
  [SerializeField] private float _currentTime => currentTime;
  [ReadOnly]
  [SerializeField] private bool _isRunning => isRunning;
  [ReadOnly]
  [SerializeField] private bool _isDrainingStarted => isDrainingStarted;
#endif

  private float currentTime = 0f;
  private bool isRunning = false;
  private bool isDrainingStarted = false;

  private PlayerAttributes playerAttributes;
  private float lastDrainTime = 0f;

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

  private void Start()
  {
    if (autoStart)
    {
      StartTimer();
    }
  }

  private void Update()
  {
    if (!isRunning) return;

    // Count up
    currentTime += Time.deltaTime;
    onTimerTick?.Invoke(currentTime);

    // Start sanity drain after threshold
    if (currentTime >= sanityThreshold && !isDrainingStarted)
    {
      isDrainingStarted = true;
      onSanityDrainStart?.Invoke();
      Debug.Log($"[SanityTimer] Threshold reached! Starting sanity drain at {sanityDrainRate}/sec");
    }

    // Drain sanity after threshold
    if (isDrainingStarted && playerAttributes != null)
    {
      if (Time.time - lastDrainTime >= 1f)
      {
        playerAttributes.TakeDamage(AttributesType.Sanity, (int)sanityDrainRate);
        lastDrainTime = Time.time;
      }
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
    lastDrainTime = Time.time;
    Debug.Log("[SanityTimer] Timer started");
  }

  /// <summary>
  /// Stop the timer
  /// </summary>
  public void StopTimer()
  {
    isRunning = false;
    Debug.Log("[SanityTimer] Timer stopped");
  }

  /// <summary>
  /// Pause the timer
  /// </summary>
  public void PauseTimer()
  {
    isRunning = false;
  }

  /// <summary>
  /// Resume the timer
  /// </summary>
  public void ResumeTimer()
  {
    isRunning = true;
  }

  /// <summary>
  /// Reset the timer to 0
  /// </summary>
  public void ResetTimer()
  {
    currentTime = 0f;
    isDrainingStarted = false;
    lastDrainTime = Time.time;
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
