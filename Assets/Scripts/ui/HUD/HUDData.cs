using System;
using UnityEngine;

/// <summary>
/// Data structure representing HUD state for saving/loading.
/// </summary>
[Serializable]
public class HUDData
{
    public float currentHealth;
    public float maxHealth;
    public float currentBattery;
    public float maxBattery;
    public float currentTime;
    public bool isTimerRunning;
    public TimerMode timerMode;

    public HUDData()
    {
        currentHealth = 100f;
        maxHealth = 100f;
        currentBattery = 100f;
        maxBattery = 100f;
        currentTime = 0f;
        isTimerRunning = false;
        timerMode = TimerMode.CountDown;
    }

    public HUDData(float health, float maxHp, float battery, float maxBatt)
    {
        currentHealth = health;
        maxHealth = maxHp;
        currentBattery = battery;
        maxBattery = maxBatt;
        currentTime = 0f;
        isTimerRunning = false;
        timerMode = TimerMode.CountDown;
    }
}

/// <summary>
/// Data for an individual item in the HUD.
/// </summary>
[Serializable]
public class HUDItemData
{
    public ItemType itemType;
    public string iconResourcePath; // Path to sprite in Resources folder
    public int quantity;

    public HUDItemData(ItemType type, string path, int qty)
    {
        itemType = type;
        iconResourcePath = path;
        quantity = qty;
    }
}

/// <summary>
/// Configuration for HUD visual settings.
/// </summary>
[CreateAssetMenu(fileName = "HUDConfig", menuName = "Game/HUD Configuration")]
public class HUDConfig : ScriptableObject
{
    [Header("Health Settings")]
    public Color healthHighColor = Color.green;
    public Color healthMediumColor = Color.yellow;
    public Color healthLowColor = Color.red;
    [Range(0f, 1f)] public float healthMediumThreshold = 0.5f;
    [Range(0f, 1f)] public float healthLowThreshold = 0.25f;

    [Header("Battery Settings")]
    public Color batteryFullColor = Color.green;
    public Color batteryMediumColor = Color.yellow;
    public Color batteryLowColor = Color.red;
    [Range(0f, 1f)] public float batteryMediumThreshold = 0.5f;
    [Range(0f, 1f)] public float batteryLowThreshold = 0.25f;
    [Range(0f, 1f)] public float batteryCriticalThreshold = 0.15f;
    public bool blinkBatteryWhenLow = true;

    [Header("Timer Settings")]
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.yellow;
    public Color timerCriticalColor = Color.red;
    public float timerWarningThreshold = 30f;
    public float timerCriticalThreshold = 10f;
    public bool blinkTimerWhenCritical = true;

    [Header("Item Display Settings")]
    public int maxItemSlots = 5;
    public Vector2 itemIconSize = new Vector2(50, 50);
    public bool showItemQuantity = true;
    public Color foodItemColor = Color.white;
    public Color medicineItemColor = Color.white;

    [Header("Animation Settings")]
    public bool useSmoothTransitions = true;
    public float transitionSpeed = 5f;
    public bool useScaleAnimations = true;
    public float scaleAnimationDuration = 0.3f;
}

/// <summary>
/// Events for HUD system.
/// </summary>
public static class HUDEvents
{
    /// <summary>
    /// Triggered when health changes.
    /// </summary>
    public class HealthChanged
    {
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float NormalizedHealth { get; set; }

        public HealthChanged(float current, float max)
        {
            CurrentHealth = current;
            MaxHealth = max;
            NormalizedHealth = max > 0 ? current / max : 0;
        }
    }

    /// <summary>
    /// Triggered when battery changes.
    /// </summary>
    public class BatteryChanged
    {
        public float CurrentBattery { get; set; }
        public float MaxBattery { get; set; }
        public float NormalizedBattery { get; set; }

        public BatteryChanged(float current, float max)
        {
            CurrentBattery = current;
            MaxBattery = max;
            NormalizedBattery = max > 0 ? current / max : 0;
        }
    }

    /// <summary>
    /// Triggered when an item is added to the HUD.
    /// </summary>
    public class ItemAdded
    {
        public ItemType ItemType { get; set; }
        public int Quantity { get; set; }

        public ItemAdded(ItemType type, int quantity)
        {
            ItemType = type;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// Triggered when an item is removed from the HUD.
    /// </summary>
    public class ItemRemoved
    {
        public ItemType ItemType { get; set; }

        public ItemRemoved(ItemType type)
        {
            ItemType = type;
        }
    }

    /// <summary>
    /// Triggered when timer reaches zero.
    /// </summary>
    public class TimerCompleted
    {
        public float FinalTime { get; set; }

        public TimerCompleted(float time)
        {
            FinalTime = time;
        }
    }

    /// <summary>
    /// Triggered when timer state changes.
    /// </summary>
    public class TimerStateChanged
    {
        public bool IsRunning { get; set; }
        public float CurrentTime { get; set; }
        public TimerMode Mode { get; set; }

        public TimerStateChanged(bool running, float time, TimerMode mode)
        {
            IsRunning = running;
            CurrentTime = time;
            Mode = mode;
        }
    }
}
