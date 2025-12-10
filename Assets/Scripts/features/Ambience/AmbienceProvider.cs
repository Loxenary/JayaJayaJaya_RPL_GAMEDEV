using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Ambience
{
    /// <summary>
    /// Pure playback manager for music ambience with dual-layer system (BASE + TENSION).
    /// Handles cross-fading, layer management, and sequence progression.
    /// Does NOT subscribe to game events - use AmbienceListener or call methods directly from external scripts.
    /// </summary>
    public class AmbienceProvider : MonoBehaviour
    {
        #region Configuration

        [Header("Configuration")]
        [SerializeField] private AmbienceConfiguration configuration;

        [Header("Audio Settings")]
        [SerializeField] private float crossFadeDuration = 2f;
        [Tooltip("Volume multiplier for tension layer (0.4 = 40% of base volume)")]
        [SerializeField][Range(0f, 1f)] private float tensionLayerVolumeMultiplier = 0.4f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Audio Sources

        private AudioSource baseLayer;
        private AudioSource tensionLayer;

        #endregion

        #region Services

        private AudioManager audioManager;

        #endregion

        #region State Tracking

        private MusicIdentifier? currentBaseTrack;
        private MusicIdentifier? currentTensionTrack;

        private int currentSequenceIndex = 0;
        private List<MusicIdentifier> activeSequence;

        private Dictionary<ConditionType, bool> activeConditions;
        private ConditionType? currentPriorityCondition;

        #endregion

        #region Tweening

        private Tweener baseVolumeTween;
        private Tweener tensionVolumeTween;

        #endregion

        #region Coroutines

        private Coroutine trackEndMonitor;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeAudioSources();
            InitializeServices();
            InitializeState();
        }

        private void OnEnable()
        {
            SubscribeToVolumeChanges();
            StartAmbienceSequence();
        }

        private void OnDisable()
        {
            UnsubscribeFromVolumeChanges();
            CleanupTweens();
            StopTrackMonitoring();
        }

        private void OnDestroy()
        {
            CleanupTweens();
        }

        #endregion

        #region Initialization

        private void InitializeAudioSources()
        {
            // Create BASE layer for primary music
            baseLayer = gameObject.AddComponent<AudioSource>();
            baseLayer.playOnAwake = false;
            baseLayer.loop = false;
            baseLayer.spatialBlend = 0f; // 2D audio
            baseLayer.priority = 64; // Medium priority

            // Create TENSION layer for secondary/backgrounded music
            tensionLayer = gameObject.AddComponent<AudioSource>();
            tensionLayer.playOnAwake = false;
            tensionLayer.loop = false;
            tensionLayer.spatialBlend = 0f;
            tensionLayer.priority = 65; // Slightly lower priority

            Log("AudioSources initialized (BASE + TENSION layers)");
        }

        private void InitializeServices()
        {
            audioManager = ServiceLocator.Get<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("[AmbienceProvider] AudioManager not found in ServiceLocator!");
                enabled = false;
                return;
            }

            Log("AudioManager connected via ServiceLocator");
        }

        private void InitializeState()
        {
            activeConditions = new Dictionary<ConditionType, bool>();
            activeSequence = new List<MusicIdentifier>();

            // Initialize all conditions to false
            foreach (ConditionType condition in Enum.GetValues(typeof(ConditionType)))
            {
                activeConditions[condition] = false;
            }

            Log("State initialized");
        }

        #endregion

        #region Volume Management Subscription

        private void SubscribeToVolumeChanges()
        {
            if (audioManager != null)
            {
                audioManager.OnVolumeChanged += UpdateVolumes;
            }
        }

        private void UnsubscribeFromVolumeChanges()
        {
            if (audioManager != null)
            {
                audioManager.OnVolumeChanged -= UpdateVolumes;
            }
        }

        #endregion

        #region Public API - Call from external scripts

        /// <summary>
        /// Activate a condition. Triggers priority evaluation and music changes.
        /// Call this from EnemyAngrySystem, AmbienceConditional, or any other script.
        /// </summary>
        public void ActivateCondition(ConditionType condition)
        {
            if (condition == ConditionType.None) return;

            Log($"Activating condition: {condition}");
            activeConditions[condition] = true;
            EvaluatePriorityCondition();
        }

        /// <summary>
        /// Deactivate a condition. Triggers priority evaluation.
        /// </summary>
        public void DeactivateCondition(ConditionType condition)
        {
            if (condition == ConditionType.None) return;

            Log($"Deactivating condition: {condition}");
            activeConditions[condition] = false;
            EvaluatePriorityCondition();
        }

        /// <summary>
        /// Clear all active conditions and return to ambient sequence.
        /// </summary>
        public void ClearAllConditions()
        {
            Log("Clearing all conditions");

            foreach (var key in new List<ConditionType>(activeConditions.Keys))
            {
                activeConditions[key] = false;
            }

            currentPriorityCondition = null;
            CrossFadeBackToAmbience();
        }

        /// <summary>
        /// Play a specific music track directly, bypassing condition system.
        /// </summary>
        public void PlayMusicDirect(MusicIdentifier music, bool asCondition = false)
        {
            Log($"Playing music directly: {music} (asCondition: {asCondition})");

            if (asCondition)
            {
                CrossFadeToConditionMusic(music);
            }
            else
            {
                // Play as new base, stopping sequence
                var musicData = GetMusicData(music);
                if (musicData == null) return;

                CleanupTweens();
                StopTrackMonitoring();

                baseLayer.clip = musicData.Clip;
                baseLayer.volume = CalculateBaseVolume(musicData);
                baseLayer.loop = false;
                baseLayer.Play();

                currentBaseTrack = music;
            }
        }

        /// <summary>
        /// Stop all music and reset the provider.
        /// </summary>
        public void StopAll()
        {
            Log("Stopping all music");

            CleanupTweens();
            StopTrackMonitoring();

            baseLayer.Stop();
            tensionLayer.Stop();

            currentBaseTrack = null;
            currentTensionTrack = null;
        }

        /// <summary>
        /// Restart the ambient sequence from the beginning.
        /// </summary>
        public void RestartSequence()
        {
            Log("Restarting ambient sequence");

            currentSequenceIndex = 0;
            ClearAllConditions();
            StartAmbienceSequence();
        }

        #endregion

        #region Priority Condition Resolution

        private void EvaluatePriorityCondition()
        {
            if (configuration == null)
            {
                Debug.LogWarning("[AmbienceProvider] No configuration set!");
                return;
            }

            var sortedConditions = configuration.GetSortedByPriority();
            ConditionType? newPriorityCondition = null;

            // Find the highest priority active condition
            foreach (var record in sortedConditions)
            {
                if (activeConditions.TryGetValue(record.conditionType, out bool isActive) && isActive)
                {
                    newPriorityCondition = record.conditionType;
                    Log($"Priority condition: {newPriorityCondition} (Priority: {record.priority})");
                    break;
                }
            }

            // If priority changed, update music
            if (newPriorityCondition != currentPriorityCondition)
            {
                OnPriorityConditionChanged(currentPriorityCondition, newPriorityCondition);
                currentPriorityCondition = newPriorityCondition;
            }
        }

        private void OnPriorityConditionChanged(ConditionType? oldCondition, ConditionType? newCondition)
        {
            Log($"Priority condition changed: {oldCondition} → {newCondition}");

            if (!newCondition.HasValue || newCondition.Value == ConditionType.None)
            {
                // Return to ambience sequence
                CrossFadeBackToAmbience();
            }
            else
            {
                var record = configuration.GetConditionRecord(newCondition.Value);
                if (record.HasValue)
                {
                    CrossFadeToConditionMusic(record.Value.musicTrack);
                }
            }
        }

        #endregion

        #region Sequence Management

        private void StartAmbienceSequence()
        {
            if (configuration == null)
            {
                Debug.LogWarning("[AmbienceProvider] No configuration assigned!");
                return;
            }

            if (configuration.AmbienceSequence == null || configuration.AmbienceSequence.Length == 0)
            {
                Debug.LogWarning("[AmbienceProvider] Ambience sequence is empty!");
                return;
            }

            activeSequence = new List<MusicIdentifier>(configuration.AmbienceSequence);
            currentSequenceIndex = 0;

            Log($"Starting ambience sequence with {activeSequence.Count} tracks");
            PlayNextInSequence();
        }

        private void PlayNextInSequence()
        {
            if (activeSequence == null || activeSequence.Count == 0)
            {
                Debug.LogWarning("[AmbienceProvider] Cannot play next in sequence: sequence is empty");
                return;
            }

            MusicIdentifier nextTrack = activeSequence[currentSequenceIndex];
            Log($"Playing sequence track [{currentSequenceIndex}]: {nextTrack}");

            PlayTrackOnLayer(baseLayer, nextTrack, true, () =>
            {
                // Move to next track in sequence
                currentSequenceIndex = (currentSequenceIndex + 1) % activeSequence.Count;
                CrossFadeToNextSequenceTrack();
            });

            currentBaseTrack = nextTrack;
        }

        private void CrossFadeToNextSequenceTrack()
        {
            if (activeSequence == null || activeSequence.Count == 0) return;

            // Don't auto-progress if a condition is active
            if (currentPriorityCondition.HasValue && currentPriorityCondition.Value != ConditionType.None)
            {
                Log("Skipping sequence progression - condition music is active");
                return;
            }

            MusicIdentifier nextTrack = activeSequence[currentSequenceIndex];
            Log($"Cross-fading to next sequence track: {nextTrack}");

            var nextData = GetMusicData(nextTrack);
            if (nextData == null)
            {
                Debug.LogError($"[AmbienceProvider] Music data not found for {nextTrack}");
                return;
            }

            // Start next track on tension layer at 0 volume
            tensionLayer.clip = nextData.Clip;
            tensionLayer.volume = 0f;
            tensionLayer.loop = false;
            tensionLayer.Play();

            // Cross-fade: base out, tension in
            CleanupTweens();

            baseVolumeTween = baseLayer.DOFade(0f, crossFadeDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => baseLayer.Stop());

            float targetVolume = CalculateBaseVolume(nextData);
            tensionVolumeTween = tensionLayer.DOFade(targetVolume, crossFadeDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    // Swap layers: tension becomes base
                    SwapAudioSourceContent(tensionLayer, baseLayer);
                    tensionLayer.Stop();
                    currentBaseTrack = nextTrack;

                    // Monitor for next transition
                    StopTrackMonitoring();
                    trackEndMonitor = StartCoroutine(MonitorTrackEnd(baseLayer, nextTrack, () =>
                    {
                        currentSequenceIndex = (currentSequenceIndex + 1) % activeSequence.Count;
                        CrossFadeToNextSequenceTrack();
                    }));
                });
        }

        #endregion

        #region Condition Music Transitions

        private void CrossFadeToConditionMusic(MusicIdentifier conditionMusic)
        {
            Log($"Transitioning to condition music: {conditionMusic}");

            CleanupTweens();

            // Current base becomes tension (lowered volume)
            if (currentBaseTrack.HasValue && baseLayer.isPlaying)
            {
                Log($"Moving current base ({currentBaseTrack}) to tension layer");

                // Copy base to tension
                SwapAudioSourceContent(baseLayer, tensionLayer);
                currentTensionTrack = currentBaseTrack;

                // Fade tension to lower volume
                var tensionData = GetMusicData(currentTensionTrack.Value);
                if (tensionData != null)
                {
                    float targetTensionVolume = CalculateTensionVolume(tensionData);
                    tensionVolumeTween = tensionLayer.DOFade(targetTensionVolume, crossFadeDuration)
                        .SetEase(Ease.InOutSine);
                }
            }

            // New condition music becomes base
            var baseData = GetMusicData(conditionMusic);
            if (baseData == null)
            {
                Debug.LogError($"[AmbienceProvider] Music data not found for {conditionMusic}");
                return;
            }

            baseLayer.clip = baseData.Clip;
            baseLayer.volume = 0f;
            baseLayer.loop = false;
            baseLayer.Play();

            float targetBaseVolume = CalculateBaseVolume(baseData);
            baseVolumeTween = baseLayer.DOFade(targetBaseVolume, crossFadeDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    currentBaseTrack = conditionMusic;
                    Log($"Condition music now playing: {conditionMusic}");

                    // Monitor track end to return to ambience
                    StopTrackMonitoring();
                    trackEndMonitor = StartCoroutine(MonitorTrackEnd(baseLayer, conditionMusic, () =>
                    {
                        Log($"Condition music {conditionMusic} finished");
                        // Check if condition still active
                        EvaluatePriorityCondition();
                    }));
                });
        }

        private void CrossFadeBackToAmbience()
        {
            Log("Returning to ambience sequence");

            // If tension layer has the ambience, bring it back to base
            if (currentTensionTrack.HasValue && tensionLayer.isPlaying)
            {
                Log($"Restoring tension layer ({currentTensionTrack}) to base");

                CleanupTweens();

                // Fade out current base (condition music)
                baseVolumeTween = baseLayer.DOFade(0f, crossFadeDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => baseLayer.Stop());

                // Fade in tension as new base
                var ambienceData = GetMusicData(currentTensionTrack.Value);
                if (ambienceData != null)
                {
                    float targetVolume = CalculateBaseVolume(ambienceData);
                    tensionVolumeTween = tensionLayer.DOFade(targetVolume, crossFadeDuration)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            // Swap: tension becomes base
                            SwapAudioSourceContent(tensionLayer, baseLayer);
                            tensionLayer.Stop();
                            currentBaseTrack = currentTensionTrack;
                            currentTensionTrack = null;

                            // Resume sequence monitoring
                            StopTrackMonitoring();
                            trackEndMonitor = StartCoroutine(MonitorTrackEnd(baseLayer, currentBaseTrack.Value, () =>
                            {
                                currentSequenceIndex = (currentSequenceIndex + 1) % activeSequence.Count;
                                CrossFadeToNextSequenceTrack();
                            }));
                        });
                }
            }
            else
            {
                // No ambience to restore, start fresh sequence
                Log("No ambience to restore, starting fresh sequence");
                PlayNextInSequence();
            }
        }

        #endregion

        #region Audio Playback Helpers

        private void PlayTrackOnLayer(AudioSource source, MusicIdentifier track, bool monitorEnd, Action onComplete = null)
        {
            var musicData = GetMusicData(track);
            if (musicData == null)
            {
                Debug.LogError($"[AmbienceProvider] Cannot play track {track}: music data not found");
                return;
            }

            source.clip = musicData.Clip;
            source.volume = CalculateBaseVolume(musicData);
            source.loop = false;
            source.Play();

            if (monitorEnd && onComplete != null)
            {
                StopTrackMonitoring();
                trackEndMonitor = StartCoroutine(MonitorTrackEnd(source, track, onComplete));
            }
        }

        private IEnumerator MonitorTrackEnd(AudioSource source, MusicIdentifier track, Action onComplete)
        {
            var clipData = GetMusicData(track);
            if (clipData == null || clipData.Clip == null)
            {
                Log($"Cannot monitor track {track}: clip data missing");
                yield break;
            }

            // Wait for clip to start playing
            yield return new WaitUntil(() => source.isPlaying);

            // Track expected end time
            float clipLength = clipData.Clip.length;
            float startTime = Time.time;
            float endTime = startTime + clipLength;

            Log($"Monitoring track {track} (length: {clipLength:F1}s)");

            // Wait until near end
            yield return new WaitForSeconds(clipLength - 0.1f);

            // Final check with timeout
            while (source.isPlaying && Time.time < endTime + 0.5f)
            {
                yield return null;
            }

            Log($"Track {track} finished playing");
            onComplete?.Invoke();
        }

        private void SwapAudioSourceContent(AudioSource source, AudioSource target)
        {
            target.clip = source.clip;
            target.volume = source.volume;
            target.time = source.time;
            target.loop = source.loop;

            if (source.isPlaying)
            {
                target.Play();
            }
        }

        #endregion

        #region Volume Management

        private MusicClipData GetMusicData(MusicIdentifier id)
        {
            return MusicClipData.GetMusicClipDataById(id);
        }

        private float CalculateBaseVolume(MusicClipData clipData)
        {
            if (clipData == null || audioManager == null) return 0f;
            return audioManager.CalculateMusicVolume(clipData.Volume);
        }

        private float CalculateTensionVolume(MusicClipData clipData)
        {
            if (clipData == null || audioManager == null) return 0f;

            float baseVolume = audioManager.CalculateMusicVolume(clipData.Volume);
            return baseVolume * tensionLayerVolumeMultiplier;
        }

        private void UpdateVolumes()
        {
            Log("Updating volumes (settings changed)");

            if (currentBaseTrack.HasValue)
            {
                var baseData = GetMusicData(currentBaseTrack.Value);
                if (baseData != null)
                {
                    baseLayer.volume = CalculateBaseVolume(baseData);
                }
            }

            if (currentTensionTrack.HasValue)
            {
                var tensionData = GetMusicData(currentTensionTrack.Value);
                if (tensionData != null)
                {
                    tensionLayer.volume = CalculateTensionVolume(tensionData);
                }
            }
        }

        #endregion

        #region Cleanup

        private void CleanupTweens()
        {
            baseVolumeTween?.Kill();
            tensionVolumeTween?.Kill();
            baseVolumeTween = null;
            tensionVolumeTween = null;
        }

        private void StopTrackMonitoring()
        {
            if (trackEndMonitor != null)
            {
                StopCoroutine(trackEndMonitor);
                trackEndMonitor = null;
            }
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[AmbienceProvider] {message}");
            }
        }

        #endregion
    }
}
