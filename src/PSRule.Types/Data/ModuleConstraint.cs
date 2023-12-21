// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// A version constraint for a PSRule module.
/// </summary>
public sealed class ModuleConstraint
{
    /// <summary>
    /// Create a version constraint for a PSRule module.
    /// </summary>
    /// <param name="module">The name of the module.</param>
    /// <param name="constraint">The version constraint of the module.</param>
    public ModuleConstraint(string module, ISemanticVersionConstraint constraint)
    {
        Module = module;
        Constraint = constraint;
    }

    /// <summary>
    /// The name of the module.
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// The version constraint of the module.
    /// </summary>
    public ISemanticVersionConstraint Constraint { get; }
}
