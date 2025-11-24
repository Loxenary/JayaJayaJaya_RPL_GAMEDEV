using UnityEngine;

/// <summary>
/// ShowHide implementation using Unity Animator.
/// Requires animator states/triggers named "Show" and "Hide".
/// Uses animation events to detect when transitions complete.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ShowHideAnimation : ShowHideBase
{
    [Header("Animation Settings")]
    [Tooltip("Trigger name for show animation")]
    [SerializeField] private string showTriggerName = "Show";

    [Tooltip("Trigger name for hide animation")]
    [SerializeField] private string hideTriggerName = "Hide";

    [Tooltip("If true, uses triggers. If false, uses PlayAnimation with state names.")]
    [SerializeField] private bool useTriggers = true;

    private FrameAnimationController _animationController;

    private void Awake()
    {
        _animationController = new FrameAnimationController(GetComponent<Animator>());
    }

    protected override void ShowInternal()
    {
        if (useTriggers)
        {
            _animationController.SetTrigger(showTriggerName);
        }
        else
        {
            _animationController.PlayAnimation(showTriggerName);
        }
    }

    protected override void HideInternal()
    {
        if (useTriggers)
        {
            _animationController.SetTrigger(hideTriggerName);
        }
        else
        {
            _animationController.PlayAnimation(hideTriggerName);
        }
    }

    /// <summary>
    /// Call this from an Animation Event at the end of your Show animation.
    /// Add an Animation Event in your animator's Show state at the last frame.
    /// </summary>
    public void AnimationEvent_ShowComplete()
    {
        OnShowComplete();
    }

    /// <summary>
    /// Call this from an Animation Event at the end of your Hide animation.
    /// Add an Animation Event in your animator's Hide state at the last frame.
    /// </summary>
    public void AnimationEvent_HideComplete()
    {
        OnHideComplete();
    }
}