using UnityEngine;

/// <summary>
/// Quick connection helper untuk menghubungkan Player system dengan HUD system.
/// Attach script ini ke Canvas atau HUD Manager untuk auto-setup.
/// </summary>
public class PlayerHUDConnector : MonoBehaviour
{
    [Header("HUD References")]
    [Tooltip("Health display component - will show fear as health (inverted)")]
    [SerializeField] private HealthDisplay healthDisplay;

    [Tooltip("Battery display component - will show flashlight battery")]
    [SerializeField] private BatteryDisplay batteryDisplay;

    [Header("Info")]
    [SerializeField] private bool debugMode = true;

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("[PlayerHUDConnector] HUD system connected to Player attributes");
            Debug.Log("- Fear system connected to HealthDisplay (inverted)");
            Debug.Log("- Battery system connected to BatteryDisplay");
        }

        ValidateSetup();
    }

    private void ValidateSetup()
    {
        if (healthDisplay == null)
        {
            Debug.LogWarning("[PlayerHUDConnector] HealthDisplay not assigned! Health HUD will not update.", this);
        }

        if (batteryDisplay == null)
        {
            Debug.LogWarning("[PlayerHUDConnector] BatteryDisplay not assigned! Battery HUD will not update.", this);
        }

        // Check if player exists
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[PlayerHUDConnector] Player not found! Make sure player has 'Player' tag.", this);
            return;
        }

        PlayerAttributes playerAttributes = player.GetComponent<PlayerAttributes>();
        if (playerAttributes == null)
        {
            Debug.LogError("[PlayerHUDConnector] PlayerAttributes component not found on player!", this);
        }
        else if (debugMode)
        {
            Debug.Log("[PlayerHUDConnector] Player found and validated successfully!");
        }
    }

    /// <summary>
    /// Auto-find and assign HUD components (useful for quick setup)
    /// </summary>
    [ContextMenu("Auto-Find HUD Components")]
    public void AutoFindHUDComponents()
    {
        if (healthDisplay == null)
        {
            healthDisplay = FindAnyObjectByType<HealthDisplay>();
            if (healthDisplay != null)
            {
                Debug.Log("[PlayerHUDConnector] HealthDisplay found and assigned!");
            }
        }

        if (batteryDisplay == null)
        {
            batteryDisplay = FindAnyObjectByType<BatteryDisplay>();
            if (batteryDisplay != null)
            {
                Debug.Log("[PlayerHUDConnector] BatteryDisplay found and assigned!");
            }
        }

        if (healthDisplay == null && batteryDisplay == null)
        {
            Debug.LogWarning("[PlayerHUDConnector] No HUD components found in scene!");
        }
    }
}
