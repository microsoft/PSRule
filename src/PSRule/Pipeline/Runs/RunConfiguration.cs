// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// Configuration for a run.
/// </summary>
[DebuggerDisplay("{Guid}: {Configuration}")]
internal sealed class RunConfiguration(IReadOnlyDictionary<string, object> configuration)
{
    /// <summary>
    /// A unique identifier for the run configuration instance.
    /// </summary>
    public string Guid { get; } = System.Guid.NewGuid().ToString();

    public IReadOnlyDictionary<string, object> Configuration { get; } = configuration;
}
