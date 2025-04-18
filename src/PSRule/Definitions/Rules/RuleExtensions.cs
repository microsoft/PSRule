// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Host;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Definitions.Rules;

#nullable enable

/// <summary>
/// Extensions methods for rules.
/// </summary>
internal static class RuleExtensions
{
    /// <summary>
    /// Convert any rule language blocks to <see cref="RuleBlock"/>.
    /// </summary>
    public static IRuleV1[] ToRuleV1(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        var results = new List<IRuleV1>();

        foreach (var block in blocks.OfType<RuleBlock>())
        {
            results.Add(block);
        }

        // Process from YAML/ JSON
        foreach (var block in blocks.OfType<RuleV1>())
        {
            var ruleName = block.Name;

            context.EnterLanguageScope(block.Source);
            context.LanguageScope!.TryGetOverride(block.Id, out var propertyOverride);
            try
            {
                var info = GetRuleHelpInfo(context, block) ?? new RuleHelpInfo(
                    ruleName,
                    ruleName,
                    block.Source.Module,
                    synopsis: new InfoString(block.Synopsis)
                );
                MergeAnnotations(info, block.Metadata);

                results.Add(new RuleBlock
                    (
                        source: block.Source,
                        id: block.Id,
                        @ref: block.Ref,
                        @default: new RuleProperties
                        {
                            Level = block.Level
                        },
                        @override: propertyOverride,
                        info: info,
                        condition: new RuleVisitor(context, block.Id, block.Source, block.Spec),
                        alias: block.Alias,
                        tag: block.Metadata.Tags,
                        dependsOn: null,  // No support for DependsOn yet
                        configuration: null, // No support for rule configuration use module or workspace config
                        extent: null,
                        flags: block.Flags,
                        labels: block.Metadata.Labels
                    ));
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return [.. results];
    }


    public static RuleHelpInfo[] ToRuleHelp(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        // Index rules by RuleId
        var results = new Dictionary<string, RuleHelpInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<RuleBlock>())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore rule blocks that don't match
                if (!Match(context, block))
                    continue;

                if (!results.ContainsKey(block.Id.Value))
                    results[block.Id.Value] = block.Info;
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }

        }
        return [.. results.Values];
    }

    public static DependencyTargetCollection<RuleBlock> ToRuleDependencyTargetCollection(this IEnumerable<IRuleV1> blocks, LegacyRunspaceContext context, bool skipDuplicateName)
    {
        // Index rules by RuleId
        var results = new DependencyTargetCollection<RuleBlock>();
        if (blocks == null) return results;

        // Keep track of rule names and ids that have been added
        var knownRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownRuleIds = new HashSet<ResourceId>(ResourceIdEqualityComparer.Default);

        // Process from PowerShell
        foreach (var block in blocks.OfType<RuleBlock>())
        {
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId) && duplicateId != null)
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName))
            {
                context.WarnDuplicateRuleName(duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            results.TryAdd(block);
            knownRuleNames.AddNames(block.Id, block.Ref, block.Alias);
            knownRuleIds.AddIds(block.Id, block.Ref, block.Alias);
        }

        // Process from YAML/ JSON
        foreach (var block in blocks.OfType<RuleV1>())
        {
            var ruleName = block.Name;
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId) && duplicateId != null)
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName))
            {
                context.WarnDuplicateRuleName(duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            context.EnterLanguageScope(block.Source);
            context.LanguageScope!.TryGetOverride(block.Id, out var propertyOverride);
            try
            {
                var info = GetRuleHelpInfo(context, block) ?? new RuleHelpInfo(
                    ruleName,
                    ruleName,
                    block.Source.Module,
                    synopsis: new InfoString(block.Synopsis)
                );
                MergeAnnotations(info, block.Metadata);

                results.TryAdd(new RuleBlock
                (
                    source: block.Source,
                    id: block.Id,
                    @ref: block.Ref,
                    @default: new RuleProperties
                    {
                        Level = block.Level
                    },
                    @override: propertyOverride,
                    info: info,
                    condition: new RuleVisitor(context, block.Id, block.Source, block.Spec),
                    alias: block.Alias,
                    tag: block.Metadata.Tags,
                    dependsOn: null,  // No support for DependsOn yet
                    configuration: null, // No support for rule configuration use module or workspace config
                    extent: block.Extent,
                    flags: block.Flags,
                    labels: block.Metadata.Labels
                ));
                knownRuleNames.AddNames(block.Id, block.Ref, block.Alias);
                knownRuleIds.AddIds(block.Id, block.Ref, block.Alias);
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return results;
    }

    private static void MergeAnnotations(RuleHelpInfo info, ResourceMetadata metadata)
    {
        if (info == null || metadata == null || metadata.Annotations == null || metadata.Annotations.Count == 0)
            return;

        info.Annotations ??= new Hashtable(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in metadata.Annotations)
        {
            if (!info.Annotations.ContainsKey(kv.Key))
                info.Annotations[kv.Key] = kv.Value;
        }
        if (!info.HasOnlineHelp())
            info.SetOnlineHelpUrl(metadata.Link);
    }

    private static bool Match(LegacyRunspaceContext context, RuleBlock resource)
    {
        try
        {
            context.EnterLanguageScope(resource.Source);
            var filter = context.LanguageScope!.GetFilter(ResourceKind.Rule);
            return filter == null || filter.Match(resource);
        }
        finally
        {
            context.ExitLanguageScope(resource.Source);
        }
    }

    private static RuleHelpInfo GetRuleHelpInfo(LegacyRunspaceContext context, IRuleV1 rule)
    {
        return HostHelper.GetRuleHelpInfo(context, rule.Name, rule.Synopsis, rule.Info.DisplayName, rule.Info.Description, rule.Recommendation);
    }
}

#nullable restore
