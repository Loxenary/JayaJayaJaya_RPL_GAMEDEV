using UnityEngine;

/// <summary>
/// Interface untuk health system yang bisa menerima damage
/// </summary>
public interface IHealth
{
    void TakeDamage(float damage);
    void Heal(float amount);
    float GetCurrentHealth();
    float GetMaxHealth();
    bool IsAlive();
}
