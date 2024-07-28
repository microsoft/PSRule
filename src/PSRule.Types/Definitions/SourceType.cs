// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// The type of source file.
/// </summary>
public enum SourceType
{
    /// <summary>
    /// PowerShell script file.
    /// </summary>
    Script = 1,

    /// <summary>
    /// YAML file.
    /// </summary>
    Yaml = 2,

    /// <summary>
    /// JSON or JSON with comments file.
    /// </summary>
    Json = 3
}
