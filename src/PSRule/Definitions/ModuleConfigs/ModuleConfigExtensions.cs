// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions.ModuleConfigs;

/// <summary>
/// Extensions methods for module configurations.
/// </summary>
internal static class ModuleConfigExtensions
{
    /// <summary>
    /// Convert any selector language blocks into <see cref="ModuleConfigV1"/> resources.
    /// </summary>
    public static ModuleConfigV1[] ToModuleConfigV1(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        // Index configurations by Name.
        var results = new Dictionary<string, ModuleConfigV1>(StringComparer.OrdinalIgnoreCase);
        foreach (var block in blocks.OfType<ModuleConfigV1>().ToArray())
        {
            if (!results.ContainsKey(block.Name))
                results[block.Name] = block;
        }
        return [.. results.Values];
    }
}
