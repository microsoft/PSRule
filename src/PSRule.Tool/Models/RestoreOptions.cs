// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool.Models;

internal sealed class RestoreOptions
{
    public string[]? Path { get; set; }

    public bool Force { get; set; }
}
