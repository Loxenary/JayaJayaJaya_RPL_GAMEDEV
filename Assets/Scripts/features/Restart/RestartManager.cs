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
        foreach (IRestartable restartable in restartables)
        {
            restartable.Restart();
        }
    }
    
}