// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// An date version constraint.
/// </summary>
public interface IDateVersionConstraint
{
    /// <summary>
    /// Determines if the date version meets the requirments of the constraint.
    /// </summary>
    bool Accepts(DateVersion.Version? version);
}
