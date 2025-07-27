// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Options;

namespace PSRule.Configuration;

/// <summary>
/// Options that configure property binding.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public interface IBindingOption : IOption
{
    /// <summary>
    /// One or more custom fields to bind.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingfield"/>.
    /// </remarks>
    FieldMap Field { get; }

    /// <summary>
    /// Determines if custom binding uses ignores case when matching properties.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingignorecase"/>.
    /// </remarks>
    bool? IgnoreCase { get; }

    /// <summary>
    /// Configures the separator to use for building a qualified name.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingnameseparator"/>.
    /// </remarks>
    string NameSeparator { get; }

    /// <summary>
    /// Property names to use to bind TargetName.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingtargetname"/>.
    /// </remarks>
    string[] TargetName { get; }

    /// <summary>
    /// Property names to use to bind TargetType.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingtargettype"/>.
    /// </remarks>
    string[] TargetType { get; }

    /// <summary>
    /// Determines if a qualified TargetName is used.
    /// </summary>
    /// <remarks>
    /// See <see href="https://aka.ms/ps-rule/options#bindingusequalifiedname"/>.
    /// </remarks>
    bool? UseQualifiedName { get; }
}
