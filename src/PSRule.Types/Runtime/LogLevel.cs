// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A set of log levels which indicate different types of diagnostic messages.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 
    /// </summary>
    Trace = 0,

    /// <summary>
    /// 
    /// </summary>
    Debug = 1,

    /// <summary>
    /// 
    /// </summary>
    Information = 2,

    /// <summary>
    /// 
    /// </summary>
    Warning = 3,

    /// <summary>
    /// 
    /// </summary>
    Error = 4,

    /// <summary>
    /// 
    /// </summary>
    Critical = 5,

    /// <summary>
    /// 
    /// </summary>
    None = 6
}
