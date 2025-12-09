using UnityEngine;
using CustomLogger;
using System.Collections.Generic;

public class TimeService : ServiceBase<TimeService>
{
    private static int _t_stop_counter = 0;

    [SerializeField, ReadOnly] private float time_counter = 0;

    private static Queue<object> _stopperQueue = new();

    public void RequestStopTime(object requesterObject)
    {
        _t_stop_counter++;
        _stopperQueue.Enqueue(requesterObject);
        HandleTimeInternal();
    }

    private void HandleTimeInternal()
    {
        // Stop Time
        if (_t_stop_counter > 0)
        {
            Time.timeScale = 0;
        }

        // Resume Time
        if (_t_stop_counter <= 0)
        {
            Time.timeScale = 1;
        }

        time_counter = _t_stop_counter;
    }

    public void RequestResumeTime(object requesterObject)
    {
        _t_stop_counter--;

        if (_stopperQueue.Count > 0 && _stopperQueue.TryDequeue(out var resumer) && !resumer.Equals(requesterObject))
        {
            BetterLogger.LogError($"The object that requested resume time {requesterObject.GetType().Name} is not the same as the object that requested stop time {resumer.GetType().Name}", BetterLogger.LogCategory.System);
            return;
        }

        HandleTimeInternal();
    }

    public void RequestResumeWhileClearingQueue()
    {
        _stopperQueue.Clear();
        Time.timeScale = 1f;
        _t_stop_counter = 0;
        time_counter = 0;
    }

    [ContextMenu("Log Stop Queue")]
    public void LogQueue()
    {
        BetterLogger.LogQueue(_stopperQueue, "Time Service Stop Queue", BetterLogger.LogCategory.System);
    }

}