using UnityEngine;
using UnityEngine.Events;

public class Damage : MonoBehaviour
{
    [Header("Damage Value")]
    [Range(0, 100)]
    [SerializeField] protected int fearAddValue = 10;

    [Header("Damage Cooldown")]
    [Tooltip("Minimum time between damage hits (in seconds)")]
    [SerializeField] protected float damageCooldown = 1f;

    [Header("Event")]
    public UnityEvent OnDoneSendDamage;

    private float lastDamageTime = -999f;

    protected virtual void SendDamage(IDamageable target)
    {
        target.Add(AttributesType.Fear, fearAddValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only damage player
        if (!other.CompareTag("Player"))
            return;

        // ATOMIC LOCK: Check AND set in one operation to prevent race condition
        float currentTime = Time.time;
        float timeSinceLast = currentTime - lastDamageTime;

        if (timeSinceLast < damageCooldown)
            return;

        // Lock immediately to block simultaneous calls
        lastDamageTime = currentTime;

        IDamageable i = other.GetComponent<IDamageable>();

        if (i != null)
        {
            SendDamage(i);
            OnDoneSendDamage?.Invoke();
        }
    }
}
