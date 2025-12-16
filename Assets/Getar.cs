using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class Getar : MonoBehaviour
{

    private void OnEnable()
    {
        EventBus.Subscribe<DamageVisualFeedback.TriggerPulseEvent>(GetarPulse);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DamageVisualFeedback.TriggerPulseEvent>(GetarPulse);

    }


    private void GetarPulse(DamageVisualFeedback.TriggerPulseEvent evt)
    {
        Getars();
    }
    public CinemachineImpulseSource cinemachineImpulseSource;

    public UnityEvent goyangin;

    [ContextMenu("Goyang Bang")]
    public void GoyangBang()
    {
        goyangin?.Invoke();
    }

    [ContextMenu("Getar")]
    public void Getars()
    {
        cinemachineImpulseSource.GenerateImpulse();
    }
}
