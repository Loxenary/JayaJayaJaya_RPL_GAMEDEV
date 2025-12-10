using UnityEngine;

/// <summary>
/// Example script demonstrating how to use the HUD system.
/// Attach this to a GameObject in your scene for testing.
/// </summary>
public class HUDExample : MonoBehaviour
{
  [Header("Test Settings")]
  [SerializeField] private bool enableTestControls = true;
  [SerializeField] private Sprite testFoodIcon;
  [SerializeField] private Sprite testMedicineIcon;

  [Header("Test Values")]
  [SerializeField] private float testSanityDamage = 100f;
  [SerializeField] private float testSanityHeal = 150f;
  [SerializeField] private float testBatteryDrain = 5f;
  [SerializeField] private float testBatteryCharge = 10f;

  private float currentSanity = 1000f;
  private float maxSanity = 1000f;
  private float currentBattery = 100f;
  private float maxBattery = 100f;

  private void Start()
  {
    // Initialize HUD with starting values
    InitializeHUD();
  }

  private void Update()
  {
    if (!enableTestControls) return;

    // Test controls
    HandleTestInput();
  }

  private void InitializeHUD()
  {
    // Show the HUD
    UIManager.Open<HUDManager>();

    // Set initial sanity
    HUDManager hudManager = FindAnyObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.UpdateSanity(currentSanity, maxSanity);
      hudManager.UpdateBattery(currentBattery, maxBattery);
    }
  }

  private void HandleTestInput()
  {
    HUDManager hudManager = FindAnyObjectByType<HUDManager>();
    if (hudManager == null) return;

    // Sanity controls
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      TakeDamage(testSanityDamage);
    }
    if (Input.GetKeyDown(KeyCode.Alpha2))
    {
      Heal(testSanityHeal);
    }

    // Battery controls
    if (Input.GetKeyDown(KeyCode.Alpha3))
    {
      DrainBattery(testBatteryDrain);
    }
    if (Input.GetKeyDown(KeyCode.Alpha4))
    {
      ChargeBattery(testBatteryCharge);
    }

    // Item controls
    if (Input.GetKeyDown(KeyCode.Alpha5) && testFoodIcon != null)
    {
      AddFood();
    }
    if (Input.GetKeyDown(KeyCode.Alpha6) && testMedicineIcon != null)
    {
      AddMedicine();
    }
    if (Input.GetKeyDown(KeyCode.Alpha7))
    {
      RemoveFood();
    }
    if (Input.GetKeyDown(KeyCode.Alpha8))
    {
      RemoveMedicine();
    }

    // Timer controls
    if (Input.GetKeyDown(KeyCode.Alpha9))
    {
      StartCountdownTimer(60f);
    }
    if (Input.GetKeyDown(KeyCode.Alpha0))
    {
      StartCountUpTimer();
    }
    if (Input.GetKeyDown(KeyCode.P))
    {
      ToggleTimerPause();
    }
    if (Input.GetKeyDown(KeyCode.R))
    {
      StopTimer();
    }

    // HUD visibility
    if (Input.GetKeyDown(KeyCode.H))
    {
      ToggleHUD();
    }
  }

  #region Sanity Methods

  public void TakeDamage(float damage)
  {
    currentSanity = Mathf.Max(0, currentSanity - damage);
    UpdateSanityDisplay();
    Debug.Log($"[HUDExample] Took {damage} sanity damage. Sanity: {currentSanity}/{maxSanity}");
  }

  public void Heal(float amount)
  {
    currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
    UpdateSanityDisplay();
    Debug.Log($"[HUDExample] Healed {amount} sanity. Sanity: {currentSanity}/{maxSanity}");
  }

  public void SetMaxSanity(float newMaxSanity)
  {
    maxSanity = newMaxSanity;
    currentSanity = Mathf.Min(currentSanity, maxSanity);
    UpdateSanityDisplay();
  }

  private void UpdateSanityDisplay()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.UpdateSanity(currentSanity, maxSanity);
    }
  }

  #endregion

  #region Battery Methods

  public void DrainBattery(float amount)
  {
    currentBattery = Mathf.Max(0, currentBattery - amount);
    UpdateBatteryDisplay();
    Debug.Log($"[HUDExample] Battery drained {amount}. Battery: {currentBattery}/{maxBattery}");
  }

  public void ChargeBattery(float amount)
  {
    currentBattery = Mathf.Min(maxBattery, currentBattery + amount);
    UpdateBatteryDisplay();
    Debug.Log($"[HUDExample] Battery charged {amount}. Battery: {currentBattery}/{maxBattery}");
  }

  public void SetMaxBattery(float newMaxBattery)
  {
    maxBattery = newMaxBattery;
    currentBattery = Mathf.Min(currentBattery, maxBattery);
    UpdateBatteryDisplay();
  }

  private void UpdateBatteryDisplay()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.UpdateBattery(currentBattery, maxBattery);
    }
  }

  #endregion

  #region Item Methods

  public void AddFood()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null && testFoodIcon != null)
    {
      hudManager.AddItemIcon(ItemType.Food, testFoodIcon, 1);
      Debug.Log("[HUDExample] Added food item");
    }
  }

  public void AddMedicine()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null && testMedicineIcon != null)
    {
      hudManager.AddItemIcon(ItemType.Medicine, testMedicineIcon, 1);
      Debug.Log("[HUDExample] Added medicine item");
    }
  }

  public void RemoveFood()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.RemoveItemIcon(ItemType.Food);
      Debug.Log("[HUDExample] Removed food item");
    }
  }

  public void RemoveMedicine()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.RemoveItemIcon(ItemType.Medicine);
      Debug.Log("[HUDExample] Removed medicine item");
    }
  }

  #endregion

  #region Timer Methods

  public void StartCountdownTimer(float duration)
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.StartCountdown(duration);
      Debug.Log($"[HUDExample] Started countdown timer: {duration} seconds");
    }
  }

  public void StartCountUpTimer()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.StartCountUp();
      Debug.Log("[HUDExample] Started count-up timer");
    }
  }

  private bool isTimerPaused = false;
  public void ToggleTimerPause()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      if (isTimerPaused)
      {
        hudManager.ResumeTimer();
        Debug.Log("[HUDExample] Timer resumed");
      }
      else
      {
        hudManager.PauseTimer();
        Debug.Log("[HUDExample] Timer paused");
      }
      isTimerPaused = !isTimerPaused;
    }
  }

  public void StopTimer()
  {
    var hudManager = FindFirstObjectByType<HUDManager>();
    if (hudManager != null)
    {
      hudManager.StopTimer();
      isTimerPaused = false;
      Debug.Log("[HUDExample] Timer stopped");
    }
  }

  #endregion

  #region HUD Visibility

  private bool isHUDVisible = true;
  public void ToggleHUD()
  {
    if (isHUDVisible)
    {
      UIManager.Close<HUDManager>();
      Debug.Log("[HUDExample] HUD hidden");
    }
    else
    {
      UIManager.Open<HUDManager>();
      Debug.Log("[HUDExample] HUD shown");
    }
    isHUDVisible = !isHUDVisible;
  }

  #endregion

  private void OnGUI()
  {
    if (!enableTestControls) return;

    // Display controls on screen
    GUI.Box(new Rect(10, 10, 300, 280), "HUD Test Controls");

    int yPos = 35;
    int lineHeight = 20;

    GUI.Label(new Rect(20, yPos, 280, lineHeight), "1 - Take Damage");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "2 - Heal");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "3 - Drain Battery");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "4 - Charge Battery");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "5 - Add Food");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "6 - Add Medicine");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "7 - Remove Food");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "8 - Remove Medicine");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "9 - Start Countdown (60s)");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "0 - Start Count Up");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "P - Pause/Resume Timer");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "R - Reset Timer");
    yPos += lineHeight;
    GUI.Label(new Rect(20, yPos, 280, lineHeight), "H - Toggle HUD Visibility");
  }
}
