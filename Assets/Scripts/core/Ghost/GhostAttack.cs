using System;
using UnityEngine;

/// <summary>
/// Sistem attack untuk ghost
/// Menangani cooldown, damage, dan efek serangan
/// </summary>
public class GhostAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float baseDamage = 15f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.5f;
    
    [Header("Sanity-Based Damage")]
    [Tooltip("Apakah damage meningkat ketika sanity player rendah")]
    [SerializeField] private bool scaleDamageWithSanity = true;
    [SerializeField] private float highSanityDamageMultiplier = 1f;
    [SerializeField] private float mediumSanityDamageMultiplier = 1.2f;
    [SerializeField] private float lowSanityDamageMultiplier = 1.5f;
    [SerializeField] private float criticalSanityDamageMultiplier = 2f;
    
    [Header("Effects")]
    [SerializeField] private bool reduceSanityOnAttack = true;
    [SerializeField] private float sanityDrainPerAttack = 5f;
    
    // State
    private float lastAttackTime;
    private bool isAttacking = false;
    private PlayerSanity targetSanity;
    
    // Events
    public event Action OnAttackStarted;
    public event Action OnAttackCompleted;
    public event Action<GameObject> OnAttackHit;
    
    // Properties
    public bool IsAttacking => isAttacking;
    public float LastAttackTime => lastAttackTime;
    
    public bool CanAttack()
    {
        return !isAttacking && Time.time - lastAttackTime >= attackCooldown;
    }
    
    /// <summary>
    /// Menyerang target
    /// </summary>
    public void Attack(GameObject target)
    {
        if (!CanAttack() || target == null) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        
        OnAttackStarted?.Invoke();
        Debug.Log($"[GhostAttack] Attacking {target.name}");
        
        // Invoke attack completion setelah duration
        Invoke(nameof(CompleteAttack), attackDuration);
        
        // Apply damage
        ApplyDamage(target);
        
        // Drain sanity
        if (reduceSanityOnAttack)
        {
            DrainSanity(target);
        }
    }
    
    private void ApplyDamage(GameObject target)
    {
        var healthComponent = target.GetComponent<IHealth>();
        if (healthComponent != null)
        {
            float finalDamage = CalculateDamage(target);
            healthComponent.TakeDamage(finalDamage);
            
            OnAttackHit?.Invoke(target);
            Debug.Log($"[GhostAttack] Dealt {finalDamage} damage to {target.name}");
        }
    }
    
    private void DrainSanity(GameObject target)
    {
        if (targetSanity == null)
        {
            targetSanity = target.GetComponent<PlayerSanity>();
        }
        
        if (targetSanity != null)
        {
            targetSanity.DecreaseSanity(sanityDrainPerAttack);
            Debug.Log($"[GhostAttack] Drained {sanityDrainPerAttack} sanity from {target.name}");
        }
    }
    
    private float CalculateDamage(GameObject target)
    {
        float damage = baseDamage;
        
        if (scaleDamageWithSanity)
        {
            if (targetSanity == null)
            {
                targetSanity = target.GetComponent<PlayerSanity>();
            }
            
            if (targetSanity != null)
            {
                damage *= GetSanityDamageMultiplier(targetSanity.CurrentSanityLevel);
            }
        }
        
        return damage;
    }
    
    private float GetSanityDamageMultiplier(PlayerSanity.SanityLevel sanityLevel)
    {
        switch (sanityLevel)
        {
            case PlayerSanity.SanityLevel.High:
                return highSanityDamageMultiplier;
                
            case PlayerSanity.SanityLevel.Medium:
                return mediumSanityDamageMultiplier;
                
            case PlayerSanity.SanityLevel.Low:
                return lowSanityDamageMultiplier;
                
            case PlayerSanity.SanityLevel.Critical:
                return criticalSanityDamageMultiplier;
                
            default:
                return highSanityDamageMultiplier;
        }
    }
    
    private void CompleteAttack()
    {
        isAttacking = false;
        OnAttackCompleted?.Invoke();
    }
    
    /// <summary>
    /// Set base damage
    /// </summary>
    public void SetBaseDamage(float damage)
    {
        baseDamage = Mathf.Max(0, damage);
    }
    
    /// <summary>
    /// Set attack cooldown
    /// </summary>
    public void SetAttackCooldown(float cooldown)
    {
        attackCooldown = Mathf.Max(0, cooldown);
    }
    
    /// <summary>
    /// Reset attack cooldown (untuk testing atau special abilities)
    /// </summary>
    public void ResetCooldown()
    {
        lastAttackTime = 0;
    }
}
