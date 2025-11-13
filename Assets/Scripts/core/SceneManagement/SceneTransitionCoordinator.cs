// File: SceneTransitionCoordinator.cs
using System;
using System.Threading.Tasks;

/// <summary>
/// Coordinates scene transitions by wrapping scene operations with transition effects.
/// This separates transition timing/logic from the core scene loading logic.
/// </summary>
public class SceneTransitionCoordinator
{
    private readonly ISceneTransitionHandler _transitionHandler;

    public SceneTransitionCoordinator(ISceneTransitionHandler transitionHandler)
    {
        _transitionHandler = transitionHandler ?? throw new ArgumentNullException(nameof(transitionHandler));
    }

    /// <summary>
    /// Executes a scene operation wrapped with transition effects.
    /// </summary>
    /// <param name="sceneOperation">The async operation to perform (e.g., loading/unloading scenes)</param>
    /// <param name="useTransition">Whether to apply transition effects</param>
    public async Task ExecuteWithTransition(Func<Task> sceneOperation, bool useTransition = true)
    {
        if (sceneOperation == null)
        {
            throw new ArgumentNullException(nameof(sceneOperation));
        }

        if (useTransition)
        {
            // Show transition and wait for it to cover the screen
            await _transitionHandler.ShowTransition();
        }

        // Execute the actual scene operation
        await sceneOperation();

        if (useTransition)
        {
            // Hide the transition
            await _transitionHandler.HideTransition();
        }
    }
}
