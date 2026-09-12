// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A context for registering scoped runtime services within a factory.
/// </summary>
public interface IRuntimeServiceCollection : IDisposable
{
    /// <summary>
    /// The name of the scope.
    /// </summary>
    string ScopeName { get; }

    /// <summary>
    /// Access configuration values at runtime.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Add a service.
    /// </summary>
    /// <typeparam name="TInterface">The specified interface type.</typeparam>
    /// <typeparam name="TService">The concrete type to add.</typeparam>
    void AddService<TInterface, TService>()
        where TInterface : class
        where TService : class, TInterface;

    /// <summary>
    /// Add a service.
    /// </summary>
    /// <param name="instanceName">A unique name of the service instance.</param>
    /// <param name="instance">An instance of the service.</param>
    void AddService(string instanceName, object instance);

    /// <summary>
    /// Add a convention.
    /// </summary>
    /// <typeparam name="TConvention">The convention type to add.</typeparam>
    void AddConvention<TConvention>()
        where TConvention : class;
}
