// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Data;

/// <summary>
/// A version constraint for a PSRule module.
/// </summary>
/// <param name="module">The name of the module.</param>
/// <param name="constraint">The version constraint of the module.</param>
/// <exception cref="ArgumentNullException">Both <paramref name="module"/> and <paramref name="constraint"/> must not be null or empty.</exception>
[DebuggerDisplay("{Module}")]
public sealed class ModuleConstraint(string module, ISemanticVersionConstraint constraint) : ISemanticVersionConstraint
{
    /// <summary>
    /// The name of the module.
    /// </summary>
    public string Module { get; } = !string.IsNullOrEmpty(module) ? module : throw new ArgumentNullException(nameof(module));

    /// <summary>
    /// The version constraint of the module.
    /// </summary>
    public ISemanticVersionConstraint Constraint { get; } = constraint ?? throw new ArgumentNullException(nameof(constraint));

    /// <inheritdoc/>
    public bool Accepts(SemanticVersion.Version? version) => Constraint.Accepts(version);

    /// <summary>
    /// Get a constraint that accepts any version of the specified module.
    /// </summary>
    /// <param name="module"></param>
    /// <param name="includePrerelease">Determines if pre-releases are accepted or only stable versions.</param>
    /// <returns>A <see cref="ModuleConstraint"/>.</returns>
    public static ModuleConstraint Any(string module, bool includePrerelease = false)
    {
        return new ModuleConstraint
        (
            module,
            includePrerelease ? SemanticVersion.VersionConstraint.Any : SemanticVersion.VersionConstraint.AnyStable
        );
    }
}
