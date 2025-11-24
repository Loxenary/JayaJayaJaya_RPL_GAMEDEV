using UnityEngine;

/// <summary>
/// ShowHide implementation using fade-based procedural animation.
/// Uses a CanvasGroup to smoothly fade the UI element in and out.
/// Automatically manages interactability and raycast blocking based on visibility.
/// </summary>
public class FadeShowHideProcedural : BaseShowHideProcedural
{
    [Header("Fade Animation Settings")]
    [Tooltip("Canvas group for fade animation (required)")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        // Initialize to hidden state if needed
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
    }

    protected override void ApplyAnimation(float t, bool isShowing)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = t;
            canvasGroup.interactable = isShowing && t > 0.5f;
            canvasGroup.blocksRaycasts = isShowing && t > 0.5f;
        }
        else
        {
            Debug.LogWarning("[FadeShowHideProcedural] Fade animation requires a CanvasGroup component!");
        }
    }

    private void OnValidate()
    {
        // Auto-find or create CanvasGroup
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
