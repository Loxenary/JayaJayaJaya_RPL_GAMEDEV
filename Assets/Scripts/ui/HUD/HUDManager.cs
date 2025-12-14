using UnityEngine;
using TMPro;

/// <summary>
/// Central manager for the HUD system. Coordinates all HUD elements including sanity, battery, items, and timer.
/// </summary>
public class HUDManager : FadeShowHideProceduralWithEventBus<HUDManager>
{
  [Header("HUD Components")]
  [SerializeField] private SanityDisplay sanityDisplay;
  [SerializeField] private BatteryDisplay batteryDisplay;
  [SerializeField] private DiscreteBatteryDisplay discreteBatteryDisplay;
  [SerializeField] private ItemIconsDisplay itemIconsDisplay;
  [SerializeField] private TimerDisplay timerDisplay;

  [Header("Auto-Find Components")]
  [Tooltip("If enabled, will automatically find HUD components in children if not assigned")]
  [SerializeField] private bool autoFindComponents = true;

  private void Start()
  {
    if (autoFindComponents)
    {
      TryFindComponents();
    }

    ValidateComponents();

    // Hide timer display (no longer used - SanityTimerSystem handles it internally)
    if (timerDisplay != null)
    {
      timerDisplay.gameObject.SetActive(false);
    }
  }

  private void TryFindComponents()
  {
    if (sanityDisplay == null)
      sanityDisplay = GetComponentInChildren<SanityDisplay>();

    if (batteryDisplay == null)
      batteryDisplay = GetComponentInChildren<BatteryDisplay>();

    if (discreteBatteryDisplay == null)
      discreteBatteryDisplay = GetComponentInChildren<DiscreteBatteryDisplay>();

    if (itemIconsDisplay == null)
      itemIconsDisplay = GetComponentInChildren<ItemIconsDisplay>();

    if (timerDisplay == null)
      timerDisplay = GetComponentInChildren<TimerDisplay>();
  }

  private void ValidateComponents()
  {
    if (sanityDisplay == null)
      Debug.LogWarning("[HUDManager] SanityDisplay not assigned or found.", this);

    if (batteryDisplay == null && discreteBatteryDisplay == null)
      Debug.LogWarning("[HUDManager] No battery display (BatteryDisplay or DiscreteBatteryDisplay) assigned or found.", this);

    if (itemIconsDisplay == null)
      Debug.LogWarning("[HUDManager] ItemIconsDisplay not assigned or found.", this);

    if (timerDisplay == null)
      Debug.LogWarning("[HUDManager] TimerDisplay not assigned or found.", this);
  }

  #region Public API - Sanity

  /// <summary>
  /// Updates the sanity display.
  /// </summary>
  /// <param name="currentSanity">Current sanity value</param>
  /// <param name="maxSanity">Maximum sanity value</param>
  public void UpdateSanity(float currentSanity, float maxSanity)
  {
    if (sanityDisplay != null)
    {
      sanityDisplay.UpdateSanity(currentSanity, maxSanity);
    }
  }

  /// <summary>
  /// Updates sanity with a single normalized value (0-1).
  /// </summary>
  public void UpdateSanityNormalized(float normalizedSanity)
  {
    if (sanityDisplay != null)
    {
      sanityDisplay.UpdateSanityNormalized(normalizedSanity);
    }
  }

  #endregion

  #region Public API - Battery

  /// <summary>
  /// Updates the battery display. Works with both BatteryDisplay and DiscreteBatteryDisplay.
  /// </summary>
  /// <param name="currentBattery">Current battery value</param>
  /// <param name="maxBattery">Maximum battery value</param>
  public void UpdateBattery(float currentBattery, float maxBattery)
  {
    if (batteryDisplay != null)
    {
      batteryDisplay.UpdateBattery(currentBattery, maxBattery);
    }
    // DiscreteBatteryDisplay subscribes to PlayerAttributes.onBatteryUpdate directly
    // No need to call it manually here
  }

  /// <summary>
  /// Updates battery with a single normalized value (0-1). Works with both battery display types.
  /// </summary>
  public void UpdateBatteryNormalized(float normalizedBattery)
  {
    if (batteryDisplay != null)
    {
      batteryDisplay.UpdateBatteryNormalized(normalizedBattery);
    }
    // DiscreteBatteryDisplay subscribes to PlayerAttributes.onBatteryUpdate directly
    // No need to call it manually here
  }

  #endregion

  #region Public API - Items

  /// <summary>
  /// Adds an item icon to the display.
  /// </summary>
  /// <param name="itemType">Type of item (Food or Medicine)</param>
  /// <param name="icon">Sprite to display</param>
  /// <param name="quantity">Optional quantity to display</param>
  public void AddItemIcon(ItemType itemType, Sprite icon, int quantity = 1)
  {
    if (itemIconsDisplay != null)
    {
      itemIconsDisplay.AddItem(itemType, icon, quantity);
    }
  }

  /// <summary>
  /// Removes an item icon from the display.
  /// </summary>
  public void RemoveItemIcon(ItemType itemType)
  {
    if (itemIconsDisplay != null)
    {
      itemIconsDisplay.RemoveItem(itemType);
    }
  }

  /// <summary>
  /// Updates the quantity of an existing item.
  /// </summary>
  public void UpdateItemQuantity(ItemType itemType, int quantity)
  {
    if (itemIconsDisplay != null)
    {
      itemIconsDisplay.UpdateItemQuantity(itemType, quantity);
    }
  }

  /// <summary>
  /// Clears all item icons.
  /// </summary>
  public void ClearAllItems()
  {
    if (itemIconsDisplay != null)
    {
      itemIconsDisplay.ClearAllItems();
    }
  }

  #endregion

  #region Public API - Timer

  /// <summary>
  /// Starts the timer in countdown mode.
  /// </summary>
  /// <param name="duration">Duration in seconds</param>
  public void StartCountdown(float duration)
  {
    if (timerDisplay != null)
    {
      timerDisplay.StartCountdown(duration);
    }
  }

  /// <summary>
  /// Starts the timer in count-up mode.
  /// </summary>
  public void StartCountUp()
  {
    if (timerDisplay != null)
    {
      timerDisplay.StartCountUp();
    }
  }

  /// <summary>
  /// Pauses the timer.
  /// </summary>
  public void PauseTimer()
  {
    if (timerDisplay != null)
    {
      timerDisplay.PauseTimer();
    }
  }

  /// <summary>
  /// Resumes the timer.
  /// </summary>
  public void ResumeTimer()
  {
    if (timerDisplay != null)
    {
      timerDisplay.ResumeTimer();
    }
  }

  /// <summary>
  /// Stops and resets the timer.
  /// </summary>
  public void StopTimer()
  {
    if (timerDisplay != null)
    {
      timerDisplay.StopTimer();
    }
  }

  /// <summary>
  /// Sets the timer to a specific time (in seconds).
  /// </summary>
  public void SetTime(float timeInSeconds)
  {
    if (timerDisplay != null)
    {
      timerDisplay.SetTime(timeInSeconds);
    }
  }

  #endregion
}
