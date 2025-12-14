using UnityEngine;

/// <summary>
/// Event published when an image should be shown to the player
/// </summary>
public struct ImageShown
{
    public Sprite imageSprite;
}

/// <summary>
/// Event published when an image should be hidden
/// </summary>
public struct ImageHidden
{
}

/// <summary>
/// Event published when an image should be toggled (shown if hidden, hidden if shown)
/// </summary>
public struct ImageToggled
{
    public Sprite imageSprite; // Optional: update sprite when toggling to show
}
