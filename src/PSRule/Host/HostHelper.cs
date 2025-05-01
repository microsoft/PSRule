// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text;
using Newtonsoft.Json;
using PSRule.Annotations;
using PSRule.Converters.Json;
using PSRule.Converters.Yaml;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Rules;
using PSRule.Help;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PSRule.Host;

#nullable enable

internal static class HostHelper
{
    private const string Markdown_Extension = ".md";

    internal static IRuleV1[] GetRule(LegacyRunspaceContext context, bool includeDependencies)
    {
        var rules = context.Pipeline.ResourceCache.OfType<IRuleV1>();
        var blocks = rules.ToRuleDependencyTargetCollection(context, skipDuplicateName: false);

        var builder = new DependencyGraphBuilder<RuleBlock>(context, includeDependencies, includeDisabled: true);
        builder.Include(blocks, filter: (b) => Match(context, b));
        return builder.GetItems();
    }

    internal static RuleHelpInfo[] GetRuleHelp(LegacyRunspaceContext context)
    {
        var rules = context.Pipeline.ResourceCache.OfType<IRuleV1>();
        var blocks = rules.ToRuleDependencyTargetCollection(context, skipDuplicateName: true);

        return blocks.GetAll().ToRuleHelp(context);
    }

    internal static DependencyGraph<RuleBlock> GetRuleBlockGraph(LegacyRunspaceContext context)
    {
        var rules = context.Pipeline.ResourceCache.OfType<IRuleV1>();
        var blocks = rules.ToRuleDependencyTargetCollection(context, skipDuplicateName: false);

        var builder = new DependencyGraphBuilder<RuleBlock>(context, includeDependencies: true, includeDisabled: false);
        builder.Include(blocks, filter: (b) => Match(context, b));
        return builder.Build();
    }

    /// <summary>
    /// Get meta resources which are resource defined in YAML or JSON.
    /// </summary>
    internal static IEnumerable<T> GetMetaResources<T>(Source[] source, IResourceDiscoveryContext context) where T : ILanguageBlock
    {
        if (source == null || source.Length == 0) return [];

        var results = new List<T>();
        results.AddRange(GetYamlLanguageBlocks(source, context).OfType<T>());
        results.AddRange(GetJsonLanguageBlocks(source, context).OfType<T>());
        return results;
    }

    /// <summary>
    /// Get PS resources which are resource defined in PowerShell.
    /// </summary>
    internal static IEnumerable<T> GetPSResources<T>(Source[] source, LegacyRunspaceContext context) where T : ILanguageBlock
    {
        if (source == null || source.Length == 0) return [];

        var results = new List<T>();
        results.AddRange(GetPSLanguageBlocks(context, source).OfType<T>());
        return results;
    }

    /// <summary>
    /// Read YAML/JSON objects and return baselines.
    /// </summary>
    internal static IEnumerable<Baseline> GetBaseline(Source[] source, LegacyRunspaceContext context)
    {
        return GetMetaResources<ILanguageBlock>(source, context).ToBaselineV1(context);
    }

    /// <summary>
    /// Called from PowerShell to get additional metadata from a language block, such as comment help.
    /// </summary>
    internal static CommentMetadata GetCommentMeta(ISourceFile file, int lineNumber, int offset)
    {
        if (lineNumber < 0 || !file.Exists())
            return new CommentMetadata();

        var lines = File.ReadAllLines(file.Path, Encoding.UTF8); ;
        var i = lineNumber;
        var comments = new List<string>();

        // Back track lines with comments immediately before block
        for (; i >= 0 && lines[i].Contains("#"); i--)
            comments.Insert(0, lines[i]);

        // Check if any comments were found
        var metadata = new CommentMetadata();
        if (comments.Count > 0)
        {
            foreach (var comment in comments)
            {
                if (comment.StartsWith("# Description: ", StringComparison.OrdinalIgnoreCase))
                    metadata.Synopsis = comment.Substring(15);

                if (comment.StartsWith("# Synopsis: ", StringComparison.OrdinalIgnoreCase))
                    metadata.Synopsis = comment.Substring(12);
            }
        }
        return metadata;
    }

    internal static void UnblockFile(IPipelineWriter writer, string[] publisher, string[] path)
    {
        var ps = PowerShell.Create();
        try
        {
            ps.Runspace.SessionStateProxy.SetVariable("trustedPublisher", publisher);
            ps.Runspace.SessionStateProxy.SetVariable("trustedPath", path);
            ps.AddScript("$trustedPath | ForEach-Object { Get-AuthenticodeSignature -FilePath $_ } | Where-Object { $_.Status -eq 'Valid' -and $_.SignerCertificate.Subject -in $trustedPublisher } | ForEach-Object { Unblock-File -Path $_.Path -Confirm:$False; }");
            ps.Invoke();
            if (ps.HadErrors)
            {
                foreach (var error in ps.Streams.Error)
                    writer.WriteError(error);
            }
        }
        finally
        {
            ps.Runspace = null;
            ps.Dispose();
        }
    }

    /// <summary>
    /// Execute PowerShell script files to get language blocks.
    /// </summary>
    private static ILanguageBlock[] GetPSLanguageBlocks(IScriptResourceDiscoveryContext context, Source[] sources)
    {
        if (context.GetExecutionOption().RestrictScriptSource == Options.RestrictScriptSource.DisablePowerShell)
            return [];

        var results = new List<ILanguageBlock>();
        var ps = context.GetPowerShell();

        try
        {
            context.Writer?.EnterScope("[Discovery.Rule]");
            context.PushScope(RunspaceScope.Source);

            // Process scripts
            foreach (var source in sources)
            {
                foreach (var file in source.File)
                {
                    if (file.Type != SourceType.Script)
                        continue;

                    ps.Commands.Clear();
                    context.Writer?.VerboseRuleDiscovery(path: file.Path);
                    context.EnterLanguageScope(file);
                    try
                    {
                        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(file.Path, out var tokens, out var errors);
                        var visitor = new RuleLanguageAst();
                        scriptAst.Visit(visitor);

                        if (visitor.Errors != null && visitor.Errors.Count > 0)
                        {
                            foreach (var record in visitor.Errors)
                                context.Writer?.WriteError(record);

                            continue;
                        }
                        if (errors != null && errors.Length > 0)
                        {
                            foreach (var error in errors)
                                context.Writer?.WriteError(error);

                            continue;
                        }

                        // Invoke script
                        ps.AddScript(string.Concat("& '", file.Path.EscapeSingleQuote(), "'"), true);
                        var invokeResults = ps.Invoke();

                        // Discovery has errors so skip this file
                        if (ps.HadErrors)
                            continue;

                        foreach (var ir in invokeResults)
                        {
                            if (ir.BaseObject is ILanguageBlock block)
                                results.Add(block);
                        }
                    }
                    finally
                    {
                        context.ExitLanguageScope(file);
                    }
                }
            }
        }
        finally
        {
            context.Writer?.ExitScope();
            context.PopScope(RunspaceScope.Source);
            ps.Runspace = null;
            ps.Dispose();
        }
        return [.. results];
    }

    private static string EscapeSingleQuote(this string input)
        => input.Replace(@"'", @"''");

    /// <summary>
    /// Get language blocks from YAML source files.
    /// </summary>
    private static ILanguageBlock[] GetYamlLanguageBlocks(Source[] sources, IResourceDiscoveryContext context)
    {
        var result = new Collection<ILanguageBlock>();
        var visitor = new ResourceValidator(context.Writer);
        var d = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeMapping<IResourceAnnotations, ResourceAnnotations>()
            .WithTypeMapping<IResourceTags, ResourceTags>()
            .WithTypeMapping<IResourceLabels, ResourceLabels>()
            .WithTypeConverter(new FieldMapYamlTypeConverter())
            .WithTypeConverter(new StringArrayMapConverter())
            .WithTypeConverter(new StringArrayConverter())
            .WithTypeConverter(new PSObjectYamlTypeConverter())
            .WithTypeConverter(new EnumMapYamlTypeConverter<SeverityLevel>())
            .WithTypeConverter(new CapabilityOptionYamlConverter())
            .WithNodeTypeResolver(new PSOptionYamlTypeResolver())
            .WithNodeDeserializer(
                inner => new ResourceNodeDeserializer(context, new LanguageExpressionDeserializer(context, inner)),
                s => s.InsteadOf<ObjectNodeDeserializer>())
            .Build();

        try
        {
            context.Writer?.EnterScope("[Discovery.Resource]");
            context.PushScope(RunspaceScope.Resource);
            foreach (var source in sources)
            {
                foreach (var file in source.File)
                {
                    if (file.Type != SourceType.Yaml)
                        continue;

                    context.Writer?.VerboseRuleDiscovery(path: file.Path);
                    context.EnterLanguageScope(file);
                    try
                    {
                        using var reader = new StreamReader(file.Path);
                        var parser = new Parser(reader);
                        parser.TryConsume<StreamStart>(out _);
                        while (parser.Current is DocumentStart)
                        {
                            var item = d.Deserialize<ResourceObject>(parser);
                            if (item?.Block != null && item.Visit(visitor))
                            {
                                result.Add(item.Block);
                            }
                            else if (item != null && item.Block == null)
                            {
                                context.Writer?.LogUnknownResourceKind(item.Kind, item.ApiVersion, file);
                            }
                        }
                    }
                    finally
                    {
                        context.ExitLanguageScope(file);
                    }
                }
            }
        }
        finally
        {
            context.Writer?.ExitScope();
            context.PopScope(RunspaceScope.Resource);
        }
        return result.Count == 0 ? [] : [.. result];
    }

    /// <summary>
    /// Get language blocks from JSON source files.
    /// </summary>
    private static ILanguageBlock[] GetJsonLanguageBlocks(Source[] sources, IResourceDiscoveryContext context)
    {
        var result = new Collection<ILanguageBlock>();
        var visitor = new ResourceValidator(context.Writer);
        var deserializer = new JsonSerializer
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        deserializer.Converters.Add(new ResourceObjectJsonConverter(context));
        deserializer.Converters.Add(new FieldMapJsonConverter());
        deserializer.Converters.Add(new StringArrayJsonConverter());
        deserializer.Converters.Add(new LanguageExpressionJsonConverter(context));
        deserializer.Converters.Add(new EnumMapJsonConverter<SeverityLevel>());
        deserializer.Converters.Add(new CapabilityOptionJsonConverter());

        try
        {
            context.Writer?.EnterScope("[Discovery.Resource]");
            context.PushScope(RunspaceScope.Resource);

            foreach (var source in sources)
            {
                foreach (var file in source.File)
                {
                    if (file.Type != SourceType.Json)
                        continue;

                    context.Writer?.VerboseRuleDiscovery(file.Path);
                    context.EnterLanguageScope(file);
                    try
                    {
                        using var reader = new JsonTextReader(new StreamReader(file.Path));

                        // Consume lines until start of array
                        reader.SkipComments(out _);
                        if (reader.TryConsume(JsonToken.StartArray))
                        {
                            reader.SkipComments(out _);
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                var item = deserializer.Deserialize<ResourceObject>(reader);
                                if (item?.Block != null && item.Visit(visitor))
                                {
                                    result.Add(item.Block);
                                }
                                else if (item != null && item.Block == null)
                                {
                                    context.Writer?.LogUnknownResourceKind(item.Kind, item.ApiVersion, file);
                                }

                                // Consume all end objects at the end of each resource
                                while (reader.TryConsume(JsonToken.EndObject)) { }
                            }
                        }
                    }
                    finally
                    {
                        context.ExitLanguageScope(file);
                    }
                }
            }
        }
        finally
        {
            context.Writer?.ExitScope();
            context.PopScope(RunspaceScope.Resource);
        }
        return result.Count == 0 ? [] : [.. result];
    }

    public static void InvokeRuleBlock(LegacyRunspaceContext context, RuleBlock ruleBlock, RuleRecord ruleRecord)
    {
        LegacyRunspaceContext.CurrentThread = context;
        var condition = ruleBlock.Condition;
        context.LogObjectStart();

        try
        {
            context.EnterLanguageScope(ruleBlock.Source);
            var invokeResult = condition.If();
            if (invokeResult == null)
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.PreconditionFail;
                return;
            }
            else if (invokeResult.HadErrors || context.HadErrors)
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.None;
                ruleRecord.Outcome = RuleOutcome.Error;
            }
            else if (invokeResult.Count == 0)
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.Inconclusive;
                ruleRecord.Outcome = RuleOutcome.Fail;
                context.WarnRuleInconclusive(ruleRecord.RuleId);
            }
            else
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.Processed;
                ruleRecord.Outcome = invokeResult.AllOf() ? RuleOutcome.Pass : RuleOutcome.Fail;
            }
            context.VerboseConditionResult(pass: invokeResult.Pass, count: invokeResult.Count, outcome: ruleRecord.Outcome);
        }
        catch (CmdletInvocationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            context.Error(ex);
        }
        // TODO: Exit scope
        //finally
        //{
        //    context.ExitSourceScope();
        //}
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

    internal static void UpdateHelpInfo(IGetLocalizedPathContext context, IResource resource)
    {
        if (context == null || resource == null || !TryHelpPath(context, resource.Name, out var path, out var culture) || !TryHelpInfo(path, culture, out var info))
            return;

        resource.Info.Update(info);
    }

    internal static bool TryHelpPath(IGetLocalizedPathContext context, string name, out string? path, out string? culture)
    {
        path = null;
        culture = null;
        if (string.IsNullOrEmpty(context?.Source?.HelpPath))
            return false;

        var helpFileName = string.Concat(name, Markdown_Extension);
        path = context?.GetLocalizedPath(helpFileName, out culture);
        return path != null;
    }

    private static bool TryHelpInfo(string? path, string? culture, out IResourceHelpInfo? info)
    {
        info = null;
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(culture))
            return false;

        var markdown = File.ReadAllText(path);
        if (string.IsNullOrEmpty(markdown))
            return false;

        var reader = new MarkdownReader(yamlHeaderOnly: false);
        var stream = reader.Read(markdown, path);
        var lexer = new ResourceHelpLexer(culture);
        info = lexer.Process(stream).ToInfo();
        return info != null;
    }

    internal static RuleHelpInfo GetRuleHelpInfo(LegacyRunspaceContext context, string name, string defaultSynopsis, string defaultDisplayName, InfoString defaultDescription, InfoString defaultRecommendation)
    {
        return !TryHelpPath(context, name, out var path, out var culture) || !TryDocument(path, culture, out var document)
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
                displayName: document!.Name ?? defaultDisplayName ?? name,
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

    private static Rules.Link[]? GetLinks(Help.Link[] links)
    {
        if (links == null || links.Length == 0)
            return null;

        var result = new Rules.Link[links.Length];
        for (var i = 0; i < links.Length; i++)
            result[i] = new Rules.Link(links[i].Name, links[i].Uri);

        return result;
    }
}

#nullable restore
