// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;

namespace PSRule.Definitions;

internal sealed class SpecDescriptor<T, TSpec> : ISpecDescriptor where T : Resource<TSpec>, IResource where TSpec : Spec, new()
{
    public SpecDescriptor(string apiVersion, string name)
    {
        ApiVersion = apiVersion;
        Name = name;
        FullName = Spec.GetFullName(apiVersion, name);
    }

    public string Name { get; }

    public string ApiVersion { get; }

    public string FullName { get; }

    public Type SpecType => typeof(TSpec);

    public IResource CreateInstance(ISourceFile source, ResourceMetadata metadata, CommentMetadata comment, ISourceExtent extent, object spec)
    {
        var info = new ResourceHelpInfo(metadata.Name, metadata.DisplayName, new InfoString(comment?.Synopsis), InfoString.Create(metadata.Description));
        return (IResource)Activator.CreateInstance(typeof(T), ApiVersion, source, metadata, info, extent, spec);
    }
}
