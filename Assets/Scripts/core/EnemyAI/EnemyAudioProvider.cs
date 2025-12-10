using UnityEngine;

public class EnemyAudioProvider : MonoBehaviour
{
    [SerializeField] private SpatialAudioProvider _audioSource;

    private SfxClipData _currentLoopingSfx = null;

    /// <summary>
    /// Plays a random SFX from the provided array once.
    /// </summary>
    public void PlayRandomSfxOnce(SfxClipData[] sfxArray)
    {
        if (sfxArray == null || sfxArray.Length == 0) return;

        // Randomly pick one from the array
        SfxClipData randomSfx = sfxArray[Random.Range(0, sfxArray.Length)];

        if (randomSfx != null)
        {
            _audioSource.PlaySfx(randomSfx);
        }
    }

    /// <summary>
    /// Plays a random SFX from the provided array in loop mode.
    /// Continues looping until StopLoopingSfx is called.
    /// </summary>
    public void PlayRandomSfxLooping(SfxClipData[] sfxArray)
    {
        if (sfxArray == null || sfxArray.Length == 0) return;

        // Stop any currently looping sound first
        StopLoopingSfx();

        // Randomly pick one from the array
        SfxClipData randomSfx = sfxArray[Random.Range(0, sfxArray.Length)];

        if (randomSfx != null && _audioSource != null && _audioSource.AudioSource != null)
        {
            _currentLoopingSfx = randomSfx;
            _audioSource.AudioSource.loop = true;
            _audioSource.PlaySfx(randomSfx);
        }
    }

    /// <summary>
    /// Stops the currently looping SFX.
    /// </summary>
    public void StopLoopingSfx()
    {
        if (_audioSource != null && _audioSource.AudioSource != null)
        {
            _audioSource.AudioSource.loop = false;
            _audioSource.AudioSource.Stop();
            _currentLoopingSfx = null;
        }
    }

    /// <summary>
    /// Checks if a looping SFX is currently playing.
    /// </summary>
    public bool IsLoopingPlaying()
    {
        return _currentLoopingSfx != null &&
               _audioSource != null &&
               _audioSource.AudioSource != null &&
               _audioSource.AudioSource.isPlaying;
    }

    /// <summary>
    /// Legacy method for playing a single SFX.
    /// </summary>
    public void PlaySfx(SfxClipData clipData)
    {
        _audioSource.PlaySfx(clipData);
    }
}