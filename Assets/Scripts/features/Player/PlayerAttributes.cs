using DG.Tweening;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent (typeof(PlayerInputHandler))]
public class PlayerAttributes : MonoBehaviour,IDamageable
{
    [Header("Fear Stats")]
    [SerializeField] int maxFear = 100;
    [Header("Flashlight Stat")]
    [SerializeField] Light flashlight;
    [SerializeField] int initalBatteryValue = 100;
    [SerializeField] bool initialTogle = true;
    [SerializeField] float decrementInterval = 1f;
    [SerializeField] int decrementBatteryaValue = 2;

    [Header("Event")]
    public UnityEvent OnValueFearUpdate;
    public UnityEvent OnValueBatteryUpdate;
    public UnityEvent OnPlayerDead;

    [Header("Debbuging")]
    [ReadOnly]
    [SerializeField] float currentFear = 0;
    [ReadOnly]
    [SerializeField] int currentBattery;
    [ReadOnly]
    [SerializeField] bool toggleFlashlight;
    [ReadOnly]
    [SerializeField] bool isDead;


    //C# Event
    public delegate void OnFearUpdate(float value);
    public static event OnFearUpdate onFearUpdate;

    public delegate void OnBatteryUpdate(float value);
    public static event OnBatteryUpdate onBatteryUpdate;

    public delegate void OnPlayerDie();
    public static event OnPlayerDie onPlayerDead;


    PlayerInputHandler input;
    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        flashlight.enabled = initialTogle;
        toggleFlashlight = initialTogle;

        currentBattery = initalBatteryValue;
    }
    private void Start()
    {
        TweeningBattery();

        // Trigger initial events for HUD update
        onFearUpdate?.Invoke(currentFear);
        onBatteryUpdate?.Invoke(currentBattery);
    }
    private void OnEnable()
    {
        input.OnFlashlightPerformed += ToggleFlashlight;


        InteractableAddAttributes.onInteractAddAttribute += UpdateAttributes;
    }

    private void OnDisable()
    {
        input.OnFlashlightPerformed -= ToggleFlashlight;

        InteractableAddAttributes.onInteractAddAttribute -= UpdateAttributes;
    }
    private void UpdateAttributes(AttributesType type, int value)
    {
        switch (type)
        {
            case AttributesType.Fear:
                SubstractFear(value);
                break;
            case AttributesType.Battery:
                currentBattery = Mathf.Clamp(currentBattery + value,0,100);
                onBatteryUpdate?.Invoke(currentBattery);
                OnValueBatteryUpdate?.Invoke();
                Debug.Log("Add Battery By Interactable");
                break;
        }
    }
    private void ToggleFlashlight()
    {
        if (currentBattery <= 0)
            return;

        toggleFlashlight = !toggleFlashlight;
        flashlight.enabled = toggleFlashlight;
        Debug.Log("Toggle Flashlight");
    }
    void AddFear(int value)
    {
        currentFear = Mathf.Clamp(currentFear+value,0,maxFear);

        if(currentFear == maxFear)
        {
            isDead = true;

            onPlayerDead?.Invoke();
            OnPlayerDead?.Invoke();

            Debug.Log("Player Dead");
            return;
        }

        onFearUpdate?.Invoke(currentFear);
        OnValueFearUpdate?.Invoke();

        Debug.Log("Fear Player added by = " + value);

    }
    void SubstractFear(int value)
    {
        float temp = ((float)value / 100) * currentFear;

        Debug.Log("Temp = " + temp);

        currentFear -= temp;

        onFearUpdate?.Invoke(currentFear);
        OnValueFearUpdate?.Invoke();
    }
    void AddBattery(int value)
    {
        currentBattery = Mathf.Clamp(currentBattery + value, 0, 100);

        // Trigger events for UI update
        onBatteryUpdate?.Invoke(currentBattery);
        OnValueBatteryUpdate?.Invoke();
    }
    void TweeningBattery()
    {
        DOTween.Sequence().SetDelay(decrementInterval).OnStepComplete(() =>
        {
            if (toggleFlashlight)
            {
                if (currentBattery - decrementBatteryaValue > 0) {
                    currentBattery-= decrementBatteryaValue;
                }
                else
                {
                    toggleFlashlight = false;
                    flashlight.enabled = toggleFlashlight;
                    currentBattery = 0;
                }

                // Trigger events for UI update
                onBatteryUpdate?.Invoke(currentBattery);
                OnValueBatteryUpdate?.Invoke();
            }
        }).SetLoops(-1);
    }
    public void Add(AttributesType type, int value)
    {
        switch (type)
        {
            case AttributesType.Fear:
                AddFear(value);
                break;
            case AttributesType.Battery:
                AddBattery(value);
                break;
        }
    }
}
