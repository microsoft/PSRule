// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.Conventions;

#nullable enable

/// <summary>
/// Extensions for conventions.
/// </summary>
internal static class ConventionExtensions
{
    /// <summary>
    /// Convert any convention language blocks into <see cref="IConventionV1"/> resources.
    /// </summary>
    public static IConventionV1[] ToConventionsV1(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        // Index by Id.
        var index = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var results = new List<IConventionV1>();

        foreach (var block in blocks.OfType<ScriptBlockConvention>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore blocks that don't match.
                if (!Match(context, block))
                    continue;

                if (!index.Contains(block.Id.Value))
                    results.Add(block);

            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return [.. results];
    }

    private static bool Match(LegacyRunspaceContext context, ScriptBlockConvention block)
    {
        try
        {
            context.EnterLanguageScope(block.Source);
            var filter = context.LanguageScope?.GetFilter(ResourceKind.Convention);
            return filter == null || filter.Match(block);
        }
        finally
        {
            context.ExitLanguageScope(block.Source);
        }
    }
}

#nullable restore
