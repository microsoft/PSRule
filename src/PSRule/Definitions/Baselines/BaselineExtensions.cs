// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.Baselines;

#nullable enable

/// <summary>
/// Extensions methods for baselines.
/// </summary>
internal static class BaselineExtensions
{
    /// <summary>
    /// Convert any baseline language blocks into <see cref="Baseline"/> resources.
    /// </summary>
    public static Baseline[] ToBaselineV1(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        // Index baselines by BaselineId
        var results = new Dictionary<string, Baseline>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<Baseline>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore baselines that don't match
                if (!Match(context, block))
                    continue;

                if (!results.ContainsKey(block.BaselineId))
                    results[block.BaselineId] = block;

            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return [.. results.Values];
    }

    private static bool Match(LegacyRunspaceContext context, Baseline resource)
    {
        try
        {
            context.EnterLanguageScope(resource.Source);
            var filter = context.LanguageScope!.GetFilter(ResourceKind.Baseline);
            return filter == null || filter.Match(resource);
        }
        finally
        {
            context.ExitLanguageScope(resource.Source);
        }
    }
}

#nullable restore
