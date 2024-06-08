// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A set of log levels which indicate different types of diagnostic messages.
/// </summary>
[Flags]
internal enum LogLevel
{
    None = 0,

    Error = 1,

    Warning = 2,

    Info = 4,

    Verbose = 8,

    Debug = 16,
}
