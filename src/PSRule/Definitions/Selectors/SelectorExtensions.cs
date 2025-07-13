// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.Selectors;

#nullable enable

/// <summary>
/// Extensions methods for selectors.
/// </summary>
internal static class SelectorExtensions
{
    /// <summary>
    /// Convert a selector into a selector visitor.
    /// </summary>
    /// <param name="resource">The selector resource.</param>
    /// <param name="runspaceContext">A valid runspace context.</param>
    /// <returns>An instance of a <see cref="SelectorVisitor"/>.</returns>
    public static SelectorVisitor ToSelectorVisitor(this ISelector resource, LegacyRunspaceContext runspaceContext)
    {
        return new SelectorVisitor(
            runspaceContext,
            resource.ApiVersion,
            resource.Id,
            resource.Source,
            resource.Spec
        );
    }

    /// <summary>
    /// Convert any selector language blocks into <see cref="SelectorV1"/> resources.
    /// </summary>
    public static SelectorV1[] ToSelectorV1(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        // Index selectors by Id.
        var results = new Dictionary<string, SelectorV1>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<SelectorV1>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore selectors that don't match.
                if (!Match(context, block))
                    continue;

                if (!results.ContainsKey(block.Id.Value))
                    results[block.Id.Value] = block;
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return [.. results.Values];
    }

    private static bool Match(LegacyRunspaceContext context, SelectorV1 resource)
    {
        try
        {
            context.EnterLanguageScope(resource.Source);
            var filter = context.Scope!.GetFilter(ResourceKind.Selector);
            return filter == null || filter.Match(resource);
        }
        finally
        {
            context.ExitLanguageScope(resource.Source);
        }
    }
}

#nullable restore
