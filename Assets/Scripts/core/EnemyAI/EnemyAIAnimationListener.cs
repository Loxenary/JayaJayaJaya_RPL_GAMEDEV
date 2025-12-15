using UnityEngine;

namespace EnemyAI
{
    /// <summary>
    /// Listens to animation events from EventBus and plays them on the Animator.
    /// This decouples the animation triggering logic from the AI state machine,
    /// allowing for more flexible animation control.
    ///
    /// Usage:
    /// EventBus.Publish(EnemyAnimationPlay.Simple("Walk"));
    /// EventBus.Publish(EnemyAnimationPlay.WithCrossFade("Run", 0.2f));
    /// EventBus.Publish(EnemyAnimationTrigger.Create("Attack"));
    /// EventBus.Publish(EnemyAnimationBool.Create("IsMoving", true));
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyAIAnimationListener : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyAnimationPlay>(OnAnimationPlay);
            EventBus.Subscribe<EnemyAnimationTrigger>(OnAnimationTrigger);
            EventBus.Subscribe<EnemyAnimationBool>(OnAnimationBool);
            EventBus.Subscribe<EnemyAnimationFloat>(OnAnimationFloat);
            EventBus.Subscribe<EnemyAnimationInt>(OnAnimationInt);

            Log("Subscribed to animation events");
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<EnemyAnimationPlay>(OnAnimationPlay);
            EventBus.Unsubscribe<EnemyAnimationTrigger>(OnAnimationTrigger);
            EventBus.Unsubscribe<EnemyAnimationBool>(OnAnimationBool);
            EventBus.Unsubscribe<EnemyAnimationFloat>(OnAnimationFloat);
            EventBus.Unsubscribe<EnemyAnimationInt>(OnAnimationInt);

            Log("Unsubscribed from animation events");
        }

        private void OnAnimationPlay(EnemyAnimationPlay evt)
        {
            if (animator == null)
            {
                LogWarning("Animator is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(evt.animationName))
            {
                LogWarning("Animation name is empty!");
                return;
            }

            if (evt.useCrossFade)
            {
                animator.CrossFade(evt.animationName, evt.crossFadeDuration, evt.layerIndex, evt.normalizedTime);
                Log($"CrossFade animation: {evt.animationName} (duration: {evt.crossFadeDuration}s, layer: {evt.layerIndex})");
            }
            else
            {
                animator.Play(evt.animationName, evt.layerIndex, evt.normalizedTime);
                Log($"Play animation: {evt.animationName} (layer: {evt.layerIndex}, time: {evt.normalizedTime})");
            }
        }

        private void OnAnimationTrigger(EnemyAnimationTrigger evt)
        {
            if (animator == null)
            {
                LogWarning("Animator is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(evt.triggerName))
            {
                LogWarning("Trigger name is empty!");
                return;
            }

            animator.SetTrigger(evt.triggerName);
            Log($"Set trigger: {evt.triggerName}");
        }

        private void OnAnimationBool(EnemyAnimationBool evt)
        {
            if (animator == null)
            {
                LogWarning("Animator is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(evt.parameterName))
            {
                LogWarning("Parameter name is empty!");
                return;
            }

            animator.SetBool(evt.parameterName, evt.value);
            Log($"Set bool parameter: {evt.parameterName} = {evt.value}");
        }

        private void OnAnimationFloat(EnemyAnimationFloat evt)
        {
            if (animator == null)
            {
                LogWarning("Animator is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(evt.parameterName))
            {
                LogWarning("Parameter name is empty!");
                return;
            }

            animator.SetFloat(evt.parameterName, evt.value);
            Log($"Set float parameter: {evt.parameterName} = {evt.value}");
        }

        private void OnAnimationInt(EnemyAnimationInt evt)
        {
            if (animator == null)
            {
                LogWarning("Animator is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(evt.parameterName))
            {
                LogWarning("Parameter name is empty!");
                return;
            }

            animator.SetInteger(evt.parameterName, evt.value);
            Log($"Set int parameter: {evt.parameterName} = {evt.value}");
        }

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[EnemyAIAnimationListener] {message}", this);
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[EnemyAIAnimationListener] {message}", this);
        }

        private void OnValidate()
        {
            // Auto-find Animator component if not assigned
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
    }
}
