using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ambience
{
    /// <summary>
    /// Event struct for triggering music events via EventBus
    /// </summary>
    public readonly struct MusicEventRequest
    {
        public readonly MusicEventType EventType;
        public readonly string Data;

        public MusicEventRequest(MusicEventType eventType, string data = null)
        {
            EventType = eventType;
            Data = data;
        }

        public override string ToString()
            => Data != null ? $"MusicEvent({EventType}, data: {Data})" : $"MusicEvent({EventType})";
    }

    /// <summary>
    /// Event struct for ending music events via EventBus
    /// </summary>
    public readonly struct MusicEventEnd
    {
        public readonly MusicEventType EventType;

        public MusicEventEnd(MusicEventType eventType)
        {
            EventType = eventType;
        }

        public override string ToString()
            => $"MusicEventEnd({EventType})";
    }

    /// <summary>
    /// Event-based music manager with state stacking for interruption/resume
    /// Handles: Silence → MainTheme → Mansion Empty loop → Chase → Resume where left off
    /// </summary>
    public class GameMusicManager : MonoBehaviour
    {

        #region Configuration

        [Header("Configuration")]
        [SerializeField] private MusicEventData musicEventData;

        [Header("Audio Settings")]
        [SerializeField] private float defaultFadeDuration = 2f;
        [SerializeField] private TransitionCurve defaultTransitionCurve = TransitionCurve.ConstantPower;

        [Header("Silence Settings")]
        [Tooltip("Duration of silence between tracks (in seconds)")]
        [SerializeField] private float silenceDuration = 15f;

        [Header("Layer Settings")]
        [Tooltip("Volume multiplier for background layer")]
        [SerializeField][Range(0f, 1f)] private float backgroundLayerMultiplier = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Audio Sources

        private AudioSource primaryLayer;
        private AudioSource secondaryLayer;

        #endregion

        #region Services

        private AudioManager audioManager;
        private MusicTransitionHelper transitionHelper;

        #endregion

        #region State Tracking

        private MusicIdentifier? currentPrimaryTrack;
        private MusicIdentifier? currentSecondaryTrack;

        private MusicEventType? currentEventType;

        private int currentSequenceIndex = 0;
        private List<MusicIdentifier> currentSequence;
        private Coroutine silenceCoroutine;

        // State stack for interruptions
        private readonly Stack<MusicState> stateStack = new();
        private bool isPlayingEventMusic = false;

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
            SubscribeToEvents();
            SubscribeToVolumeChanges();
            StartDefaultMusic();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            UnsubscribeFromVolumeChanges();
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            transitionHelper?.StopAllTransitions();
        }

        #endregion

        #region Initialization

        private void InitializeAudioSources()
        {
            primaryLayer = gameObject.AddComponent<AudioSource>();
            primaryLayer.playOnAwake = false;
            primaryLayer.loop = false;
            primaryLayer.spatialBlend = 0f;
            primaryLayer.priority = 64;

            secondaryLayer = gameObject.AddComponent<AudioSource>();
            secondaryLayer.playOnAwake = false;
            secondaryLayer.loop = false;
            secondaryLayer.spatialBlend = 0f;
            secondaryLayer.priority = 65;

            Log("Audio layers initialized");
        }

        private void InitializeServices()
        {
            audioManager = ServiceLocator.Get<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("[GameMusicManager] AudioManager not found!");
                enabled = false;
                return;
            }

            transitionHelper = MusicTransitionHelper.Instance;
            Log("Services connected");
        }

        private void InitializeState()
        {
            currentSequence = new List<MusicIdentifier>();
            currentEventType = null;
            Log("State initialized");
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<MusicEventRequest>(HandleMusicEventRequest);
            EventBus.Subscribe<MusicEventEnd>(HandleMusicEventEnd);
            Log("Subscribed to music events via EventBus");
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<MusicEventRequest>(HandleMusicEventRequest);
            EventBus.Unsubscribe<MusicEventEnd>(HandleMusicEventEnd);
        }

        private void SubscribeToVolumeChanges()
        {
            audioManager = ServiceLocator.Get<AudioManager>();
            if (audioManager != null)
            {
                audioManager.OnVolumeChanged += UpdateVolumes;
            }
        }

        private void UnsubscribeFromVolumeChanges()
        {
            audioManager = ServiceLocator.Get<AudioManager>();
            if (audioManager != null)
            {
                audioManager.OnVolumeChanged -= UpdateVolumes;
            }
        }

        #endregion

        #region Event Handling

        private void HandleMusicEventRequest(MusicEventRequest request)
        {
            Log($"Music event received: {request.EventType} (data: {request.Data ?? "none"})");

            if (musicEventData == null)
            {
                Debug.LogWarning("[GameMusicManager] No MusicEventData assigned!");
                return;
            }

            if (!musicEventData.HasEventConfig(request.EventType))
            {
                Log($"No configuration for event: {request.EventType}");
                return;
            }

            // Simply play the requested music event
            PlayEventMusic(request.EventType);
        }

        private void HandleMusicEventEnd(MusicEventEnd end)
        {
            Log($"Ending event: {end.EventType}");

            // If this is the current event, resume
            if (currentEventType == end.EventType)
            {
                ResumeFromStack();
            }
        }

        #endregion

        #region Music Playback

        private void StartDefaultMusic()
        {
            if (musicEventData == null || musicEventData.DefaultMusicSequence.Count == 0)
            {
                Log("No default music sequence configured - starting with silence");
                StartSilence();
                return;
            }

            currentSequence = new List<MusicIdentifier>(musicEventData.DefaultMusicSequence);

            if (musicEventData.ShuffleDefaultSequence)
            {
                ShuffleSequence(currentSequence);
            }

            currentSequenceIndex = 0;
            Log($"Starting default music sequence with {currentSequence.Count} tracks");

            PlaySequenceTrack(currentSequenceIndex);
        }

        private void PlaySequenceTrack(int index)
        {
            if (currentSequence == null || currentSequence.Count == 0) return;

            MusicIdentifier track = currentSequence[index];
            Log($"Playing sequence track [{index}]: {track}");

            PlayMusicOnLayer(primaryLayer, track, true, () =>
            {
                OnSequenceTrackFinished();
            });

            currentPrimaryTrack = track;
        }

        private void OnSequenceTrackFinished()
        {
            // Don't auto-progress if an event is active
            if (isPlayingEventMusic)
            {
                Log("Skipping sequence progression - event music active");
                return;
            }

            // Start silence before next track
            Log($"Track finished, starting {silenceDuration}s silence");
            StartSilence();
        }

        private void StartSilence()
        {
            if (silenceCoroutine != null)
            {
                StopCoroutine(silenceCoroutine);
            }

            primaryLayer.Stop();
            currentPrimaryTrack = null;

            silenceCoroutine = StartCoroutine(SilenceCoroutine());
        }

        private IEnumerator SilenceCoroutine()
        {
            Log($"Silence started ({silenceDuration}s)");
            yield return new WaitForSeconds(silenceDuration);

            Log("Silence ended, playing next track");

            // Move to next track
            if (currentSequence != null && currentSequence.Count > 0)
            {
                currentSequenceIndex++;

                if (currentSequenceIndex >= currentSequence.Count)
                {
                    if (musicEventData.LoopDefaultSequence)
                    {
                        currentSequenceIndex = 0;

                        if (musicEventData.ShuffleDefaultSequence)
                        {
                            ShuffleSequence(currentSequence);
                        }
                    }
                    else
                    {
                        Log("Sequence finished (no loop)");
                        yield break;
                    }
                }

                PlaySequenceTrack(currentSequenceIndex);
            }
        }

        private void PlayEventMusic(MusicEventType eventType)
        {
            var config = musicEventData.GetEventConfig(eventType);
            if (!config.HasValue) return;

            var track = musicEventData.GetRandomMusicForEvent(eventType);
            if (!track.HasValue)
            {
                Log($"No music tracks for event: {eventType}");
                return;
            }

            Log($"Playing event music: {track.Value} (Event: {eventType})");

            // If this is an interrupt type, save current state
            if (config.Value.interruptCurrent)
            {
                SaveCurrentState();
            }

            currentEventType = eventType;

            var musicData = GetMusicData(track.Value);
            if (musicData == null) return;

            float fadeDuration = config.Value.customFadeInDuration > 0
                ? config.Value.customFadeInDuration
                : defaultFadeDuration;

            isPlayingEventMusic = true;

            // Stop silence if active
            if (silenceCoroutine != null)
            {
                StopCoroutine(silenceCoroutine);
                silenceCoroutine = null;
            }

            // Stop current music and play new one
            if (primaryLayer.isPlaying)
            {
                // Fade out current, fade in new
                transitionHelper.Crossfade(
                    primaryLayer,
                    secondaryLayer,
                    fadeDuration,
                    CalculateVolume(musicData),
                    defaultTransitionCurve,
                    () =>
                    {
                        transitionHelper.SwapAudioSourceContent(secondaryLayer, primaryLayer);
                        secondaryLayer.Stop();
                        primaryLayer.loop = config.Value.loopMusic;
                        currentPrimaryTrack = track.Value;

                        if (!config.Value.loopMusic)
                        {
                            StartCoroutine(MonitorTrackEnd(primaryLayer, track.Value, () =>
                            {
                                OnEventMusicFinished(eventType);
                            }));
                        }
                    });
            }
            else
            {
                // Just play new track
                primaryLayer.clip = musicData.Clip;
                primaryLayer.loop = config.Value.loopMusic;

                float targetVolume = CalculateVolume(musicData);
                transitionHelper.FadeIn(primaryLayer, targetVolume, fadeDuration, defaultTransitionCurve, () =>
                {
                    currentPrimaryTrack = track.Value;

                    if (!config.Value.loopMusic)
                    {
                        StartCoroutine(MonitorTrackEnd(primaryLayer, track.Value, () =>
                        {
                            OnEventMusicFinished(eventType);
                        }));
                    }
                });
            }
        }

        private void OnEventMusicFinished(MusicEventType eventType)
        {
            Log($"Event music finished: {eventType}");

            currentEventType = null;
            isPlayingEventMusic = false;

            // Resume from stack if this was an interrupt
            ResumeFromStack();
        }

        #endregion

        #region State Stack Management

        private void SaveCurrentState()
        {
            var state = MusicState.Capture(
                primaryLayer,
                currentPrimaryTrack,
                currentEventType,
                currentSequence,
                currentSequenceIndex
            );

            stateStack.Push(state);
            Log($"State saved to stack (depth: {stateStack.Count}): {state}");
        }

        private void ResumeFromStack()
        {
            if (stateStack.Count == 0)
            {
                Log("Stack empty, returning to default music");
                currentEventType = null;
                isPlayingEventMusic = false;
                StartDefaultMusic();
                return;
            }

            var savedState = stateStack.Pop();
            Log($"Resuming from stack (remaining: {stateStack.Count}): {savedState}");

            currentEventType = savedState.eventType;
            isPlayingEventMusic = false;

            if (savedState.isSequence)
            {
                // Resume sequence playback
                currentSequence = savedState.sequence;
                currentSequenceIndex = savedState.sequenceIndex;

                if (savedState.wasPlaying && savedState.currentTrack.HasValue)
                {
                    // Resume from where we left off
                    ResumeTrackFromTime(savedState.currentTrack.Value, savedState.trackTime);
                }
                else
                {
                    // Was in silence, continue silence
                    StartSilence();
                }
            }
            else
            {
                // Was playing event music
                if (savedState.currentTrack.HasValue)
                {
                    ResumeTrackFromTime(savedState.currentTrack.Value, savedState.trackTime);
                }
            }
        }

        private void ResumeTrackFromTime(MusicIdentifier track, float time)
        {
            Log($"Resuming track {track} from {time:F1}s");

            var musicData = GetMusicData(track);
            if (musicData == null) return;

            float fadeDuration = defaultFadeDuration * 0.5f; // Shorter fade for resume

            // Crossfade if something is playing
            if (primaryLayer.isPlaying)
            {
                secondaryLayer.clip = musicData.Clip;
                secondaryLayer.time = Mathf.Min(time, secondaryLayer.clip.length - 0.1f);
                secondaryLayer.volume = 0f;

                float targetVolume = CalculateVolume(musicData);

                transitionHelper.Crossfade(
                    primaryLayer,
                    secondaryLayer,
                    fadeDuration,
                    targetVolume,
                    defaultTransitionCurve,
                    () =>
                    {
                        transitionHelper.SwapAudioSourceContent(secondaryLayer, primaryLayer);
                        secondaryLayer.Stop();
                        currentPrimaryTrack = track;

                        // Continue monitoring for sequence
                        StartCoroutine(MonitorTrackEnd(primaryLayer, track, OnSequenceTrackFinished));
                    });
            }
            else
            {
                // Just fade in
                primaryLayer.clip = musicData.Clip;
                primaryLayer.time = Mathf.Min(time, primaryLayer.clip.length - 0.1f);

                float targetVolume = CalculateVolume(musicData);
                transitionHelper.FadeIn(primaryLayer, targetVolume, fadeDuration, defaultTransitionCurve, () =>
                {
                    currentPrimaryTrack = track;
                    StartCoroutine(MonitorTrackEnd(primaryLayer, track, OnSequenceTrackFinished));
                });
            }
        }

        #endregion

        #region Track Monitoring

        private IEnumerator MonitorTrackEnd(AudioSource source, MusicIdentifier track, Action onComplete)
        {
            var clipData = GetMusicData(track);
            if (clipData == null || clipData.Clip == null) yield break;

            yield return new WaitUntil(() => source.isPlaying);

            float clipLength = clipData.Clip.length;
            float remainingTime = clipLength - source.time;

            Log($"Monitoring track {track} (remaining: {remainingTime:F1}s)");

            yield return new WaitForSeconds(Mathf.Max(0, remainingTime - 0.1f));

            while (source.isPlaying && source.time < clipLength - 0.05f)
            {
                yield return null;
            }

            Log($"Track {track} finished");
            onComplete?.Invoke();
        }

        #endregion

        #region Helper Methods

        private void PlayMusicOnLayer(AudioSource layer, MusicIdentifier track, bool monitorEnd, Action onComplete = null)
        {
            var musicData = GetMusicData(track);
            if (musicData == null) return;

            layer.clip = musicData.Clip;
            layer.volume = CalculateVolume(musicData);
            layer.loop = false;
            layer.Play();

            if (monitorEnd && onComplete != null)
            {
                StartCoroutine(MonitorTrackEnd(layer, track, onComplete));
            }
        }

        private MusicClipData GetMusicData(MusicIdentifier id)
        {
            return MusicClipData.GetMusicClipDataById(id);
        }

        private float CalculateVolume(MusicClipData clipData)
        {
            if (clipData == null || audioManager == null) return 0f;
            return audioManager.CalculateMusicVolume(clipData.Volume);
        }

        private void UpdateVolumes()
        {
            Log("Updating volumes");

            if (currentPrimaryTrack.HasValue)
            {
                var data = GetMusicData(currentPrimaryTrack.Value);
                if (data != null) primaryLayer.volume = CalculateVolume(data);
            }

            if (currentSecondaryTrack.HasValue)
            {
                var data = GetMusicData(currentSecondaryTrack.Value);
                if (data != null) secondaryLayer.volume = CalculateVolume(data) * backgroundLayerMultiplier;
            }
        }

        private void ShuffleSequence(List<MusicIdentifier> sequence)
        {
            for (int i = sequence.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (sequence[i], sequence[j]) = (sequence[j], sequence[i]);
            }

            Log("Shuffled sequence");
        }

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[GameMusicManager] {message}");
            }
        }

        #endregion

        #region Public API

        public void TriggerEvent(MusicEventType eventType, string data = null)
        {
            EventBus.Publish(new MusicEventRequest(eventType, data));
        }

        public void EndEvent(MusicEventType eventType)
        {
            EventBus.Publish(new MusicEventEnd(eventType));
        }

        public void ClearAllEvents()
        {
            Log("Clearing all events");

            currentEventType = null;
            isPlayingEventMusic = false;
            stateStack.Clear();

            StartDefaultMusic();
        }

        public void StopAll()
        {
            Log("Stopping all music");

            transitionHelper.StopAllTransitions();
            StopAllCoroutines();

            primaryLayer.Stop();
            secondaryLayer.Stop();

            currentPrimaryTrack = null;
            currentSecondaryTrack = null;
            stateStack.Clear();
        }

        #endregion
    }
}
