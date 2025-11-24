using System;
using UnityEngine;

/// <summary>
/// Sistem sanity untuk player yang menurun seiring waktu atau karena kejadian tertentu
/// Sanity yang rendah akan mempengaruhi perilaku ghost (lebih agresif)
/// </summary>
public class PlayerSanity : MonoBehaviour
{
    [Header("Sanity Settings")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity = 100f;
    
    [Header("Sanity Decay")]
    [Tooltip("Sanity berkurang per detik ketika dalam kondisi tertentu")]
    [SerializeField] private float passiveDecayRate = 0.5f;
    [SerializeField] private bool enablePassiveDecay = false;
    
    [Header("Sanity Thresholds")]
    [Tooltip("Threshold untuk level sanity berbeda (0-1)")]
    [SerializeField] private float highSanityThreshold = 0.7f;
    [SerializeField] private float mediumSanityThreshold = 0.4f;
    [SerializeField] private float lowSanityThreshold = 0.2f;
    
    [Header("Recovery")]
    [SerializeField] private float recoveryRate = 1f;
    [SerializeField] private bool canRecover = true;
    
    // Events
    public event Action<float> OnSanityChanged;
    public event Action<SanityLevel> OnSanityLevelChanged;
    public event Action OnSanityDepleted;
    
    private SanityLevel currentLevel = SanityLevel.High;
    private bool isDepleted = false;
    
    public enum SanityLevel
    {
        High,
        Medium,
        Low,
        Critical
    }
    
    // Properties
    public float CurrentSanity => currentSanity;
    public float MaxSanity => maxSanity;
    public float SanityPercentage => currentSanity / maxSanity;
    public SanityLevel CurrentSanityLevel => currentLevel;
    public bool IsDepleted => isDepleted;
    
    private void Start()
    {
        currentSanity = maxSanity;
        UpdateSanityLevel();
    }
    
    private void Update()
    {
        if (enablePassiveDecay && currentSanity > 0)
        {
            DecreaseSanity(passiveDecayRate * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Mengurangi sanity player
    /// </summary>
    public void DecreaseSanity(float amount)
    {
        if (amount <= 0) return;
        
        float previousSanity = currentSanity;
        currentSanity = Mathf.Max(0, currentSanity - amount);
        
        if (previousSanity != currentSanity)
        {
            OnSanityChanged?.Invoke(currentSanity);
            UpdateSanityLevel();
            
            if (currentSanity <= 0 && !isDepleted)
            {
                isDepleted = true;
                OnSanityDepleted?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Menambah sanity player
    /// </summary>
    public void IncreaseSanity(float amount)
    {
        if (!canRecover || amount <= 0) return;
        
        float previousSanity = currentSanity;
        currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
        
        if (previousSanity != currentSanity)
        {
            if (isDepleted && currentSanity > 0)
            {
                isDepleted = false;
            }
            
            OnSanityChanged?.Invoke(currentSanity);
            UpdateSanityLevel();
        }
    }
    
    /// <summary>
    /// Mengaktifkan recovery sanity secara bertahap
    /// </summary>
    public void StartRecovery()
    {
        canRecover = true;
        StartCoroutine(RecoveryCoroutine());
    }
    
    /// <summary>
    /// Menghentikan recovery sanity
    /// </summary>
    public void StopRecovery()
    {
        canRecover = false;
        StopCoroutine(RecoveryCoroutine());
    }
    
    private System.Collections.IEnumerator RecoveryCoroutine()
    {
        while (canRecover && currentSanity < maxSanity)
        {
            IncreaseSanity(recoveryRate * Time.deltaTime);
            yield return null;
        }
    }
    
    /// <summary>
    /// Reset sanity ke maksimal
    /// </summary>
    public void ResetSanity()
    {
        currentSanity = maxSanity;
        isDepleted = false;
        OnSanityChanged?.Invoke(currentSanity);
        UpdateSanityLevel();
    }
    
    /// <summary>
    /// Set nilai sanity langsung
    /// </summary>
    public void SetSanity(float value)
    {
        currentSanity = Mathf.Clamp(value, 0, maxSanity);
        OnSanityChanged?.Invoke(currentSanity);
        UpdateSanityLevel();
    }
    
    /// <summary>
    /// Enable atau disable passive decay
    /// </summary>
    public void SetPassiveDecay(bool enabled)
    {
        enablePassiveDecay = enabled;
    }
    
    /// <summary>
    /// Update level sanity berdasarkan threshold
    /// </summary>
    private void UpdateSanityLevel()
    {
        SanityLevel previousLevel = currentLevel;
        float sanityRatio = SanityPercentage;
        
        if (sanityRatio >= highSanityThreshold)
        {
            currentLevel = SanityLevel.High;
        }
        else if (sanityRatio >= mediumSanityThreshold)
        {
            currentLevel = SanityLevel.Medium;
        }
        else if (sanityRatio >= lowSanityThreshold)
        {
            currentLevel = SanityLevel.Low;
        }
        else
        {
            currentLevel = SanityLevel.Critical;
        }
        
        if (previousLevel != currentLevel)
        {
            OnSanityLevelChanged?.Invoke(currentLevel);
            Debug.Log($"[PlayerSanity] Sanity level changed: {previousLevel} -> {currentLevel}");
        }
    }
    
    // Utility methods untuk trigger penurunan sanity dari external events
    public void OnGhostSeen(float sanityLoss = 5f)
    {
        DecreaseSanity(sanityLoss);
    }
    
    public void OnScareEvent(float sanityLoss = 10f)
    {
        DecreaseSanity(sanityLoss);
    }
    
    public void OnDarkArea(float sanityLossPerSecond = 1f)
    {
        DecreaseSanity(sanityLossPerSecond * Time.deltaTime);
    }
}
