using UnityEngine;
using TMPro;

/// <summary>
/// Central manager for the HUD system. Coordinates all HUD elements including health, battery, items, and timer.
/// </summary>
public class HUDManager : FadeShowHideProceduralWithEventBus<HUDManager>
{
    [Header("HUD Components")]
    [SerializeField] private HealthDisplay healthDisplay;
    [SerializeField] private BatteryDisplay batteryDisplay;
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
    }

    private void TryFindComponents()
    {
        if (healthDisplay == null)
            healthDisplay = GetComponentInChildren<HealthDisplay>();

        if (batteryDisplay == null)
            batteryDisplay = GetComponentInChildren<BatteryDisplay>();

        if (itemIconsDisplay == null)
            itemIconsDisplay = GetComponentInChildren<ItemIconsDisplay>();

        if (timerDisplay == null)
            timerDisplay = GetComponentInChildren<TimerDisplay>();
    }

    private void ValidateComponents()
    {
        if (healthDisplay == null)
            Debug.LogWarning("[HUDManager] HealthDisplay not assigned or found.", this);

        if (batteryDisplay == null)
            Debug.LogWarning("[HUDManager] BatteryDisplay not assigned or found.", this);

        if (itemIconsDisplay == null)
            Debug.LogWarning("[HUDManager] ItemIconsDisplay not assigned or found.", this);

        if (timerDisplay == null)
            Debug.LogWarning("[HUDManager] TimerDisplay not assigned or found.", this);
    }

    #region Public API - Health

    /// <summary>
    /// Updates the health display.
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthDisplay != null)
        {
            healthDisplay.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Updates health with a single normalized value (0-1).
    /// </summary>
    public void UpdateHealthNormalized(float normalizedHealth)
    {
        if (healthDisplay != null)
        {
            healthDisplay.UpdateHealthNormalized(normalizedHealth);
        }
    }

    #endregion

    #region Public API - Battery

    /// <summary>
    /// Updates the battery display.
    /// </summary>
    /// <param name="currentBattery">Current battery value</param>
    /// <param name="maxBattery">Maximum battery value</param>
    public void UpdateBattery(float currentBattery, float maxBattery)
    {
        if (batteryDisplay != null)
        {
            batteryDisplay.UpdateBattery(currentBattery, maxBattery);
        }
    }

    /// <summary>
    /// Updates battery with a single normalized value (0-1).
    /// </summary>
    public void UpdateBatteryNormalized(float normalizedBattery)
    {
        if (batteryDisplay != null)
        {
            batteryDisplay.UpdateBatteryNormalized(normalizedBattery);
        }
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
