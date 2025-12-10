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

#if UNITY_EDITOR
  [Header("Runtime Info (Read Only)")]
  [SerializeField][ReadOnly] private float _currentAngryPoints => currentAngryPoints;
  [SerializeField][ReadOnly] private EnemyLevel _currentLevel => currentLevel;
  [SerializeField][ReadOnly] private float _currentSanityValue => currentSanityValue;
  [SerializeField][ReadOnly] private int _currentItemsTaken => currentItemsTaken;
  [SerializeField][ReadOnly] private float _maxSanityValue => maxSanityValue;

#endif

  private float currentAngryPoints = 0f;
  private EnemyLevel currentLevel = EnemyLevel.FIRST;
  private float currentSanityValue = 1f;
  private int currentItemsTaken = 0;
  private float maxSanityValue = 1000f;

  // Reference to player for getting max sanity
  private PlayerAttributes playerAttributes;

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
    FindPlayerAttributes();
  }

  private void OnEnable()
  {
    // Subscribe to player events
    PlayerAttributes.onSanityUpdate += OnPlayerSanityChanged;
    EventBus.Subscribe<InteractedPuzzleCount>(evt => OnItemTaken(evt));
  }

  private void OnDisable()
  {
    PlayerAttributes.onSanityUpdate -= OnPlayerSanityChanged;
    EventBus.Unsubscribe<InteractedPuzzleCount>(evt => OnItemTaken(evt));
  }

  /// <summary>
  /// Find PlayerAttributes in the scene to get max sanity (single source of truth)
  /// </summary>
  private void FindPlayerAttributes()
  {
    playerAttributes = FindAnyObjectByType<PlayerAttributes>();

    if (playerAttributes != null)
    {
      maxSanityValue = playerAttributes.MaxSanity;
      Log($"Found PlayerAttributes. Max Sanity: {maxSanityValue}");
    }
    else
    {
      Debug.LogWarning("[EnemyAngrySystem] PlayerAttributes not found! Using default max sanity value.", this);
    }
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
    // Calculate dynamic points per second based on current state
    float pointsPerSecond = pointConfiguration.CalculatePointsPerSecond(currentItemsTaken, currentSanityValue, maxSanityValue);

    if (pointsPerSecond > 0)
    {
      float pointsThisFrame = pointsPerSecond * Time.deltaTime;
      AddAngryPoints(pointsThisFrame);
    }
  }

  /// <summary>
  /// Called when an item is taken by the player
  /// </summary>
  private void OnItemTaken(InteractedPuzzleCount evt)
  {
    currentItemsTaken = evt.puzzleCount;

    // Recalculate angry points based on new state
    RecalculateAngryPoints();

    Log($"Item taken! Total items: {currentItemsTaken}/{pointConfiguration.TotalPuzzleItems}");
  }

  /// <summary>
  /// Called when player sanity changes (receives normalized value 0-1)
  /// </summary>
  private void OnPlayerSanityChanged(float normalizedSanity)
  {
    // Convert normalized sanity (0-1) to actual sanity value using PlayerAttributes max sanity
    currentSanityValue = normalizedSanity * maxSanityValue;

    // Recalculate angry points based on new sanity
    RecalculateAngryPoints();
  }

  /// <summary>
  /// Recalculate angry points based on current game state
  /// </summary>
  private void RecalculateAngryPoints()
  {
    float newPoints = pointConfiguration.CalculateAngryPoints(currentItemsTaken, currentSanityValue, maxSanityValue);

    // Update points to match current game state (can increase or decrease)
    if (Mathf.Abs(newPoints - currentAngryPoints) > 0.01f) // Only update if there's a meaningful change
    {
      SetAngryPoints(newPoints);

      string changeDirection = newPoints > currentAngryPoints ? "increased" : "decreased";
      Log($"Angry points {changeDirection}: {currentAngryPoints:F1} (Items: {currentItemsTaken}, Sanity: {currentSanityValue:F1}/{maxSanityValue})");
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
    if (sortedConfigurations == null || sortedConfigurations.Length == 0)
    {
      Debug.LogWarning("[EnemyAngrySystem] Attempted to get stats before system was initialized!");
      return null;
    }

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

    float currentPointsPerSec = pointConfiguration.CalculatePointsPerSecond(currentItemsTaken, currentSanityValue, maxSanityValue);
    float calculatedPoints = pointConfiguration.CalculateAngryPoints(currentItemsTaken, currentSanityValue, maxSanityValue);

    string debugText = $"=== ENEMY ANGRY SYSTEM (DYNAMIC) ===\n" +
                      $"Current Level: {currentLevel}\n" +
                      $"Angry Points: {currentAngryPoints:F1} / {pointConfiguration.MaxAngryPoints:F1}\n" +
                      $"Progress to Next: {GetProgressToNextLevel():P0}\n" +
                      $"At Max Level: {IsAtMaxLevel()}\n" +
                      $"\nPlayer Status:\n" +
                      $"Items Taken: {currentItemsTaken}/{pointConfiguration.TotalPuzzleItems}\n" +
                      $"Sanity: {currentSanityValue:F1}/{maxSanityValue:F1} (from PlayerAttributes)\n" +
                      $"\nDynamic Calculation:\n" +
                      $"Calculated Points: {calculatedPoints:F1}\n" +
                      $"Points/Second: {currentPointsPerSec:F2}\n" +
                      $"\nWeights:\n" +
                      $"Item Weight: {pointConfiguration.ItemWeight:F2} (Exp: {pointConfiguration.ItemExponent:F1})\n" +
                      $"Sanity Weight: {pointConfiguration.SanityWeight:F2} (Exp: {pointConfiguration.SanityExponent:F1})";

    GUI.Box(new Rect(10, 10, 350, 280), debugText, style);
  }

  #endregion
}
