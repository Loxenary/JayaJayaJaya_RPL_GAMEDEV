using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Timer mode for the display.
/// </summary>
public enum TimerMode
{
    CountDown,
    CountUp
}

/// <summary>
/// Displays a game timer that can count up or down.
/// </summary>
public class TimerDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display the timer")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Tooltip("Optional icon for the timer")]
    [SerializeField] private UnityEngine.UI.Image timerIcon;

    [Header("Timer Settings")]
    [Tooltip("Initial timer mode")]
    [SerializeField] private TimerMode mode = TimerMode.CountDown;

    [Tooltip("Initial duration for countdown (in seconds)")]
    [SerializeField] private float initialDuration = 60f;

    [Tooltip("Start timer automatically on awake")]
    [SerializeField] private bool autoStart = false;

    [Header("Display Format")]
    [Tooltip("Time format: HH:MM:SS, MM:SS, SS, or custom")]
    [SerializeField] private TimeFormat timeFormat = TimeFormat.MM_SS;

    [Tooltip("Custom format string (e.g., '{0:00}:{1:00}:{2:00}')")]
    [SerializeField] private string customFormat = "{0:00}:{1:00}:{2:00}";

    [Header("Visual Settings")]
    [Tooltip("Color when time is normal")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Color when time is running low")]
    [SerializeField] private Color warningColor = Color.yellow;

    [Tooltip("Color when time is critical")]
    [SerializeField] private Color criticalColor = Color.red;

    [Tooltip("Time threshold for warning (in seconds)")]
    [SerializeField] private float warningThreshold = 30f;

    [Tooltip("Time threshold for critical (in seconds)")]
    [SerializeField] private float criticalThreshold = 10f;

    [Tooltip("Blink when time is critical")]
    [SerializeField] private bool blinkWhenCritical = true;

    [Tooltip("Blink speed (times per second)")]
    [SerializeField] private float blinkSpeed = 2f;

    [Header("Events")]
    [Tooltip("Invoked when countdown reaches zero")]
    public UnityEngine.Events.UnityEvent OnCountdownComplete;

    [Tooltip("Invoked when timer reaches warning threshold")]
    public UnityEngine.Events.UnityEvent OnWarningThreshold;

    [Tooltip("Invoked when timer reaches critical threshold")]
    public UnityEngine.Events.UnityEvent OnCriticalThreshold;

    private float currentTime;
    private float targetTime;
    private bool isRunning;
    private bool hasReachedWarning;
    private bool hasReachedCritical;
    private float blinkTimer;
    private bool isBlinkVisible = true;

    public enum TimeFormat
    {
        HH_MM_SS,   // Hours:Minutes:Seconds
        MM_SS,      // Minutes:Seconds
        SS,         // Seconds only
        Custom      // Use custom format string
    }

    private void Awake()
    {
        ValidateComponents();

        if (mode == TimerMode.CountDown)
        {
            currentTime = initialDuration;
        }
        else
        {
            currentTime = 0f;
        }

        UpdateDisplay();

        if (autoStart)
        {
            if (mode == TimerMode.CountDown)
            {
                StartCountdown(initialDuration);
            }
            else
            {
                StartCountUp();
            }
        }
    }

    private void ValidateComponents()
    {
        if (timerText == null)
        {
            Debug.LogError("[TimerDisplay] Timer text is not assigned!", this);
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        float previousTime = currentTime;

        if (mode == TimerMode.CountDown)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;
                OnCountdownComplete?.Invoke();
            }

            CheckThresholds();
        }
        else // CountUp
        {
            currentTime += Time.deltaTime;
        }

        UpdateDisplay();
        UpdateVisuals();
    }

    private void CheckThresholds()
    {
        if (currentTime <= criticalThreshold && !hasReachedCritical)
        {
            hasReachedCritical = true;
            OnCriticalThreshold?.Invoke();
        }
        else if (currentTime <= warningThreshold && !hasReachedWarning)
        {
            hasReachedWarning = true;
            OnWarningThreshold?.Invoke();
        }
    }

    private void UpdateVisuals()
    {
        if (timerText == null) return;

        Color targetColor = normalColor;

        if (mode == TimerMode.CountDown)
        {
            if (currentTime <= criticalThreshold)
            {
                targetColor = criticalColor;

                // Blinking effect
                if (blinkWhenCritical)
                {
                    blinkTimer += Time.deltaTime * blinkSpeed;
                    bool shouldBeVisible = Mathf.Sin(blinkTimer * Mathf.PI) > 0;

                    if (shouldBeVisible != isBlinkVisible)
                    {
                        isBlinkVisible = shouldBeVisible;
                        Color color = timerText.color;
                        color.a = isBlinkVisible ? 1f : 0.3f;
                        timerText.color = color;
                    }
                }
            }
            else if (currentTime <= warningThreshold)
            {
                targetColor = warningColor;
            }
            else
            {
                targetColor = normalColor;
            }
        }

        // Set color (preserve alpha if blinking)
        if (!blinkWhenCritical || currentTime > criticalThreshold)
        {
            timerText.color = targetColor;
        }
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        int hours = Mathf.FloorToInt(currentTime / 3600f);
        int minutes = Mathf.FloorToInt((currentTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int milliseconds = Mathf.FloorToInt((currentTime * 100f) % 100f);

        string displayText = FormatTime(hours, minutes, seconds, milliseconds);
        timerText.text = displayText;
    }

    private string FormatTime(int hours, int minutes, int seconds, int milliseconds)
    {
        switch (timeFormat)
        {
            case TimeFormat.HH_MM_SS:
                return $"{hours:00}:{minutes:00}:{seconds:00}";

            case TimeFormat.MM_SS:
                return $"{minutes:00}:{seconds:00}";

            case TimeFormat.SS:
                return $"{seconds}";

            case TimeFormat.Custom:
                return string.Format(customFormat, hours, minutes, seconds, milliseconds);

            default:
                return $"{minutes:00}:{seconds:00}";
        }
    }

    #region Public API

    /// <summary>
    /// Starts the timer in countdown mode.
    /// </summary>
    public void StartCountdown(float duration)
    {
        mode = TimerMode.CountDown;
        targetTime = duration;
        currentTime = duration;
        isRunning = true;
        hasReachedWarning = false;
        hasReachedCritical = false;
        UpdateDisplay();
    }

    /// <summary>
    /// Starts the timer in count-up mode.
    /// </summary>
    public void StartCountUp()
    {
        mode = TimerMode.CountUp;
        currentTime = 0f;
        isRunning = true;
        UpdateDisplay();
    }

    /// <summary>
    /// Pauses the timer.
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resumes the timer.
    /// </summary>
    public void ResumeTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// Stops and resets the timer.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;

        if (mode == TimerMode.CountDown)
        {
            currentTime = targetTime;
        }
        else
        {
            currentTime = 0f;
        }

        hasReachedWarning = false;
        hasReachedCritical = false;
        UpdateDisplay();
    }

    /// <summary>
    /// Sets the timer to a specific time (in seconds).
    /// </summary>
    public void SetTime(float timeInSeconds)
    {
        currentTime = timeInSeconds;

        if (mode == TimerMode.CountDown)
        {
            targetTime = timeInSeconds;
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Adds time to the current timer (positive or negative).
    /// </summary>
    public void AddTime(float seconds)
    {
        currentTime = Mathf.Max(0, currentTime + seconds);
        UpdateDisplay();
    }

    /// <summary>
    /// Gets the current time in seconds.
    /// </summary>
    public float GetCurrentTime()
    {
        return currentTime;
    }

    /// <summary>
    /// Gets whether the timer is currently running.
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }

    /// <summary>
    /// Sets the timer icon sprite.
    /// </summary>
    public void SetTimerIcon(Sprite icon)
    {
        if (timerIcon != null)
        {
            timerIcon.sprite = icon;
        }
    }

    /// <summary>
    /// Shows or hides the timer display.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    #endregion
}
