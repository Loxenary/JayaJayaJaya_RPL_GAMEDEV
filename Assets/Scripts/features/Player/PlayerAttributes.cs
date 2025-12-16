using DG.Tweening;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerAttributes : MonoBehaviour, IDamageable
{
  [Header("Sanity Stats")]
  [SerializeField] int maxSanity = 1000;

  // Public property to expose max sanity (single source of truth)
  public int MaxSanity => maxSanity;
  public int CurrentSanity => currentSanity;
  public bool IsDead => isDead;

  [Header("Flashlight Stat")]
  [SerializeField] Light flashlight;
  [SerializeField] int initalBatteryValue = 100;
  [SerializeField] bool initialTogle = true;
  [SerializeField] float decrementInterval = 3f;
  [SerializeField] int decrementBatteryValue = 1;

  [Header("Sanity Drain (No Flashlight)")]
  [Tooltip("Sanity lost per second when flashlight is off/dead")]
  [SerializeField] float sanityDrainRate = 5f;

  [Header("Event")]
  public UnityEvent OnValueSanityUpdate;
  public UnityEvent OnValueBatteryUpdate;
  public UnityEvent OnPlayerDead;

  [Header("Debbuging")]
  private bool isDead;
  private int currentSanity;
  private int currentBattery;
  private bool toggleFlashlight;


  //C# Event - Sanity: 1000 = full health (100%), 0 = dead (0%)
  // Normalized value sent to HUD: 0.0 - 1.0
  public delegate void OnSanityUpdate(float normalizedValue);
  public static event OnSanityUpdate onSanityUpdate;

  public delegate void OnBatteryUpdate(float value);
  public static event OnBatteryUpdate onBatteryUpdate;

  public delegate void OnPlayerDie();
  public static event OnPlayerDie onPlayerDead;


  PlayerInputHandler input;
  private Sequence batteryDrainSequence;
  private float sanityDrainAccumulator = 0f;

  private void Awake()
  {
    input = GetComponent<PlayerInputHandler>();
    flashlight.enabled = initialTogle;
    toggleFlashlight = initialTogle;

    currentBattery = initalBatteryValue;
    currentSanity = maxSanity; // Start with full sanity
  }
  private void Start()
  {
    TweeningBattery();

    // Trigger initial events for HUD update (normalized: 0-1)
    float normalizedSanity = (float)currentSanity / maxSanity;
    onSanityUpdate?.Invoke(normalizedSanity);
    onBatteryUpdate?.Invoke(currentBattery);
  }

  private void Update()
  {
    // Drain sanity when flashlight is off/dead
    if (!flashlight.enabled || currentBattery <= 0)
    {
      DrainSanityOverTime();
    }
    else
    {
      // Reset accumulator when flashlight is on
      sanityDrainAccumulator = 0f;
    }
  }

  private void OnDestroy()
  {
    // Kill DOTween sequence when object is destroyed
    batteryDrainSequence?.Kill();
  }
  private void OnEnable()
  {
    input.OnFlashlightPerformed += ToggleFlashlight;

    InteractableAddAttributes.onInteractAddAttribute += UpdateAttributes;
    InteractableDamager.onInteractDamager += ListenDamageFromInteractable;
    //EventBus.Subscribe<int>(ListenDamageFromInteractable);
  }

  private void OnDisable()
  {
    input.OnFlashlightPerformed -= ToggleFlashlight;

    InteractableAddAttributes.onInteractAddAttribute -= UpdateAttributes;
    InteractableDamager.onInteractDamager -= ListenDamageFromInteractable;
  }
  void ListenDamageFromInteractable(int val)
  {
    AddSanity(val);
  }

  private void UpdateAttributes(AttributesType type, int value)
  {
    switch (type)
    {
      case AttributesType.Sanity:
        AddSanity(value);
        break;
      case AttributesType.Battery:
        int previousBattery = currentBattery;
        currentBattery = Mathf.Clamp(currentBattery + value, 0, 100);
        onBatteryUpdate?.Invoke(currentBattery);
        OnValueBatteryUpdate?.Invoke();
        break;
    }
  }

  private void ToggleFlashlight()
  {
    if (currentBattery <= 0)
      return;
    toggleFlashlight = !toggleFlashlight;
    flashlight.enabled = toggleFlashlight;


  }


  
  /// <summary>
  /// Drains sanity over time when flashlight is off/dead (called per frame)
  /// </summary>
  private void DrainSanityOverTime()
  {
    if (isDead)
      return;

    // Accumulate drain over time (sanityDrainRate is per second)
    sanityDrainAccumulator += sanityDrainRate * Time.deltaTime;

    // Only apply drain when accumulated value is >= 1
    if (sanityDrainAccumulator >= 1f)
    {
      int drainAmount = Mathf.FloorToInt(sanityDrainAccumulator);
      sanityDrainAccumulator -= drainAmount;

      ReduceSanity(drainAmount);
    }
  }

  /// <summary>
  /// Add sanity (healing). Positive value = heal.
  /// </summary>
  void AddSanity(int value)
  {
    if (isDead)
      return;

    int previousSanity = currentSanity;
    currentSanity = Mathf.Clamp(currentSanity + value, 0, maxSanity);

    float normalizedSanity = (float)currentSanity / maxSanity;

    onSanityUpdate?.Invoke(normalizedSanity);
    OnValueSanityUpdate?.Invoke();
  }

  /// <summary>
  /// Reduce sanity (damage). Positive value = damage taken.
  /// </summary>
  void ReduceSanity(int damage)
  {
    if (isDead)
      return;

    int previousSanity = currentSanity;
    currentSanity = Mathf.Clamp(currentSanity - damage, 0, maxSanity);

    float normalizedSanity = (float)currentSanity / maxSanity;

    if (currentSanity <= 0)
    {
      isDead = true;

      // Freeze player immediately
      FreezePlayer();

      onPlayerDead?.Invoke();
      OnPlayerDead?.Invoke();
      return;
    }

    onSanityUpdate?.Invoke(normalizedSanity);
    OnValueSanityUpdate?.Invoke();
  }

  void AddBattery(int value)
  {
    int previousBattery = currentBattery;
    currentBattery = Mathf.Clamp(currentBattery + value, 0, 100);

    // Trigger events for UI update
    onBatteryUpdate?.Invoke(currentBattery);
    OnValueBatteryUpdate?.Invoke();
  }

  void TweeningBattery()
  {
    // Kill existing sequence to prevent multiple sequences running
    batteryDrainSequence?.Kill();

    batteryDrainSequence = DOTween.Sequence()
        .AppendInterval(decrementInterval)
        .AppendCallback(() =>
        {
          if (toggleFlashlight && currentBattery > 0)
          {
            int previousBattery = currentBattery;

            if (currentBattery - decrementBatteryValue > 0)
            {
              currentBattery -= decrementBatteryValue;
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
        })
        .SetLoops(-1)
        .SetUpdate(UpdateType.Normal, false)
        .SetAutoKill(false);
  }

  /// <summary>
  /// IDamageable implementation - receives damage
  /// </summary>
  public void TakeDamage(AttributesType type, int value)
  {
    if (isDead)
      return;

    switch (type)
    {
      case AttributesType.Sanity:
        ReduceSanity(value); // Damage reduces sanity
        break;
      case AttributesType.Battery:
        AddBattery(-value); // Negative to reduce battery
        break;
    }
  }

  /// <summary>
  /// Freeze player when dead (disable movement and input)
  /// </summary>
  private void FreezePlayer()
  {
    // Disable flashlight
    if (flashlight != null)
    {
      flashlight.enabled = false;
      toggleFlashlight = false;
    }

    // Freeze input
    if (input != null)
    {
      input.SetFrozen(true);
    }

    // Freeze movement (need to access PlayerController)
    var playerController = GetComponent<PlayerController>();
    if (playerController != null)
    {
      playerController.SetFrozen(true);
    }

    // Lock and show cursor for death UI
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }
}
