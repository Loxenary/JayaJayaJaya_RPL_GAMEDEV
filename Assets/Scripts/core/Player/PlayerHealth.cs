using System;
using UnityEngine;

/// <summary>
/// Implementasi health system untuk player
/// </summary>
public class PlayerHealth : MonoBehaviour, IHealth, IPlayerHealth
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    
    [Header("Regeneration")]
    [SerializeField] private bool canRegenerate = false;
    [SerializeField] private float regenerationRate = 5f;
    [SerializeField] private float regenerationDelay = 3f;
    
    // Events
    public event Action<float> OnHealthChanged;
    public event Action<float> OnDamageTaken;
    public event Action<float> OnHealed;
    public event Action OnDeath;
    
    private float lastDamageTime;
    private bool isDead = false;
    
    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => isDead;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
        // Auto regeneration jika enabled
        if (canRegenerate && !isDead && currentHealth < maxHealth)
        {
            if (Time.time - lastDamageTime >= regenerationDelay)
            {
                Heal(regenerationRate * Time.deltaTime);
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        lastDamageTime = Time.time;
        
        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke(damage);
        
        Debug.Log($"[PlayerHealth] Took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead || amount <= 0) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (previousHealth != currentHealth)
        {
            OnHealthChanged?.Invoke(currentHealth);
            OnHealed?.Invoke(amount);
        }
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsAlive()
    {
        return !isDead;
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        Debug.Log("[PlayerHealth] Player died!");
        
        // Trigger respawn jika ada PlayerRespawn component
        var respawn = GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.Die("Health depleted");
        }
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
}
