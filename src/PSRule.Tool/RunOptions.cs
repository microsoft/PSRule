// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;

namespace PSRule.Tool;

internal sealed class RunOptions
{
    public string[]? Path { get; set; }

    public string[]? Module { get; set; }

    public string? Option { get; set; }

    public string? Baseline { get; set; }

    public RuleOutcome? Outcome { get; set; }

    public string[]? InputPath { get; set; }

    public bool Verbose { get; set; }

    public bool Debug { get; set; }
}
