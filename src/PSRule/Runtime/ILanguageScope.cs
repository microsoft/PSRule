// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime.Binding;

namespace PSRule.Runtime;

/// <summary>
/// A named scope for language elements.
/// Any elements in a language scope are not visible to language elements in another scope.
/// </summary>
public interface ILanguageScope : IDisposable
{
    /// <summary>
    /// The name of the scope.
    /// </summary>
    string Name { get; }

    StringComparer GetBindingComparer();

    /// <summary>
    /// Get an ordered culture preference list which will be tries for finding help.
    /// </summary>
    string[]? Culture { get; }

    void Configure(OptionContext context);

    /// <summary>
    /// Try to get a specific configuration value by name.
    /// </summary>
    bool TryConfigurationValue(string key, out object? value);

    /// <summary>
    /// Add a filter to the language scope.
    /// </summary>
    void WithFilter(IResourceFilter resourceFilter);

    /// <summary>
    /// Get a filter for a specific resource kind.
    /// </summary>
    IResourceFilter? GetFilter(ResourceKind kind);

    /// <summary>
    /// Add a service to the scope.
    /// </summary>
    void AddService(string name, object service);

    /// <summary>
    /// Configure a service in the scope.
    /// </summary>
    /// <param name="configure">A delegate action to call to configure services.</param>
    void ConfigureServices(Action<IRuntimeServiceCollection>? configure);

    /// <summary>
    /// Get a previously added service.
    /// </summary>
    object? GetService(string name);

    /// <summary>
    /// Get any emitters added to the scope.
    /// </summary>
    IEnumerable<Type> GetEmitters();

    /// <summary>
    /// Get any conventions added to the scope.
    /// </summary>
    IEnumerable<Type> GetConventions();

    ITargetBindingResult? Bind(ITargetObject targetObject);

    /// <summary>
    /// Try to bind the type of the object.
    /// </summary>
    bool TryGetType(ITargetObject o, out string? type, out string? path);

    /// <summary>
    /// Try to bind the name of the object.
    /// </summary>
    bool TryGetName(ITargetObject o, out string? name, out string? path);

    /// <summary>
    /// Try to get a rule override by resource ID.
    /// </summary>
    bool TryGetOverride(ResourceId id, out RuleOverride? propertyOverride);

    /// <summary>
    /// Get the current scope as a configuration object.
    /// </summary>
    /// <returns>Returns an <see cref="IConfiguration"/> instance.</returns>
    IConfiguration ToConfiguration();
}
