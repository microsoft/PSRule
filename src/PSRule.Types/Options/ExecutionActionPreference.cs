// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Options;

/// <summary>
/// Determines the action to take for execution behaviors.
/// See <see cref="ExecutionOption"/> for the specific behaviors that are configurable.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ExecutionActionPreference
{
    /// <summary>
    /// No preference.
    /// This will inherit from the default.
    /// </summary>
    None = 0,

    /// <summary>
    /// Continue to execute silently.
    /// </summary>
    Ignore = 1,

    /// <summary>
    /// Continue to execute but log a warning.
    /// </summary>
    Warn = 2,

    /// <summary>
    /// Generate an error.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Continue to execute but write a debug log.
    /// </summary>
    Debug = 4
}
