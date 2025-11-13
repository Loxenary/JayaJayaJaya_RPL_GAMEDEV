// File: NullTransitionHandler.cs
using System.Threading.Tasks;

/// <summary>
/// A no-op transition handler that performs no transitions.
/// Useful for testing or when you want instant scene changes.
/// </summary>
public class NullTransitionHandler : ISceneTransitionHandler
{
    public Task ShowTransition()
    {
        return Task.CompletedTask;
    }

    public Task HideTransition()
    {
        return Task.CompletedTask;
    }
}
