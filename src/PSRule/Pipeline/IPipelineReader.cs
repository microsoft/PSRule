// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Pipeline;

internal interface IPipelineReader
{
    int Count { get; }

    bool IsEmpty { get; }

    /// <summary>
    /// Add a new object into the stream.
    /// </summary>
    /// <param name="sourceObject">An object to process.</param>
    /// <param name="targetType">A pre-bound type.</param>
    /// <param name="skipExpansion">Determines if expansion is skipped.</param>
    void Enqueue(object sourceObject, string? targetType = null, bool skipExpansion = false);

    bool TryDequeue(out ITargetObject sourceObject);

    void Open();

    /// <summary>
    /// Add a path to the list of inputs.
    /// </summary>
    /// <param name="path">The path of files to add.</param>
    void Add(string path);
}

#nullable restore
