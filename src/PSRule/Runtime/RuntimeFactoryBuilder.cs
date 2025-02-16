// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PSRule.Resources;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// Defines a helper class to build runtime service via a runtime factory.
/// A runtime factory implements a constructor for dependency injection and
/// is annotated with the <see cref="RuntimeFactoryAttribute"/> attribute.
/// </summary>
internal sealed class RuntimeFactoryBuilder
{
    private static readonly EventId PSR0014 = new(14, nameof(PSR0014));

    private readonly ILogger? _Logger;
    private readonly ServiceCollection _Services;

    public RuntimeFactoryBuilder(ILogger? logger)
    {
        _Logger = logger;
        _Services = new ServiceCollection();
        _Services.AddSingleton(_Logger ?? NullLogger.Instance);
    }

    public RuntimeFactoryContainer BuildFromAssembly(string scope, Assembly[] assemblies)
    {
        if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentNullException(nameof(scope));
        if (assemblies == null || assemblies.Length == 0) throw new ArgumentException(nameof(assemblies));

        var serviceProvider = _Services.BuildServiceProvider();
        var container = new RuntimeFactoryContainer(scope);

        // Activate any runtime factories from each assembly and add them to the container.
        foreach (var assembly in assemblies)
        {
            if (assembly == null)
                continue;

            foreach (var type in GetRuntimeFactoryTypeForAssembly(assembly))
            {
                try
                {
                    var factory = (IRuntimeFactory)ActivatorUtilities.CreateInstance(serviceProvider, type);
                    if (factory == null)
                        continue;

                    container.AddFactory(factory);
                }
                catch (Exception ex)
                {
                    LogFailedToCreateFactory(ex, type);
                }
            }
        }
        return container;
    }

    /// <summary>
    /// Failed to create runtime factory '{Type}' from '{Assembly}'. {Message}
    /// </summary>
    private void LogFailedToCreateFactory(Exception exception, Type type)
    {
        if (_Logger == null)
            return;

        _Logger.LogError(PSR0014, exception, PSRuleResources.PSR0014, type.FullName, type.Assembly.FullName, exception.Message);
    }

    /// <summary>
    /// Check if the specified type meets the requirements to be a potential runtime service factory.
    /// The type must be public, not abstract, annotated with the <see cref="RuntimeFactoryAttribute"/>, and implement <see cref="IRuntimeFactory"/>.
    /// </summary>
    private static bool IsRuntimeFactory(Type type)
    {
        return type.IsPublic && !type.IsAbstract &&
            type.GetCustomAttribute<RuntimeFactoryAttribute>() != null &&
            typeof(IRuntimeFactory).IsAssignableFrom(type);
    }

    /// <summary>
    /// Get any types marked as a runtime factory in a specified assembly.
    /// </summary>
    private static IEnumerable<Type> GetRuntimeFactoryTypeForAssembly(Assembly assembly)
    {
        return assembly.GetExportedTypes().Where(type => IsRuntimeFactory(type));
    }
}

#nullable restore
