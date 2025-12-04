using UnityEngine;

public interface IDamageable
{
    void TakeDamage(AttributesType type, int value);
}

public enum AttributesType
{
    Sanity,
    Battery
}
