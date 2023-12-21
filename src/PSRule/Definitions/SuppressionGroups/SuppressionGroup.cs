// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Pipeline;

namespace PSRule.Definitions.SuppressionGroups;

/// <summary>
/// A specification for a V1 suppression group resource.
/// </summary>
internal interface ISuppressionGroupV1Spec
{
    /// <summary>
    /// The date time that the suppression is valid until.
    /// After this date time, the suppression is ignored.
    /// When not set, the suppression does not expire.
    /// This RFC3339 (ISO 8601) formatted date time using the format yyyy-MM-ddTHH:mm:ssZ.
    /// </summary>
    DateTime? ExpiresOn { get; set; }

    /// <summary>
    /// This only applies to rules that match the specified rule names.
    /// </summary>
    string[] Rule { get; }

    /// <summary>
    /// An expression. If the expression evaluates as true and rules specified by <see cref="Rule"/> are suppressed.
    /// </summary>
    LanguageIf If { get; }
}

/// <summary>
/// A suppression group resource V1.
/// </summary>
[Spec(Specs.V1, Specs.SuppressionGroup)]
internal sealed class SuppressionGroupV1 : InternalResource<SuppressionGroupV1Spec>
{
    public SuppressionGroupV1(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, SuppressionGroupV1Spec spec)
        : base(ResourceKind.SuppressionGroup, apiVersion, source, metadata, info, extent, spec) { }
}

/// <summary>
/// A specification for a V1 suppression group resource.
/// </summary>
internal sealed class SuppressionGroupV1Spec : Spec, ISuppressionGroupV1Spec
{
    /// <inheritdoc/>
    public DateTime? ExpiresOn { get; set; }

    /// <inheritdoc/>
    public string[] Rule { get; set; }

    /// <inheritdoc/>
    public LanguageIf If { get; set; }
}
