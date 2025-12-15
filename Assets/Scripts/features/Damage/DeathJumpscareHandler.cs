using UnityEngine;

/// <summary>
/// Handles jumpscare effects when player dies (sanity reaches 0).
/// Plays a crying sound as a jumpscare effect.
/// </summary>
public class DeathJumpscareHandler : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    [Tooltip("Delay before playing the jumpscare sound (in seconds)")]
    [SerializeField] private float jumpscareDelay = 0f;

    [Tooltip("Use random cry sound from Noni's cry pool")]
    [SerializeField] private bool useRandomCrySound = true;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        // Subscribe to player death event
        PlayerAttributes.onPlayerDead += OnPlayerDeath;
    }

    private void OnDisable()
    {
        PlayerAttributes.onPlayerDead -= OnPlayerDeath;
    }

    /// <summary>
    /// Called when player dies (sanity reaches 0)
    /// </summary>
    private void OnPlayerDeath()
    {
        if (hasTriggered) return; // Only trigger once
        hasTriggered = true;

        if (jumpscareDelay > 0f)
        {
            Invoke(nameof(PlayJumpscareCry), jumpscareDelay);
        }
        else
        {
            PlayJumpscareCry();
        }

        Debug.Log("[DeathJumpscare] Player died! Playing jumpscare cry.");
    }

    /// <summary>
    /// Play the jumpscare crying sound
    /// </summary>
    private void PlayJumpscareCry()
    {
        var audioManager = ServiceLocator.Get<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("[DeathJumpscare] AudioManager not found!");
            return;
        }

        if (useRandomCrySound)
        {
            // Play random cry from Noni's cry pool
            audioManager.PlayRandomSfx(
                SFXIdentifier.Cry_Alone_Noni,
                SFXIdentifier.Cry_Alone_Noni2,
                SFXIdentifier.Cry_Alone_Noni3,
                SFXIdentifier.Cry_Alone_Noni4
            );
        }
        else
        {
            // Play dedicated jumpscare cry sound
            audioManager.PlaySfx(SFXIdentifier.Death_Jumpscare_Cry);
        }
    }

    /// <summary>
    /// Reset the handler (for respawn scenarios)
    /// </summary>
    public void Reset()
    {
        hasTriggered = false;
    }
}
