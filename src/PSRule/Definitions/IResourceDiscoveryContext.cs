// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions;

#nullable enable

/// <summary>
/// A context that is used for discovery of resources.
/// </summary>
internal interface IResourceDiscoveryContext
{
    /// <summary>
    /// A writer to log messages.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// The current source file.
    /// </summary>
    ISourceFile? Source { get; }

    /// <summary>
    /// Enter a language scope.
    /// </summary>
    /// <param name="file">The source file to enter.</param>
    void EnterLanguageScope(ISourceFile file);

    /// <summary>
    /// Exit a language scope.
    /// </summary>
    /// <param name="file">The source file to exit.</param>
    void ExitLanguageScope(ISourceFile file);

    void PushScope(RunspaceScope scope);

    void PopScope(RunspaceScope scope);
}

#nullable restore
