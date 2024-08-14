// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Information related to suppression of a rule.
/// </summary>
internal interface ISuppressionInfo
{
    ResourceId Id { get; }

    InfoString Synopsis { get; }

    int Count { get; }
}
