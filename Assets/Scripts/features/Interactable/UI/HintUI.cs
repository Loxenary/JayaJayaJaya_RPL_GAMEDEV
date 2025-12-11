using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// UI component that displays hint messages.
/// Automatically shows hints via EventBus and hides after a specified duration.
/// </summary>
public class HintUI : FadeShowHideProcedural
{
    [Header("Hint UI Components")]
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Default Settings")]
    [Tooltip("Default display duration if event doesn't specify one")]
    [SerializeField] private float defaultDisplayDuration = 3f;

    private Coroutine hideCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe<HintShown>(OnHintShown);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<HintShown>(OnHintShown);

        // Clean up coroutine if still running
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private void OnHintShown(HintShown evt)
    {
        // Stop any existing hide coroutine
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // Update hint text
        if (hintText != null)
        {
            hintText.text = evt.hintText;
        }
        else
        {
            Debug.LogWarning("[HintUI] Hint text component is not assigned!", this);
        }

        // Show the UI
        ShowUI();

        // Determine display duration (use event duration or default)
        float duration = evt.displayDuration > 0 ? evt.displayDuration : defaultDisplayDuration;

        // Schedule auto-hide
        hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideUI();
        hideCoroutine = null;
    }

    private void OnValidate()
    {
        // Auto-find TextMeshProUGUI if not assigned
        if (hintText == null)
        {
            hintText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}
