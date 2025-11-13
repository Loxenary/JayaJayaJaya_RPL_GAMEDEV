using System;
using System.Collections.Generic;
using System.Linq;

public static class EventBus
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<Type, Delegate> _eventTable = new();

    // --- DEBUGGING FIELDS (Editor Only) ---
    #if UNITY_EDITOR
    private static readonly Dictionary<Type, int> _publishCounts = new();
    private static readonly List<string> _publishHistory = new();
    private const int HistoryLimit = 50;
    #endif

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        lock (_lock)
        {
            if (_eventTable.TryGetValue(type, out var existing))
            {
                _eventTable[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _eventTable[type] = handler;
            }
        }
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        lock (_lock)
        {
            if (_eventTable.TryGetValue(type, out var existing))
            {
                var current = Delegate.Remove(existing, handler);
                if (current == null)
                    _eventTable.Remove(type);
                else
                    _eventTable[type] = current;
            }
        }
    }
    
    // Add this method inside the #if UNITY_EDITOR block in EventBus.cs

    /// <summary>
    /// Returns a dictionary mapping event names to a list of their subscriber types' names.
    /// </summary>
    public static Dictionary<string, List<string>> GetSubscriberDetails()
    {
        lock (_lock)
        {
            var details = new Dictionary<string, List<string>>();
            foreach (var kvp in _eventTable)
            {
                var eventName = kvp.Key.Name;
                var subscribers = new List<string>();
                foreach (var del in kvp.Value.GetInvocationList())
                {
                    // Get the type name of the object that the delegate is targeting.
                    // If del.Target is null, it's a static method.
                    string subscriberName = del.Target?.GetType().Name ?? "Static Method";
                    subscribers.Add(subscriberName);
                }
                details[eventName] = subscribers;
            }
            return details;
        }
    }

    public static void Publish<T>(T eventData)
    {
        Delegate d;
        var type = typeof(T);

        // --- DEBUGGING LOGIC (Editor Only) ---
#if UNITY_EDITOR
        lock (_lock)
        {
            // Increment the publish count for this event type
            _publishCounts.TryGetValue(type, out var count);
            _publishCounts[type] = count + 1;

            // Add to the history log
            _publishHistory.Insert(0, $"[{System.DateTime.Now:HH:mm:ss}] {type.Name}");
            if (_publishHistory.Count > HistoryLimit)
            {
                _publishHistory.RemoveAt(HistoryLimit);
            }
        }
#endif

        lock (_lock)
        {
            _eventTable.TryGetValue(type, out d);
        }

        if (d != null)
        {
            (d as Action<T>)?.Invoke(eventData);
        }
    }

    public static void ClearAll()
    {
        lock (_lock)
        {
            _eventTable.Clear();
            #if UNITY_EDITOR
            _publishCounts.Clear();
            _publishHistory.Clear();
            #endif
        }
    }

    // --- DEBUGGING METHODS (Editor Only) ---
    #if UNITY_EDITOR
    /// <summary>
    /// Returns a snapshot of the current event bus state for debugging.
    /// </summary>
    public static Dictionary<string, int> GetSubscriberCounts()
    {
        lock (_lock)
        {
            return _eventTable.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value.GetInvocationList().Length
            );
        }
    }

    public static Dictionary<string, int> GetPublishCounts()
    {
        lock (_lock)
        {
            return _publishCounts.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value
            );
        }
    }

    public static List<string> GetPublishHistory()
    {
        lock (_lock)
        {
            return new List<string>(_publishHistory);
        }
    }
    #endif
}
