using System;
using System.Threading.Tasks;
using CustomLogger;

/// <summary>
/// Utility untuk task fire-and-forget TANPA menelan exception (B-15).
/// Pola pemakaian — entry point tetap kompatibel UnityEvent/button:
///   public void OnRestartClicked() => RestartAsync().Forget(nameof(OnRestartClicked));
///   private async Task RestartAsync() { ... }
/// </summary>
public static class TaskExtensions
{
    public static async void Forget(this Task task, string context = null)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            BetterLogger.LogError(
                $"Unhandled async exception{(context != null ? $" [{context}]" : string.Empty)}: {ex}",
                BetterLogger.LogCategory.System);
        }
    }
}
