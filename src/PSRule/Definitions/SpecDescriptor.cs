// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;

namespace PSRule.Definitions;

internal sealed class SpecDescriptor<T, TSpec>(string apiVersion, string name) : ISpecDescriptor where T : Resource<TSpec>, IResource where TSpec : Spec, new()
{
    public string Name { get; } = name;

    public string ApiVersion { get; } = apiVersion;

    public string FullName { get; } = Spec.GetFullName(apiVersion, name);

    public Type SpecType => typeof(TSpec);

    public IResource CreateInstance(ISourceFile source, ResourceMetadata metadata, CommentMetadata comment, ISourceExtent extent, object spec)
    {
        var info = new ResourceHelpInfo(metadata.Name, metadata.DisplayName, new InfoString(comment?.Synopsis), InfoString.Create(metadata.Description));
        return (IResource)Activator.CreateInstance(typeof(T), ApiVersion, source, metadata, info, extent, spec);
    }
}
