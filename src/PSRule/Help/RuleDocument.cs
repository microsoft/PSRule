// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Help;

internal sealed class RuleDocument : IHelpDocument
{
    public RuleDocument(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public InfoString Synopsis { get; set; }

    public InfoString Description { get; set; }

    public TextBlock Notes { get; set; }

    public InfoString Recommendation { get; set; }

    public Link[] Links { get; set; }

    public ResourceTags Annotations { get; set; }
}
