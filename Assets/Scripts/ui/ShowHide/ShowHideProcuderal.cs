using System.Collections;
using UnityEngine;

/// <summary>
/// ShowHide implementation using procedural animations (code-based).
/// Uses coroutines to animate scale, fade, or position over time.
/// Useful for simple UI transitions without needing animator setup.
/// </summary>
public class ShowHideProcedural : ShowHideBase
{
    [Header("Procedural Animation Settings")]
    [Tooltip("Type of procedural animation to use")]
    [SerializeField] private ProceduralAnimationType animationType = ProceduralAnimationType.Scale;

    [Tooltip("Duration of the show/hide animation in seconds")]
    [SerializeField] private float animationDuration = 0.3f;

    [Tooltip("Animation curve for the transition")]
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Animation")]
    [Tooltip("Scale when hidden")]
    [SerializeField] private Vector3 hiddenScale = Vector3.zero;

    [Tooltip("Scale when shown")]
    [SerializeField] private Vector3 shownScale = Vector3.one;

    [Header("Fade Animation")]
    [Tooltip("Canvas group for fade animation (required if using Fade type)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Position Animation")]
    [Tooltip("Offset position when hidden (relative to current position)")]
    [SerializeField] private Vector3 hiddenPositionOffset = new Vector3(0, -100, 0);

    private Vector3 _originalPosition;
    private Coroutine _currentAnimation;

    private void Start()
    {
        _originalPosition = transform.localPosition;

        // Initialize to hidden state if needed
        if (animationType == ProceduralAnimationType.Fade && canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
    }

    protected override void ShowInternal()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(AnimateShow());
    }

    protected override void HideInternal()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(AnimateHide());
    }

    private IEnumerator AnimateShow()
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);

            ApplyAnimation(t, true);

            yield return null;
        }

        // Ensure we end at exactly the target values
        ApplyAnimation(1f, true);

        _currentAnimation = null;
        OnShowComplete();
    }

    private IEnumerator AnimateHide()
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);

            ApplyAnimation(1f - t, true);

            yield return null;
        }

        // Ensure we end at exactly the target values
        ApplyAnimation(0f, true);

        _currentAnimation = null;
        OnHideComplete();
    }

    private void ApplyAnimation(float t, bool isShowing)
    {
        switch (animationType)
        {
            case ProceduralAnimationType.Scale:
                transform.localScale = Vector3.Lerp(hiddenScale, shownScale, t);
                break;

            case ProceduralAnimationType.Fade:
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                    canvasGroup.interactable = isShowing && t > 0.5f;
                    canvasGroup.blocksRaycasts = isShowing && t > 0.5f;
                }
                else
                {
                    Debug.LogWarning("[ShowHideProcedural] Fade animation requires a CanvasGroup component!");
                }
                break;

            case ProceduralAnimationType.Position:
                Vector3 hiddenPos = _originalPosition + hiddenPositionOffset;
                transform.localPosition = Vector3.Lerp(hiddenPos, _originalPosition, t);
                break;

            case ProceduralAnimationType.ScaleAndFade:
                transform.localScale = Vector3.Lerp(hiddenScale, shownScale, t);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                    canvasGroup.interactable = isShowing && t > 0.5f;
                    canvasGroup.blocksRaycasts = isShowing && t > 0.5f;
                }
                break;
        }
    }

    private void OnValidate()
    {
        // Auto-find or create CanvasGroup if using fade animations
        if (animationType == ProceduralAnimationType.Fade ||
            animationType == ProceduralAnimationType.ScaleAndFade)
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}

public enum ProceduralAnimationType
{
    Scale,
    Fade,
    Position,
    ScaleAndFade
}