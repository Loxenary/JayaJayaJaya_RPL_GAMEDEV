using UnityEngine;

/// <summary>
/// Simple Ghost entity that causes fear damage to the player.
/// Ghost is stationary (diam di tempat) and only gives damage when player touches it.
/// This is a basic dummy implementation without 3D model or animations.
/// </summary>
public class Ghost : Damage
{
    [Header("Ghost Settings")]
    [SerializeField] private bool isActive = true;

    [Header("Damage Per Contact Settings")]
    [Tooltip("If true, ghost can only damage once per collision (player must leave and re-enter)")]
    [SerializeField] private bool damageOncePerContact = true;

    [Header("Debug Info")]
    [ReadOnly]
    [SerializeField] private bool hasHitPlayer = false;
    [ReadOnly]
    [SerializeField] private bool currentlyTouchingPlayer = false;

    private float lastDamageTimeGhost = -999f;

    /// <summary>
    /// Override SendDamage to check if ghost is active
    /// </summary>
    protected override void SendDamage(IDamageable target)
    {
        Debug.Log($"[Ghost] SendDamage called on {gameObject.name} at time {Time.time:F3}");

        if (!isActive)
        {
            Debug.Log("[Ghost] Ghost is not active, no damage sent");
            return;
        }

        // ABSOLUTE PROTECTION: Time-based check at Ghost level
        if (Time.time - lastDamageTimeGhost < 0.5f)
        {
            Debug.Log($"[Ghost] Too soon! Blocked by ghost-level cooldown (time since last: {Time.time - lastDamageTimeGhost:F3}s)");
            return;
        }

        // If damageOncePerContact is enabled, only damage once until player leaves
        if (damageOncePerContact && currentlyTouchingPlayer)
        {
            Debug.Log("[Ghost] Already touching player, no additional damage (blocked by damageOncePerContact)");
            return;
        }

        // Set time IMMEDIATELY to block any simultaneous calls
        lastDamageTimeGhost = Time.time;
        currentlyTouchingPlayer = true;

        base.SendDamage(target);
        hasHitPlayer = true;

        Debug.Log($"[Ghost] Ghost damaged player! (Time: {Time.time:F2}, Flag set)");
    }    private void OnTriggerExit(Collider other)
    {
        // Reset when player leaves
        if (other.CompareTag("Player"))
        {
            currentlyTouchingPlayer = false;
        }
    }

    /// <summary>
    /// Activate or deactivate the ghost
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        // Enable/disable collider when active state changes
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = active;
        }
    }

    /// <summary>
    /// Check if ghost is currently active
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    // Visualization in editor
    private void OnDrawGizmosSelected()
    {
        // Draw ghost area indicator
        Gizmos.color = isActive ? Color.red : Color.gray;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
