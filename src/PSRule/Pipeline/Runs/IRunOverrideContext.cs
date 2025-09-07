// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Definitions.Rules;

namespace PSRule.Pipeline.Runs;

/// <summary>
/// Context for looking up rule overrides for a run.
/// </summary>
internal interface IRunOverrideContext
{
    /// <summary>
    /// Get an override configuration for a rule.
    /// </summary>
    /// <param name="id">The <see cref="ResourceId"/> of the rule.</param>
    /// <param name="propertyOverride">Configured override properties for the specific rule.</param>
    /// <returns>Returns <c>true</c> if the override was found, <c>false</c> otherwise.</returns>
    bool TryGetOverride(ResourceId id, out RuleOverride? propertyOverride);
}
