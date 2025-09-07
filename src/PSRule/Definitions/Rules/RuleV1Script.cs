// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Rules;

/// <summary>
/// Implement a script block similar to YAML/ JSON resources for later consolidation.
/// </summary>
internal sealed class RuleV1Script(string apiVersion, ISourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, RuleV1ScriptSpec spec)
    : InternalResource<RuleV1ScriptSpec>(ResourceKind.Rule, apiVersion, source, metadata, info, extent, spec), IDisposable, IResource, IRuleV1
{
    private bool _Disposed;

    /// <inheritdoc/>
    [JsonIgnore]
    [YamlIgnore]
    public ResourceId? Ref => ResourceHelper.GetIdNullable(source.Module, metadata.Ref, ResourceIdKind.Ref);

    /// <inheritdoc/>
    [JsonIgnore]
    [YamlIgnore]
    public ResourceId[] Alias => ResourceHelper.GetResourceId(source.Module, metadata.Alias, ResourceIdKind.Alias);

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public SeverityLevel Level => ResourceHelper.GetLevel(spec.Level);

    /// <summary>
    /// A human readable block of text, used to identify the purpose of the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public string Synopsis => Info.Synopsis.Text;

    /// <inheritdoc/>
    ResourceId? IDependencyTarget.Ref => Ref;

    /// <inheritdoc/>
    ResourceId[] IDependencyTarget.Alias => Alias;

    /// <inheritdoc/>
    ResourceId[] IDependencyTarget.DependsOn => spec.DependsOn ?? [];

    /// <inheritdoc/>
    bool IDependencyTarget.Dependency => Source.IsDependency();

    /// <inheritdoc/>
    ResourceId? IResource.Ref => Ref;

    /// <inheritdoc/>
    ResourceId[] IResource.Alias => Alias;

    /// <inheritdoc/>
    IResourceTags IRuleV1.Tag => Metadata.Tags;

    /// <inheritdoc/>
    InfoString IRuleV1.Recommendation => null; // Not supported for PowerShell rules.

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                Spec?.Dispose();
            }

            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
