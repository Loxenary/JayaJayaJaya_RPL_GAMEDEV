// File: SceneTransitionSettings.cs
using UnityEngine;

/// <summary>
/// ScriptableObject that holds configuration for scene transitions.
/// This allows you to adjust transition settings without touching code.
/// </summary>
[CreateAssetMenu(fileName = "SceneTransitionSettings", menuName = "Scene Management/Transition Settings")]
public class SceneTransitionSettings : ScriptableObject
{
    [Tooltip("Speed of the transition animation. Higher values = faster transitions.")]
    [Range(0.1f, 5f)]
    public float animationSpeed = 1.1f;

    [Tooltip("Whether transitions are enabled by default")]
    public bool enableTransitionsByDefault = true;

    [Tooltip("Type of transition handler to use")]
    public TransitionHandlerType handlerType = TransitionHandlerType.EventBus;
}

public enum TransitionHandlerType
{
    /// <summary>No transitions - instant scene changes</summary>
    None,
    /// <summary>Use EventBus to communicate with TransitionUI</summary>
    EventBus
}
