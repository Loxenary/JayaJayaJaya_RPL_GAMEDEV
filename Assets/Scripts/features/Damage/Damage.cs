using UnityEngine;
using UnityEngine.Events;

public class Damage : MonoBehaviour
{
    [Header("Damage Value")]
    [Range(0, 100)]
    [SerializeField] protected int fearAddValue = 10;

    [Header("Event")]
    public UnityEvent OnDoneSendDamage;
    protected virtual void SendDamage(IDamageable target)
    {
        target.Add(AttributesType.Fear, fearAddValue);
    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable i = other.GetComponent<IDamageable>();

        if (i != null)
        {
            SendDamage(i);
            OnDoneSendDamage?.Invoke();

            Debug.Log($"[Damage] Sent {fearAddValue} fear damage to {other.gameObject.name}");
        }
    }
}
