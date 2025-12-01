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

    [Header("Debug Info")]
    [ReadOnly]
    [SerializeField] private bool hasHitPlayer = false;

    /// <summary>
    /// Override SendDamage to check if ghost is active
    /// </summary>
    protected override void SendDamage(IDamageable target)
    {
        if (!isActive)
        {
            Debug.Log("[Ghost] Ghost is not active, no damage sent");
            return;
        }

        base.SendDamage(target);
        hasHitPlayer = true;

        Debug.Log("[Ghost] Ghost damaged player!");
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
