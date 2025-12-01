using UnityEngine;

public interface IDamageable
{
    void Add(AttributesType type, int value);
    //void Substract(AttributesType type, int value);
}
public enum AttributesType
{
    Fear,
    Battery
}
