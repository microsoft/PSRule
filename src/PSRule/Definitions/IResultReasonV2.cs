// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// A reason for the rule result.
/// </summary>
public interface IResultReasonV2
{
    /// <summary>
    /// The object path that failed.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// The object path including the path of the parent object.
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// The reason message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Return a formatted reason string.
    /// </summary>
    string Format();
}
