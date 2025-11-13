// File: ISceneTransitionHandler.cs
using System.Threading.Tasks;

/// <summary>
/// Interface for handling scene transition animations.
/// Allows different implementations (UI-based, event-based, or none).
/// </summary>
public interface ISceneTransitionHandler
{
    /// <summary>
    /// Shows the transition effect and waits for it to complete.
    /// </summary>
    Task ShowTransition();

    /// <summary>
    /// Hides/closes the transition effect.
    /// </summary>
    Task HideTransition();
}
