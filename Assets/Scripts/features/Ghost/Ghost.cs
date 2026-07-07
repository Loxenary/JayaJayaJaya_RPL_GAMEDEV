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
    private bool currentlyTouchingPlayer = false;
    private bool hasHitPlayer = false;

    /// <summary>
    /// Override SendDamage to check if ghost is active.
    /// Cooldown waktu SUDAH ditegakkan parent (Damage.damageCooldown, serialized)
    /// — cooldown kedua yang hardcoded di sini dihapus agar hanya ada satu
    /// sumber kebenaran yang bisa di-tune designer (B-20).
    /// </summary>
    protected override void SendDamage(IDamageable target)
    {
        if (!isActive)
        {
            return;
        }

        // If damageOncePerContact is enabled, only damage once until player leaves
        if (damageOncePerContact && currentlyTouchingPlayer)
        {
            return;
        }

        currentlyTouchingPlayer = true;

        base.SendDamage(target);
        hasHitPlayer = true;
    }
    private void OnTriggerExit(Collider other)
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
