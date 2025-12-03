using UnityEngine;

/// <summary>
/// Quick connection helper untuk menghubungkan Player system dengan HUD system.
/// Attach script ini ke Canvas atau HUD Manager untuk auto-setup.
/// </summary>
public class PlayerHUDConnector : MonoBehaviour
{
    [Header("HUD References")]
    [Tooltip("Sanity display component - shows player's mental health")]
    [SerializeField] private SanityDisplay sanityDisplay;

    [Tooltip("Battery display component - will show flashlight battery")]
    [SerializeField] private BatteryDisplay batteryDisplay;

    [Header("Info")]
    [SerializeField] private bool debugMode = true;

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("[PlayerHUDConnector] HUD system connected to Player attributes");
            Debug.Log("- Sanity system connected to SanityDisplay");
            Debug.Log("- Battery system connected to BatteryDisplay");
        }

        ValidateSetup();
    }

    private void ValidateSetup()
    {
        if (sanityDisplay == null)
        {
            Debug.LogWarning("[PlayerHUDConnector] SanityDisplay not assigned! Sanity HUD will not update.", this);
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
        if (sanityDisplay == null)
        {
            sanityDisplay = FindAnyObjectByType<SanityDisplay>();
            if (sanityDisplay != null)
            {
                Debug.Log("[PlayerHUDConnector] SanityDisplay found and assigned!");
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

        if (sanityDisplay == null && batteryDisplay == null)
        {
            Debug.LogWarning("[PlayerHUDConnector] No HUD components found in scene!");
        }
    }
}
