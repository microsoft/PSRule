// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Help;

internal sealed class ResourceHelpDocument : IHelpDocument
{
    public ResourceHelpDocument(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public InfoString Synopsis { get; set; }

    public InfoString Description { get; set; }

    public Link[] Links { get; set; }

    internal IResourceHelpInfo ToInfo()
    {
        return new ResourceHelpInfo(Name, Name, Synopsis, Description);
    }
}
