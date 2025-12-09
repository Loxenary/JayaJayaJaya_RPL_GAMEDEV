using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Enemy Angry System - Manages the anger level of enemies based on accumulated anger points.
/// Anger points increase from:
/// - Items taken by player
/// - Low player sanity
/// - Low game timer
///
/// As anger points accumulate, the enemy levels up through different anger states,
/// each with different stats and potentially visual effects.
/// </summary>
public class EnemyAngrySystem : MonoBehaviour
{
  [Header("Configuration")]
  [SerializeField] private EnemyAngryConfiguration angryConfiguration;

  [SerializeField] private EnemyAngryPointConfiguration pointConfiguration;

  [Header("Events")]
  [Tooltip("Triggered when anger level changes")]
  public UnityEvent<EnemyLevel> OnAngryLevelChanged;

  [Tooltip("Triggered when anger points are added")]
  public UnityEvent<float> OnAngryPointsChanged;

  [Header("Debug")]
  [SerializeField] private bool showDebugLogs = true;
  [SerializeField] private bool showDebugGUI = true;

  [Header("Runtime Info (Read Only)")]
  [SerializeField][ReadOnly] private float currentAngryPoints = 0f;
  [SerializeField][ReadOnly] private EnemyLevel currentLevel = EnemyLevel.FIRST;
  [SerializeField][ReadOnly] private float currentSanityValue = 1f;
  [SerializeField][ReadOnly] private float currentTimerValue = 0f;

  // C# Events for better integration
  public static event Action<EnemyLevel> OnGlobalAngryLevelChanged;
  public static event Action<float> OnGlobalAngryPointsChanged;

  // Properties
  public float CurrentAngryPoints => currentAngryPoints;
  public EnemyLevel CurrentLevel => currentLevel;
  public EnemyConfigurationRecord CurrentConfiguration { get; private set; }

  private EnemyConfigurationRecord[] sortedConfigurations;
  private bool isInitialized = false;

  #region Unity Lifecycle

  private void Awake()
  {
    InitializeSystem();
  }

  private void OnEnable()
  {
    // Subscribe to player events when they're implemented
    PlayerAttributes.onSanityUpdate += OnPlayerSanityChanged;
    // TODO: Subscribe to timer events when implemented
    // TODO: Subscribe to item collection events when implemented
  }

  private void OnDisable()
  {
    PlayerAttributes.onSanityUpdate -= OnPlayerSanityChanged;
    // TODO: Unsubscribe from timer events
    // TODO: Unsubscribe from item collection events
  }

  private void Update()
  {
    if (!isInitialized) return;

    // Accumulate points based on current conditions
    AccumulateAngryPointsOverTime();
  }

  #endregion

  #region Initialization

  private void InitializeSystem()
  {
    if (angryConfiguration == null)
    {
      Debug.LogError("[EnemyAngrySystem] No EnemyAngryConfiguration assigned!", this);
      enabled = false;
      return;
    }

    // Sort configurations by angry point baseline
    sortedConfigurations = angryConfiguration.GetSortedConfigurations();

    if (sortedConfigurations == null || sortedConfigurations.Length == 0)
    {
      Debug.LogError("[EnemyAngrySystem] No configurations found in EnemyAngryConfiguration!", this);
      enabled = false;
      return;
    }

    // Start at the first level
    currentLevel = sortedConfigurations[0].level;
    CurrentConfiguration = sortedConfigurations[0];
    currentAngryPoints = 0f;

    isInitialized = true;

    Log($"EnemyAngrySystem initialized. Starting at level: {currentLevel}");
  }

  #endregion

  #region Anger Point Management

  /// <summary>
  /// Add anger points and check for level up
  /// </summary>
  public void AddAngryPoints(float points)
  {
    if (!isInitialized || points <= 0) return;

    currentAngryPoints += points;

    Log($"Added {points} angry points. Total: {currentAngryPoints}");

    // Trigger events
    OnAngryPointsChanged?.Invoke(currentAngryPoints);
    OnGlobalAngryPointsChanged?.Invoke(currentAngryPoints);

    // Check for level up
    CheckAndUpdateAngryLevel();
  }

  /// <summary>
  /// Manually set angry points (useful for testing or special events)
  /// </summary>
  public void SetAngryPoints(float points)
  {
    if (!isInitialized) return;

    currentAngryPoints = Mathf.Max(0, points);

    Log($"Set angry points to: {currentAngryPoints}");

    OnAngryPointsChanged?.Invoke(currentAngryPoints);
    OnGlobalAngryPointsChanged?.Invoke(currentAngryPoints);

    CheckAndUpdateAngryLevel();
  }

  /// <summary>
  /// Reset angry points to zero and return to first level
  /// </summary>
  public void ResetAngryPoints()
  {
    currentAngryPoints = 0f;
    ChangeLevel(sortedConfigurations[0].level);

    Log("Angry points reset to 0");
  }

  private void CheckAndUpdateAngryLevel()
  {
    // Find the highest level we qualify for based on current points
    EnemyConfigurationRecord newConfig = sortedConfigurations[0];

    for (int i = sortedConfigurations.Length - 1; i >= 0; i--)
    {
      if (currentAngryPoints >= sortedConfigurations[i].angryPointBaseline)
      {
        newConfig = sortedConfigurations[i];
        break;
      }
    }

    // Check if level changed
    if (newConfig.level != currentLevel)
    {
      ChangeLevel(newConfig.level);
    }
  }

  private void ChangeLevel(EnemyLevel newLevel)
  {
    EnemyLevel previousLevel = currentLevel;
    currentLevel = newLevel;

    // Update current configuration
    CurrentConfiguration = sortedConfigurations.First(c => c.level == newLevel);

    Log($"LEVEL UP! {previousLevel} -> {currentLevel} (Points: {currentAngryPoints}/{CurrentConfiguration.angryPointBaseline})");

    // Trigger events
    OnAngryLevelChanged?.Invoke(currentLevel);
    OnGlobalAngryLevelChanged?.Invoke(currentLevel);
  }

  #endregion

  #region Point Accumulation Methods

  private void AccumulateAngryPointsOverTime()
  {
    float pointsThisFrame = 0f;

    // Accumulate from low sanity
    if (currentSanityValue < pointConfiguration.LowSanityThreshold)
    {
      float sanityPoints = pointConfiguration.PointsPerPerSecondLowSanity * Time.deltaTime;
      pointsThisFrame += sanityPoints;
    }

    // Accumulate from low timer (if timer is implemented)
    if (currentTimerValue > 0 && currentTimerValue < pointConfiguration.LowTimerThreshold)
    {
      float timerPoints = pointConfiguration.PointsPerSecondLowTimer * Time.deltaTime;
      pointsThisFrame += timerPoints;
    }

    if (pointsThisFrame > 0)
    {
      AddAngryPoints(pointsThisFrame);
    }
  }

  /// <summary>
  /// Called when an item is taken by the player
  /// Hook this up when item collection is implemented
  /// </summary>
  public void OnItemTaken()
  {
    AddAngryPoints(pointConfiguration.PointsPerItemTaken);
    Log($"Item taken! Added {pointConfiguration.PointsPerItemTaken} angry points");
  }

  /// <summary>
  /// Called when player sanity changes
  /// </summary>
  private void OnPlayerSanityChanged(float normalizedSanity)
  {
    currentSanityValue = normalizedSanity;

    if (showDebugLogs && normalizedSanity < pointConfiguration.LowSanityThreshold)
    {
      Log($"Player sanity low ({normalizedSanity:P1}), accumulating anger points...");
    }
  }

  /// <summary>
  /// Called when game timer changes
  /// Hook this up when timer is implemented
  /// </summary>
  public void OnTimerChanged(float timeRemaining)
  {
    currentTimerValue = timeRemaining;

    if (showDebugLogs && timeRemaining < pointConfiguration.LowTimerThreshold && timeRemaining > 0)
    {
      Log($"Timer low ({timeRemaining:F0}s), accumulating anger points...");
    }
  }

  #endregion

  #region Public API

  /// <summary>
  /// Get the enemy stats for the current anger level
  /// </summary>
  public EnemyAI.EnemyStats GetCurrentStats()
  {
    return CurrentConfiguration?.enemyStats;
  }

  /// <summary>
  /// Get the enemy stats for a specific anger level
  /// </summary>
  public EnemyAI.EnemyStats GetStatsForLevel(EnemyLevel level)
  {
    var config = sortedConfigurations.FirstOrDefault(c => c.level == level);
    return config?.enemyStats;
  }

  /// <summary>
  /// Get progress to next level (0-1)
  /// Returns 1 if at max level
  /// </summary>
  public float GetProgressToNextLevel()
  {
    int currentIndex = Array.FindIndex(sortedConfigurations, c => c.level == currentLevel);

    if (currentIndex == sortedConfigurations.Length - 1)
    {
      return 1f; // At max level
    }

    float currentBaseline = sortedConfigurations[currentIndex].angryPointBaseline;
    float nextBaseline = sortedConfigurations[currentIndex + 1].angryPointBaseline;

    float progress = (currentAngryPoints - currentBaseline) / (nextBaseline - currentBaseline);
    return Mathf.Clamp01(progress);
  }

  /// <summary>
  /// Check if at maximum anger level
  /// </summary>
  public bool IsAtMaxLevel()
  {
    return currentLevel == sortedConfigurations[sortedConfigurations.Length - 1].level;
  }

  #endregion

  #region Debug & Utility

  private void Log(string message)
  {
    if (showDebugLogs)
    {
      Debug.Log($"[EnemyAngrySystem] {message}", this);
    }
  }

  private void OnGUI()
  {
    if (!showDebugGUI || !isInitialized) return;

    GUIStyle style = new GUIStyle(GUI.skin.box);
    style.alignment = TextAnchor.UpperLeft;
    style.fontSize = 12;
    style.normal.textColor = Color.white;

    string debugText = $"=== ENEMY ANGRY SYSTEM ===\n" +
                      $"Current Level: {currentLevel}\n" +
                      $"Angry Points: {currentAngryPoints:F1}\n" +
                      $"Progress to Next: {GetProgressToNextLevel():P1}\n" +
                      $"At Max Level: {IsAtMaxLevel()}\n" +
                      $"\nPlayer Status:\n" +
                      $"Sanity: {currentSanityValue:P1} (Threshold: {pointConfiguration.LowSanityThreshold:P1})\n" +
                      $"Timer: {currentTimerValue:F1}s (Threshold: {pointConfiguration.LowTimerThreshold:F0}s)\n" +
                      $"\nAccumulation Rates:\n" +
                      $"Per Item: {pointConfiguration.PointsPerItemTaken:F1}\n" +
                      $"Per Sec (Low Sanity): {pointConfiguration.PointsPerPerSecondLowSanity:F1}\n" +
                      $"Per Sec (Low Timer): {pointConfiguration.PointsPerSecondLowTimer:F1}";

    GUI.Box(new Rect(10, 10, 300, 240), debugText, style);
  }

  #endregion
}
