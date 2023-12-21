// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool;

internal sealed class RestoreOptions
{
    public string[]? Path { get; set; }

    public string? Option { get; set; }

    public bool Verbose { get; set; }

    public bool Debug { get; set; }

    public bool Force { get; set; }
}
