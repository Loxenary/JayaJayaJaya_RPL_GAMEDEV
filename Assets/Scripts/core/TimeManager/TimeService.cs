using UnityEngine;
using CustomLogger;
using System.Collections.Generic;

public class TimeService : ServiceBase<TimeService>
{
    // Instance fields (bukan static) — state static bertahan melewati scene
    // reload dan bisa mengunci timeScale=0 permanen (B-08).
    private int _t_stop_counter = 0;

#if UNITY_EDITOR
    [ReadOnly]
#endif
    private float time_counter = 0;

    private Queue<object> _stopperQueue = new();

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
        // Clamp — resume tak berpasangan tidak boleh membuat counter negatif
        _t_stop_counter = Mathf.Max(0, _t_stop_counter - 1);

        // Keluarkan entri milik requester ini bila ada di depan antrean;
        // jangan dequeue buta yang bisa membuang entri stopper lain.
        if (_stopperQueue.Count > 0 && ReferenceEquals(_stopperQueue.Peek(), requesterObject))
        {
            _stopperQueue.Dequeue();
        }
        else if (_stopperQueue.Count > 0)
        {
            BetterLogger.Log($"Resume oleh {requesterObject} tidak cocok dengan kepala antrean stopper", BetterLogger.LogCategory.System);
        }

        if (_t_stop_counter > 0)
        {
            BetterLogger.Log("Stay paused due to time stopper still > 0", BetterLogger.LogCategory.System);
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
        HandleTimeInternal();
    }

    [ContextMenu("Log Stop Queue")]
    public void LogQueue()
    {
        BetterLogger.LogQueue(_stopperQueue, "Time Service Stop Queue", BetterLogger.LogCategory.System);
    }

}
