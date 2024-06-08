// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Options;

/// <summary>
/// Determine the rule severity level at which to break the pipeline.
/// </summary>
public enum BreakLevel
{
    /// <summary>
    /// No preference.
    /// Inherits the default of <c>OnError</c>.
    /// </summary>
    None = 0,

    /// <summary>
    /// Continue even if a rule fails regardless of rule severity.
    /// </summary>
    Never = 1,

    /// <summary>
    /// Only break on error.
    /// </summary>
    OnError = 2,

    /// <summary>
    /// Break if any rule of warning or error severity fails.
    /// </summary>
    OnWarning = 3,

    /// <summary>
    /// Break if any rule fails.
    /// </summary>
    OnInformation = 4
}
