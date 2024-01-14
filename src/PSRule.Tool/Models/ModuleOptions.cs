// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool.Models;

internal sealed class ModuleOptions
{
    public string[]? Path { get; set; }

    public string[]? Module { get; set; }

    public bool Force { get; set; }

    public string? Version { get; set; }

    public bool SkipVerification { get; set; }
}
