# Audio System Documentation

## Overview
The Audio System provides a clean, modular architecture for managing both 2D (global) and 3D (spatial) audio in your game. It features proper separation of concerns, dependency injection, and easy-to-use APIs.

---

## Architecture

### Component Responsibilities

```
AudioManager
├── AudioVolumeController - Volume calculations and management
├── AudioDataRegistry - Loading and caching audio clip data
├── AudioSettingsManager - Saving/loading player preferences
├── GlobalAudioPlayback - 2D/UI/Global audio playback
└── SpatialAudioPlayback - 3D positioned audio playback
    └── SpatialAudioProvider - Component for 3D GameObjects
```

### 1. AudioManager (Orchestrator)
**Responsibility**: Provides a unified API and coordinates all audio subsystems.

**Use When**:
- You need to play audio from anywhere in your game
- You want to control volume settings
- You need to access audio functionality

**Key Methods**:
```csharp
// 2D Audio
audioManager.PlaySfx(SFXIdentifier.Jump);
audioManager.PlayMusic(MusicIdentifier.MenuTheme);

// 3D Audio
audioManager.PlaySfxAtPosition(SFXIdentifier.Explosion, position);
audioManager.PlaySfx(SFXIdentifier.Footstep, audioSource);

// Volume Control
audioManager.SetMasterVolume(0.8f);
audioManager.SetMusicVolume(0.7f);
audioManager.SetSFXVolume(0.9f);
```

### 2. AudioVolumeController
**Responsibility**: Manages volume levels and calculations.

**Features**:
- Master, Music, and SFX volume properties
- Automatic volume calculation with all multipliers
- Volume change events
- Clamping to 0-1 range

**Use When**:
- Building volume sliders in UI
- Need to calculate final audio volumes
- Want to react to volume changes

**Example**:
```csharp
// Get current volumes
float masterVol = audioManager.GetMasterVolume();
float musicVol = audioManager.GetMusicVolume();
float sfxVol = audioManager.GetSFXVolume();

// Set volumes (automatically saves after debounce)
audioManager.SetMasterVolume(0.8f);

// Calculate final volume
float finalVolume = audioManager.CalculateSFXVolume(0.5f);
// Returns: 0.5 * sfxVolume * masterVolume
```

### 3. AudioDataRegistry
**Responsibility**: Loads and manages audio clip data from Resources.

**Features**:
- Loads SFX from `Resources/Audio/SFX`
- Loads Music from `Resources/Audio/Music`
- Centralized registry management
- Automatic initialization

**Use When**:
- Audio Manager initializes (automatic)
- Need to reload audio data at runtime

**Note**: This is handled internally by AudioManager. You typically don't interact with it directly.

### 4. AudioSettingsManager
**Responsibility**: Persists audio settings to disk with debouncing.

**Features**:
- Async save/load operations
- Debounced saving (default: 3 seconds)
- Prevents excessive file writes
- Integrates with SaveLoadManager

**Use When**:
- Automatically saves when volumes change
- Can manually trigger save if needed

**Example**:
```csharp
// Automatic (on volume change, debounced)
audioManager.SetMasterVolume(0.8f); // Will save after 3 seconds

// Manual immediate save
await audioManager.SaveSettingsAsync();
```

### 5. GlobalAudioPlayback (2D/Global Audio)
**Responsibility**: Handles all non-spatial audio playback.

**Use For**:
- UI sounds (button clicks, menu navigation)
- Background music
- Ambient sounds
- Sounds that should be heard equally everywhere

**Features**:
- SFX playback (one-shot, pitched, random)
- Music playback (looping, one-shot, intro+loop)
- Pitch chains (increasing pitch on repeated plays)
- Volume synchronized with AudioManager

**Examples**:
```csharp
// Basic SFX
audioManager.PlaySfx(SFXIdentifier.ButtonClick);

// SFX with custom pitch
audioManager.PlaySfx(SFXIdentifier.Jump, pitch: 1.2f);

// Increasing pitch (e.g., collecting coins rapidly)
audioManager.PlayIncreasingPitchSFX(SFXIdentifier.Coin);

// Random SFX
audioManager.PlayRandomSfx(
    SFXIdentifier.Footstep1,
    SFXIdentifier.Footstep2,
    SFXIdentifier.Footstep3
);

// Background music
audioManager.PlayMusic(MusicIdentifier.BattleTheme);

// Music with intro then loop
audioManager.PlayMusicWithIntro(
    MusicIdentifier.BattleIntro,
    MusicIdentifier.BattleLoop
);

// One-shot music effect
audioManager.PlayMusicOneShot(MusicIdentifier.Victory);

// Stop audio
audioManager.StopAllSfx();
audioManager.StopMusic();
```

### 6. SpatialAudioPlayback (3D Spatial Audio)
**Responsibility**: Handles all 3D positioned audio playback.

**Use For**:
- Footsteps, impacts, explosions
- Environmental sounds
- Enemy sounds
- Any sound that should attenuate with distance

**Features**:
- Play at specific 3D positions
- Play on AudioSource components
- Distance-based attenuation
- Volume synchronized with AudioManager

**Examples**:
```csharp
// Play at a specific position (creates temporary AudioSource)
audioManager.PlaySfxAtPosition(
    SFXIdentifier.Explosion,
    explosionPosition
);

// Play on an existing AudioSource (more efficient for frequent sounds)
audioManager.PlaySfx(SFXIdentifier.Footstep, audioSource);

// With custom pitch
audioManager.PlaySfx(SFXIdentifier.Impact, audioSource, pitch: 0.9f);
```

### 7. SpatialAudioProvider (Component)
**Responsibility**: Provides convenient spatial audio for GameObjects.

**Use For**:
- Attaching to characters, enemies, objects
- Objects that frequently emit sounds
- Pooled objects that need audio

**Features**:
- Auto-configures AudioSource for 3D audio
- Synchronizes volume with AudioManager
- Inspector-configurable spatial settings
- Convenience methods for playing sounds

**Setup**:
1. Add `SpatialAudioProvider` component to GameObject
2. Configure in Inspector:
   - **Spatial Blend**: 0 = 2D, 1 = 3D (default: 1)
   - **Min Distance**: Start of attenuation (default: 1)
   - **Max Distance**: End of attenuation (default: 50)
3. Use in code

**Example**:
```csharp
public class Enemy : MonoBehaviour
{
    private SpatialAudioProvider _audioProvider;

    private void Awake()
    {
        _audioProvider = GetComponent<SpatialAudioProvider>();
    }

    private void Attack()
    {
        // Simple - plays at this GameObject's position
        _audioProvider.PlaySfx(SFXIdentifier.EnemyAttack);
    }

    private void TakeDamage()
    {
        // With custom pitch
        _audioProvider.PlaySfx(SFXIdentifier.EnemyHurt, pitch: 1.1f);
    }
}
```

---

## Common Usage Patterns

### Pattern 1: UI Sounds (2D)
```csharp
public class UIButton : MonoBehaviour
{
    private AudioManager _audioManager;

    private void Start()
    {
        _audioManager = ServiceLocator.Get<AudioManager>();
    }

    public void OnClick()
    {
        _audioManager.PlaySfx(SFXIdentifier.ButtonClick);
    }

    public void OnHover()
    {
        _audioManager.PlaySfx(SFXIdentifier.ButtonHover);
    }
}
```

### Pattern 2: Player Character (Mixed 2D/3D)
```csharp
public class Player : MonoBehaviour
{
    private AudioManager _audioManager;
    private SpatialAudioProvider _audioProvider;

    private void Start()
    {
        _audioManager = ServiceLocator.Get<AudioManager>();
        _audioProvider = GetComponent<SpatialAudioProvider>();
    }

    private void Jump()
    {
        // Use 2D for important player feedback
        _audioManager.PlaySfx(SFXIdentifier.Jump);
    }

    private void Footstep()
    {
        // Use 3D for footsteps (spatial immersion)
        _audioProvider.PlaySfx(SFXIdentifier.Footstep);
    }
}
```

### Pattern 3: Projectile with Impact Sound
```csharp
public class Grenade : MonoBehaviour
{
    private AudioManager _audioManager;

    private void Start()
    {
        _audioManager = ServiceLocator.Get<AudioManager>();
    }

    private void Explode()
    {
        // Play explosion at this position
        _audioManager.PlaySfxAtPosition(
            SFXIdentifier.Explosion,
            transform.position
        );

        Destroy(gameObject);
    }
}
```

### Pattern 4: Volume Settings UI
```csharp
public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private AudioManager _audioManager;

    private void Start()
    {
        _audioManager = ServiceLocator.Get<AudioManager>();

        // Initialize sliders with current values
        masterSlider.value = _audioManager.GetMasterVolume();
        musicSlider.value = _audioManager.GetMusicVolume();
        sfxSlider.value = _audioManager.GetSFXVolume();

        // Connect callbacks
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnMasterVolumeChanged(float value)
    {
        _audioManager.SetMasterVolume(value);
        // Play preview sound
        _audioManager.PlaySfx(SFXIdentifier.UIClick);
    }

    private void OnMusicVolumeChanged(float value)
    {
        _audioManager.SetMusicVolume(value);
        // Music volume updates automatically
    }

    private void OnSFXVolumeChanged(float value)
    {
        _audioManager.SetSFXVolume(value);
        // Play preview sound
        _audioManager.PlaySfx(SFXIdentifier.UIClick);
    }
}
```

### Pattern 5: Pooled Objects with Spatial Audio
```csharp
public class PooledEnemy : MonoBehaviour, IPoolable
{
    private SpatialAudioProvider _audioProvider;

    private void Awake()
    {
        _audioProvider = GetComponent<SpatialAudioProvider>();
    }

    public void OnSpawn()
    {
        // Audio is automatically positioned
        _audioProvider.PlaySfx(SFXIdentifier.EnemySpawn);
    }

    public void OnDespawn()
    {
        // Clean audio state if needed
    }

    private void Attack()
    {
        _audioProvider.PlaySfx(SFXIdentifier.EnemyAttack);
    }
}
```

---

## Best Practices

### When to Use 2D vs 3D Audio

**Use 2D (Global) Audio For**:
- UI sounds (buttons, menus, notifications)
- Background music
- Important player feedback (level up, achievement)
- Voiceovers and narration
- Sounds that should always be heard clearly

**Use 3D (Spatial) Audio For**:
- Footsteps
- Environmental sounds (wind, water, ambient)
- Enemy sounds
- Impacts and collisions
- Interactive objects
- Anything that exists at a specific location in the world

### Performance Considerations

**2D Audio**:
- Very cheap (uses shared AudioSource components)
- No distance calculations
- Ideal for frequent sounds

**3D Audio (Position)**:
- `PlaySfxAtPosition()` creates temporary GameObjects
- Use sparingly for one-off sounds
- Good for explosions, impacts, etc.

**3D Audio (Provider)**:
- Uses persistent AudioSource
- More efficient for frequently-played sounds
- Recommended for characters and pooled objects

### Volume Management

```csharp
// Volumes are automatically saved after 3 seconds of no changes
audioManager.SetMasterVolume(0.8f);
audioManager.SetMusicVolume(0.7f);
// Both saved together after debounce

// Force immediate save (e.g., on game quit)
private async void OnApplicationQuit()
{
    await audioManager.SaveSettingsAsync();
}
```

### Spatial Audio Configuration

Configure in Inspector on `SpatialAudioProvider`:
- **Spatial Blend = 1.0**: Fully 3D (recommended)
- **Min Distance**: Sound starts attenuating (e.g., 1-5 meters)
- **Max Distance**: Sound completely inaudible (e.g., 20-100 meters)

**Example Settings**:
- **Footsteps**: Min: 1, Max: 20
- **Gunshots**: Min: 5, Max: 100
- **Ambient**: Min: 10, Max: 50

---

## API Quick Reference

### AudioManager Methods

#### SFX Methods
```csharp
void PlaySfx(SFXIdentifier sfxId)
void PlaySfx(SFXIdentifier sfxId, float pitch)
void PlaySfx(SFXIdentifier sfxId, AudioSource audioSource)
void PlaySfx(SFXIdentifier sfxId, AudioSource audioSource, float pitch)
void PlaySfxAtPosition(SFXIdentifier sfxId, Vector3 position)
void PlayIncreasingPitchSFX(SFXIdentifier sfxId)
void PlayRandomSfx(params SFXIdentifier[] choices)
void StopAllSfx()
```

#### Music Methods
```csharp
void PlayMusic(MusicIdentifier musicId)
void PlayMusicOneShot(MusicIdentifier musicId, float pitch = 1f)
void PlayMusicWithIntro(MusicIdentifier introId, MusicIdentifier loopId)
void StopMusic()
void StopOneShotMusic()
AudioClip GetCurrentMusic()
```

#### Volume Methods
```csharp
float GetMasterVolume()
float GetMusicVolume()
float GetSFXVolume()
void SetMasterVolume(float volume)
void SetMusicVolume(float volume)
void SetSFXVolume(float volume)
float CalculateSFXVolume(float clipVolume)
float CalculateMusicVolume(float clipVolume)
```

#### Advanced Methods
```csharp
Task SaveSettingsAsync()
void ReloadAudioData()
```

### SpatialAudioProvider Methods
```csharp
void PlaySfx(SFXIdentifier sfxId)
void PlaySfx(SFXIdentifier sfxId, float pitch)
AudioSource AudioSource { get; }
```

---

## Troubleshooting

### Audio Not Playing
**Check**:
1. Is audio clip assigned in ScriptableObject?
2. Are volumes set to 0?
3. Is AudioManager initialized? (check ServiceLocator)
4. Check console for warnings

### Spatial Audio Not Attenuating
**Check**:
1. `spatialBlend` set to 1.0 (fully 3D)
2. `minDistance` and `maxDistance` configured appropriately
3. Camera has AudioListener component
4. Object is within max distance

### Volume Not Saving
**Check**:
1. SaveLoadManager is initialized
2. Wait for debounce period (3 seconds)
3. Check console for save errors
4. Verify write permissions

### Performance Issues
**Solutions**:
1. Use `SpatialAudioProvider` instead of `PlaySfxAtPosition` for frequent sounds
2. Limit number of simultaneous spatial sounds
3. Increase min distance to cull distant sounds earlier
4. Use audio pooling for very frequent sounds

---

## Events

### OnVolumeChanged / OnVolumeChangeEvent
Subscribe to react to volume changes:

```csharp
private void Start()
{
    var audioManager = ServiceLocator.Get<AudioManager>();
    audioManager.OnVolumeChanged += HandleVolumeChanged;
}

private void HandleVolumeChanged()
{
    // React to volume change
    UpdateVolumeUI();
}
```

---

## Testing

### Dependency Injection for Tests
```csharp
[Test]
public void AudioManager_CalculatesVolumeCorrectly()
{
    // Create mock dependencies
    var volumeController = new AudioVolumeController();
    var dataRegistry = new AudioDataRegistry();
    var settingsManager = new AudioSettingsManager(volumeController, 3f);

    // Inject into AudioManager
    audioManager.InjectDependencies(volumeController, dataRegistry, settingsManager);

    // Test behavior
    volumeController.MasterVolume = 0.5f;
    volumeController.SFXVolume = 0.8f;
    float result = audioManager.CalculateSFXVolume(1f);

    Assert.AreEqual(0.4f, result, 0.01f);
}
```

---

## Summary

The Audio System provides:
- ✅ Clean separation of concerns
- ✅ Both 2D and 3D audio support
- ✅ Easy-to-use API
- ✅ Automatic volume management
- ✅ Persistent settings
- ✅ Dependency injection for testing
- ✅ Professional-grade architecture

For questions or issues, refer to the component-specific documentation or console warnings.
