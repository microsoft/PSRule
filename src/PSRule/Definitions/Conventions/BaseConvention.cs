// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using PSRule.Runtime;

namespace PSRule.Definitions.Conventions;

[DebuggerDisplay("{Id}")]
internal abstract class BaseConvention
{
    protected BaseConvention(ISourceFile source, string name)
    {
        Source = source;
        Name = name;
        Id = new ResourceId(Source.Module, name, ResourceIdKind.Id);
    }

    public ISourceFile Source { get; }

    public ResourceId Id { get; }

    /// <summary>
    /// The name of the convention.
    /// </summary>
    public string Name { get; }

    public string SourcePath => Source.Path;

    public string Module => Source.Module;

    public virtual void Initialize(LegacyRunspaceContext context, IEnumerable input)
    {

    }

    public virtual void Begin(LegacyRunspaceContext context, IEnumerable input)
    {

    }

    public virtual void Process(LegacyRunspaceContext context, IEnumerable input)
    {

    }

    public virtual void End(LegacyRunspaceContext context, IEnumerable input)
    {

    }
}
