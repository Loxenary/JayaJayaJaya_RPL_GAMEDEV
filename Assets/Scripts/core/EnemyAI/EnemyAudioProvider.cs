using System.Collections;
using UnityEngine;

public class EnemyAudioProvider : MonoBehaviour
{
    [SerializeField] private SpatialAudioProvider _audioSource;

    private SfxClipData _currentLoopingSfx = null;
    private SfxClipData[] _sequentialSfxArray = null;
    private int _currentSequentialIndex = 0;
    private Coroutine _sequentialCoroutine = null;

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
    /// Plays SFX from the provided array sequentially in a loop.
    /// Each sound plays in order, then loops back to the beginning.
    /// </summary>
    public void PlaySequentialSfxLooping(SfxClipData[] sfxArray)
    {
        if (sfxArray == null || sfxArray.Length == 0) return;

        // Stop any currently playing sounds
        StopAllSounds();

        // Store the array and start the sequential playback
        _sequentialSfxArray = sfxArray;
        _currentSequentialIndex = 0;
        _sequentialCoroutine = StartCoroutine(PlaySequentialCoroutine());
    }

    private IEnumerator PlaySequentialCoroutine()
    {
        while (_sequentialSfxArray != null && _sequentialSfxArray.Length > 0)
        {
            // Get the current SFX to play
            SfxClipData currentSfx = _sequentialSfxArray[_currentSequentialIndex];

            if (currentSfx != null && _audioSource != null && _audioSource.AudioSource != null)
            {
                // Play the current SFX
                _audioSource.PlaySfx(currentSfx);

                // Wait for the clip duration plus any delay
                float waitTime = currentSfx.Clip.length + currentSfx.Delay;
                yield return new WaitForSeconds(waitTime);
            }

            // Move to the next index, looping back to 0 when reaching the end
            _currentSequentialIndex = (_currentSequentialIndex + 1) % _sequentialSfxArray.Length;
        }
    }

    /// <summary>
    /// Stops the currently playing sequential SFX loop.
    /// </summary>
    public void StopSequentialSfx()
    {
        if (_sequentialCoroutine != null)
        {
            StopCoroutine(_sequentialCoroutine);
            _sequentialCoroutine = null;
        }

        _sequentialSfxArray = null;
        _currentSequentialIndex = 0;
    }

    /// <summary>
    /// Stops all currently playing sounds (sequential, looping, and audio source).
    /// </summary>
    private void StopAllSounds()
    {
        // Stop sequential playback
        if (_sequentialCoroutine != null)
        {
            StopCoroutine(_sequentialCoroutine);
            _sequentialCoroutine = null;
        }
        _sequentialSfxArray = null;
        _currentSequentialIndex = 0;

        // Stop looping playback
        _currentLoopingSfx = null;

        // Stop the audio source
        if (_audioSource != null && _audioSource.AudioSource != null)
        {
            _audioSource.AudioSource.loop = false;
            _audioSource.AudioSource.Stop();
        }
    }

    /// <summary>
    /// Plays a random SFX from the provided array in loop mode.
    /// Continues looping until StopLoopingSfx is called.
    /// </summary>
    public void PlayRandomSfxLooping(SfxClipData[] sfxArray)
    {
        if (sfxArray == null || sfxArray.Length == 0) return;

        // Stop any currently playing sounds
        StopAllSounds();

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
        StopAllSounds();
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
    /// Checks if sequential SFX playback is currently active.
    /// </summary>
    public bool IsSequentialPlaying()
    {
        return _sequentialCoroutine != null && _sequentialSfxArray != null;
    }

    /// <summary>
    /// Legacy method for playing a single SFX.
    /// </summary>
    public void PlaySfx(SfxClipData clipData)
    {
        _audioSource.PlaySfx(clipData);
    }
}