using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// MonoBehaviour that listens to scene transition events and animates the transition UI.
/// Subscribe to EventBus events: ShowTransitionEvent and CloseTransitionEvent
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class TransitionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The image that covers the screen during transition (usually a black panel)")]
    [SerializeField] private Image transitionPanel;

    [Header("Animation Settings")]
    [Tooltip("Duration of the fade in/out animation")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("Easing curve for the fade animation")]
    [SerializeField] private Ease fadeEase = Ease.InOutQuad;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private CanvasGroup canvasGroup;
    private Tweener currentTween;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (transitionPanel == null)
        {
            transitionPanel = GetComponentInChildren<Image>();
        }

        // Start hidden
        SetVisibility(0f);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ShowTransitionEvent>(OnShowTransition);
        EventBus.Subscribe<CloseTransitionEvent>(OnCloseTransition);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ShowTransitionEvent>(OnShowTransition);
        EventBus.Unsubscribe<CloseTransitionEvent>(OnCloseTransition);
    }

    private void OnShowTransition(ShowTransitionEvent evt)
    {
        Log("Showing transition...");

        // Kill any existing tween
        currentTween?.Kill();

        // Calculate duration based on animation speed
        float duration = fadeDuration / evt.animationSpeed;

        // Fade in to cover the screen
        currentTween = canvasGroup.DOFade(1f, duration)
            .SetEase(fadeEase)
            .SetUpdate(true) // Use unscaled time so it works even when Time.timeScale = 0
            .OnComplete(() =>
            {
                Log("Transition fully visible");
                currentTween = null;
            });
    }

    private void OnCloseTransition(CloseTransitionEvent evt)
    {
        Log("Closing transition...");

        // Kill any existing tween
        currentTween?.Kill();

        // Fade out to reveal the new scene
        currentTween = canvasGroup.DOFade(0f, fadeDuration)
            .SetEase(fadeEase)
            .SetUpdate(true) // Use unscaled time
            .OnComplete(() =>
            {
                Log("Transition fully hidden");
                currentTween = null;
            });
    }

    private void SetVisibility(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[TransitionUI] {message}");
        }
    }

    private void OnDestroy()
    {
        // Clean up tweens
        currentTween?.Kill();
    }
}
