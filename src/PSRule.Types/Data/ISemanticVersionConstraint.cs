// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A semantic version constraint.
/// </summary>
public interface ISemanticVersionConstraint
{
    /// <summary>
    /// Determines if the semantic version meets the requirments of the constraint.
    /// </summary>
    bool Accepts(SemanticVersion.Version? version);
}
