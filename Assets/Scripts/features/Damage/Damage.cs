using UnityEngine;
using UnityEngine.Events;

public class Damage : MonoBehaviour
{
    [Header("Damage Value")]
    [Range(0, 100)]
    [SerializeField] int fearAddValue = 10;

    [Header("Event")]
    public UnityEvent OnDoneSendDamage;
    protected virtual void SendDamage(IDamageable target)
    {
        target.Add(AttributesType.Fear, fearAddValue);
    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable i = other.GetComponent<IDamageable>();

        SendDamage(i);

        OnDoneSendDamage?.Invoke();
    }
}
