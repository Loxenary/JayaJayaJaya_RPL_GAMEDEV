using System.Collections.Generic;

public static class RestartManager
{
    private static List<IRestartable> restartables = new();

    public static void Register(IRestartable restartable)
    {
        if (restartables.Contains(restartable))
        {
            return;
        }
        restartables.Add(restartable);
    }

    public static void Unregister(IRestartable restartable)
    {
        if (restartables.Contains(restartable))
        {
            restartables.Remove(restartable);
        }
    }

    public static void Restart()
    {
        // Iterasi atas salinan — Restart() sebuah IRestartable boleh memicu
        // Register/Unregister tanpa melempar InvalidOperationException (B-13).
        foreach (IRestartable restartable in restartables.ToArray())
        {
            restartable.Restart();
        }

        // Pastikan pause counter tidak nyangkut melewati restart (B-08):
        // timeScale kembali 1 apa pun kondisi antrean stopper sebelumnya.
        ServiceLocator.Get<TimeService>()?.RequestResumeWhileClearingQueue();
    }

}
