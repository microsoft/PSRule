// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Definitions;

/// <summary>
/// A context that is used for discovery of resources.
/// </summary>
internal interface IResourceContext : IGetLocalizedPathContext //, IRunspaceScopedContext
{
    /// <summary>
    /// A writer to log messages.
    /// </summary>
    ILogger? Logger { get; }

    ///// <summary>
    ///// The current source file.
    ///// </summary>
    //ISourceFile? Source { get; }

    /// <summary>
    /// The name of the current scope.
    /// </summary>
    string? Scope { get; }

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

    void ReportIssue(ResourceIssue resourceIssue);
}
