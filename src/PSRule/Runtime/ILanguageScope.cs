// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Runtime.Binding;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// A named scope for language elements.
/// Any elements in a language scope are not visible to language elements in another scope.
/// </summary>
internal interface ILanguageScope : IDisposable
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
    /// Get a previously added service.
    /// </summary>
    object? GetService(string name);

    ITargetBindingResult? Bind(TargetObject targetObject);

    ITargetBindingResult? Bind(object targetObject);

    /// <summary>
    /// Try to bind the type of the object.
    /// </summary>
    bool TryGetType(object o, out string? type, out string? path);

    /// <summary>
    /// Try to bind the name of the object.
    /// </summary>
    bool TryGetName(object o, out string? name, out string? path);
}

#nullable restore
