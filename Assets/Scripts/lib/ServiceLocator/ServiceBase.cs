using UnityEngine;

/// <summary>
/// Base class for services that automatically registers itself with the ServiceLocator.
/// Inherit from this instead of MonoBehaviour to avoid forgetting to register manually.
/// </summary>
public abstract class ServiceBase<T> : MonoBehaviour, IService where T : ServiceBase<T>
{
    protected virtual void Awake()
    {
        ServiceLocator.Register((T)this);
    }

    protected virtual void OnDestroy()
    {
        
    }
}

/// <summary>
/// Base class for services that require initialization and automatically registers itself.
/// Inherit from this for services that need async initialization.
/// </summary>
public abstract class InitializableServiceBase<T> : MonoBehaviour, IInitializableService where T : InitializableServiceBase<T>
{
    public abstract ServicePriority InitializationPriority { get; }

    protected virtual void Awake()
    {
        ServiceLocator.Register((T)this);
    }

    public abstract System.Threading.Tasks.Task Initialize();

    protected virtual void OnDestroy()
    {
        
    }
}
