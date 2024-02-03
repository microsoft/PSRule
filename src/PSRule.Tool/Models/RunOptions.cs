// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Rules;

namespace PSRule.Tool.Models;

internal sealed class RunOptions
{
    public string[]? Path { get; set; }

    public string[]? Module { get; set; }

    public string? Baseline { get; set; }

    public RuleOutcome? Outcome { get; set; }

    public string[]? InputPath { get; set; }
}
