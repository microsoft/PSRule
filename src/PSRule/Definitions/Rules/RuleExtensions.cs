// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Help;
using PSRule.Pipeline.Runs;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Definitions.Rules;

/// <summary>
/// Extensions methods for rules.
/// </summary>
internal static class RuleExtensions
{
    private const string Markdown_Extension = ".md";

    /// <summary>
    /// Convert any rule language blocks to <see cref="RuleBlock"/>.
    /// </summary>
    public static IRuleV1[] ToRuleV1(this IEnumerable<ILanguageBlock> blocks, LegacyRunspaceContext context)
    {
        if (blocks == null) return [];

        var results = new List<IRuleV1>();

        foreach (var block in blocks.OfType<RuleV1Script>())
        {
            var ruleName = block.Name;

            context.EnterLanguageScope(block.Source);
            context.TryGetOverride(block.Id, out var propertyOverride);
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
                        condition: block.Spec.Condition,
                        alias: block.Alias,
                        tag: block.Metadata.Tags,
                        dependsOn: block.Spec.DependsOn,
                        configuration: block.Spec.Configure,
                        extent: block.Extent,
                        flags: block.Flags,
                        labels: block.Metadata.Labels
                    ));
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }

        // Process from YAML/ JSON
        foreach (var block in blocks.OfType<RuleV1>())
        {
            var ruleName = block.Name;

            context.EnterLanguageScope(block.Source);
            context.TryGetOverride(block.Id, out var propertyOverride);
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
                        condition: new RuleVisitor(block.Id, block.Source, block.Spec),
                        alias: block.Alias,
                        tag: block.Metadata.Tags,
                        dependsOn: null,  // No support for DependsOn yet
                        configuration: null, // No support for rule configuration use module or workspace config
                        extent: block.Extent,
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

        foreach (var block in blocks.OfType<IRuleV1>())
        {
            context.EnterLanguageScope(block.Source);

            var filter = context.Scope!.GetFilter(ResourceKind.Rule);

            try
            {
                // Ignore rule blocks that don't match
                if (filter != null && !filter.Match(block))
                    continue;

                var id = ((IResource)block).Id;
                if (!results.ContainsKey(id.Value))
                {
                    results[id.Value] = GetRuleHelpInfo(context, block) ?? new RuleHelpInfo(
                        block.Name,
                        block.Name,
                        block.Source.Module,
                        synopsis: new InfoString(block.Synopsis)
                    );
                }
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }

        }
        return [.. results.Values];
    }

    public static DependencyTargetCollection<RuleBlock> ToRuleDependencyTargetCollection(this IEnumerable<IRuleV1> blocks, IRunBuilderContext context, bool skipDuplicateName)
    {
        // Index rules by RuleId
        var results = new DependencyTargetCollection<RuleBlock>();
        if (blocks == null) return results;

        // Keep track of rule names and ids that have been added
        var knownRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownRuleIds = new HashSet<ResourceId>(ResourceIdEqualityComparer.Default);

        // Process from PowerShell
        foreach (var block in blocks.OfType<RuleV1Script>())
        {
            var ruleName = block.Name;
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId) && duplicateId != null)
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName) && duplicateName != null)
            {
                context.DuplicateResourceName(block.Id, duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            context.EnterLanguageScope(block.Source);
            context.TryGetOverride(block.Id, out var propertyOverride);
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
                    condition: block.Spec.Condition,
                    alias: block.Alias,
                    tag: block.Metadata.Tags,
                    dependsOn: block.Spec.DependsOn,
                    configuration: block.Spec.Configure,
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

        // Process from YAML/ JSON
        foreach (var block in blocks.OfType<RuleV1>())
        {
            var ruleName = block.Name;
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId) && duplicateId != null)
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName) && duplicateName != null)
            {
                context.DuplicateResourceName(block.Id, duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            context.EnterLanguageScope(block.Source);
            context.TryGetOverride(block.Id, out var propertyOverride);
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
                    condition: new RuleVisitor(block.Id, block.Source, block.Spec),
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

    public static DependencyTargetCollection<IRuleBlock> ToRuleDependencyTargetCollectionV2(this IEnumerable<IRuleV1> blocks, IRunBuilderContext context, bool skipDuplicateName)
    {
        // Index rules by RuleId
        var results = new DependencyTargetCollection<IRuleBlock>();
        if (blocks == null) return results;

        // Keep track of rule names and ids that have been added
        var knownRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownRuleIds = new HashSet<ResourceId>(ResourceIdEqualityComparer.Default);

        // Process from PowerShell
        foreach (var block in blocks.OfType<RuleV1Script>())
        {
            var ruleName = block.Name;
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId) && duplicateId != null)
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName) && duplicateName != null)
            {
                context.DuplicateResourceName(block.Id, duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            context.EnterLanguageScope(block.Source);
            context.TryGetOverride(block.Id, out var propertyOverride);
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
                    condition: block.Spec.Condition,
                    alias: block.Alias,
                    tag: block.Metadata.Tags,
                    dependsOn: block.Spec.DependsOn,
                    configuration: block.Spec.Configure,
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

        // Process from YAML/ JSON
        foreach (var block in blocks.OfType<RuleV1>())
        {
            var ruleName = block.Name;
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId) && duplicateId != null)
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName) && duplicateName != null)
            {
                context.DuplicateResourceName(block.Id, duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            context.EnterLanguageScope(block.Source);
            context.TryGetOverride(block.Id, out var propertyOverride);
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
                    condition: new RuleVisitor(block.Id, block.Source, block.Spec),
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

    private static RuleHelpInfo GetRuleHelpInfo(IGetLocalizedPathContext context, IRuleV1 rule)
    {


        return GetRuleHelpInfo(context, rule, rule.Name, rule.Synopsis, rule.Info.DisplayName, rule.Info.Description, rule.Recommendation);
    }

    private static RuleHelpInfo GetRuleHelpInfo(IGetLocalizedPathContext context, IRuleV1 rule, string name, string defaultSynopsis, string defaultDisplayName, InfoString defaultDescription, InfoString defaultRecommendation)
    {
        return !TryHelpPath(context, rule, name, out var path, out var culture) || !TryDocument(path, culture, out var document) || document == null
            ? new RuleHelpInfo(
                name: name,
                displayName: defaultDisplayName ?? name,
                moduleName: context.Source!.Module,
                synopsis: InfoString.Create(defaultSynopsis),
                description: defaultDescription,
                recommendation: defaultRecommendation
            )
            : new RuleHelpInfo(
                name: name,
                displayName: document.Name ?? defaultDisplayName ?? name,
                moduleName: context.Source!.Module,
                synopsis: document.Synopsis ?? new InfoString(defaultSynopsis),
                description: document.Description ?? defaultDescription,
                recommendation: document.Recommendation ?? defaultRecommendation ?? document.Synopsis ?? InfoString.Create(defaultSynopsis)
            )
            {
                Notes = document.Notes?.Text,
                Links = GetLinks(document.Links),
                Annotations = document.Annotations?.ToHashtable()
            };
    }

    private static bool TryHelpPath(IGetLocalizedPathContext context, IResource resource, string name, out string? path, out string? culture)
    {
        path = null;
        culture = null;
        if (string.IsNullOrEmpty(resource.Source.HelpPath))
            return false;

        var helpFileName = string.Concat(name, Markdown_Extension);
        path = context?.GetLocalizedPath(helpFileName, out culture);
        return path != null;
    }

    private static bool TryDocument(string? path, string? culture, out RuleDocument? document)
    {
        document = null;
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(culture))
            return false;

        var markdown = File.ReadAllText(path);
        if (string.IsNullOrEmpty(markdown))
            return false;

        var reader = new MarkdownReader(yamlHeaderOnly: false);
        var stream = reader.Read(markdown, path);
        var lexer = new RuleHelpLexer(culture);
        document = lexer.Process(stream);
        return document != null;
    }

    private static PSRule.Rules.Link[]? GetLinks(Help.Link[] links)
    {
        if (links == null || links.Length == 0)
            return null;

        var result = new PSRule.Rules.Link[links.Length];
        for (var i = 0; i < links.Length; i++)
            result[i] = new PSRule.Rules.Link(links[i].Name, links[i].Uri);

        return result;
    }
}
