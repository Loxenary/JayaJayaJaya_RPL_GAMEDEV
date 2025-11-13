using System;
using System.Threading.Tasks;


public interface IService { }

public enum ServicePriority
{
    PRIMARY,
    SECONDARY,
    LEAST
}

public interface IInitializableService : IService
{
    /// <summary>
    /// Called by GameInitializer to perform any async setup.
    /// </summary>
    Task Initialize();

    ServicePriority InitializationPriority { get; }
}
