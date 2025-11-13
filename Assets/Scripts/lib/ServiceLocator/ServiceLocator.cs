using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class ServiceLocator
{
    // Registered service instances by type
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>
    /// Fired immediately after any service is registered.
    /// Subscribers receive the concrete service type.
    /// </summary>
    public static event Action<Type> ServiceRegistered;

    /// <summary>
    /// Register a service instance under its concrete type.
    /// Must be called (for example) in Awake() of each manager.
    /// </summary>
    public static void Register<T>(T service) where T : IService
    {
        var t = typeof(T);

        if (_services.ContainsKey(t))
            Debug.LogWarning($"ServiceLocator: Overwriting existing registration for {t.Name}");

        _services[t] = service;

        // Broadcast that a new service was registered
        ServiceRegistered?.Invoke(t);
    }

    /// <summary>
    /// Retrieve a registered service, or null if none.
    /// </summary>
    public static T Get<T>() where T : class, IService
    {
        _services.TryGetValue(typeof(T), out var svc);

        if (svc == null)
        {
            Debug.LogWarning($"[SERVICE WARNING] : No Service of type {typeof(T)} can be found");
        }
        return svc as T;
    }

    /// <summary>
    /// Retrieves all registered services that implement a specific interface or are of a specific type.
    /// </summary>
    public static IEnumerable<T> GetAll<T>() where T : IService
    {
        // Find all values in our dictionary that can be cast to type T
        return _services.Values.OfType<T>();
    }
}
