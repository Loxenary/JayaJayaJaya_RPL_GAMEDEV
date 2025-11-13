using System;
using UnityEngine;

[Serializable]
public class FrameAnimationController
{
    private Animator _animator;

    public Animator Animator => _animator;

    private string _currentAnimation;

    public FrameAnimationController(Animator animator)
    {
        _animator = animator;
    }

    public void PlayAnimation(string animationName)
    {
        if (_currentAnimation == animationName) return;

        _animator.Play(animationName);
        _currentAnimation = animationName;
    }

    public void StopAnimation()
    {
        _animator.StopPlayback();
        _currentAnimation = null;
    }
        
    public void SetTrigger(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }
}