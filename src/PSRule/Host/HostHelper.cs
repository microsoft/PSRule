// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Newtonsoft.Json;
using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Conventions;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Definitions.Selectors;
using PSRule.Parser;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using Rule = PSRule.Rules.Rule;

namespace PSRule.Host
{
    internal static class HostHelper
    {
        private const string Markdown_Extension = ".md";

        internal static Rule[] GetRule(Source[] source, RunspaceContext context, bool includeDependencies)
        {
            var rules = ToRuleV1(GetLanguageBlock(context, source), context);
            var builder = new DependencyGraphBuilder<Rule>(includeDependencies);
            builder.Include(rules, filter: (b) => Match(context, b));
            return builder.GetItems();
        }

        internal static RuleHelpInfo[] GetRuleHelp(Source[] source, RunspaceContext context)
        {
            return ToRuleHelp(ToRuleBlockV1(GetLanguageBlock(context, source), context), context);
        }

        internal static DependencyGraph<RuleBlock> GetRuleBlockGraph(Source[] source, RunspaceContext context)
        {
            var blocks = GetLanguageBlock(context, source);
            var rules = ToRuleBlockV1(blocks, context);
            Import(GetConventions(blocks, context), context);
            var builder = new DependencyGraphBuilder<RuleBlock>(includeDependencies: true);
            builder.Include(rules, filter: (b) => Match(context, b));
            return builder.Build();
        }

        /// <summary>
        /// Read YAML objects and return rules.
        /// </summary>
        internal static IEnumerable<Rule> GetRuleYaml(Source[] source, RunspaceContext context)
        {
            return ToRuleV1(ReadYamlObjects(source, context), context);
        }

        internal static IEnumerable<RuleBlock> GetRuleYamlBlocks(Source[] source, RunspaceContext context)
        {
            return ToRuleBlockV1(ReadYamlObjects(source, context), context);
        }

        /// <summary>
        /// Read YAML objects and return baselines.
        /// </summary>
        internal static IEnumerable<Baseline> GetBaselineYaml(Source[] source, RunspaceContext context)
        {
            var results = new List<ILanguageBlock>();

            results.AddRange(ReadYamlObjects(source, context));
            results.AddRange(ReadJsonObjects(source, context));

            return ToBaselineV1(results, context);
        }

        /// <summary>
        /// Read YAML objects and return module configurations.
        /// </summary>
        internal static IEnumerable<ModuleConfigV1> GetModuleConfigYaml(Source[] source, RunspaceContext context)
        {
            return ToModuleConfigV1(ReadYamlObjects(source, context), context);
        }

        /// <summary>
        /// Read YAML objects and return selectors.
        /// </summary>
        internal static IEnumerable<SelectorV1> GetSelectorYaml(Source[] source, RunspaceContext context)
        {
            return ToSelectorV1(ReadYamlObjects(source, context), context);
        }

        internal static void ImportResource(Source[] source, RunspaceContext context)
        {
            if (source == null || source.Length == 0)
                return;

            var results = new List<ILanguageBlock>();

            results.AddRange(ReadYamlObjects(source, context));
            results.AddRange(ReadJsonObjects(source, context));

            Import(results.ToArray(), context);
        }

        /// <summary>
        /// Called from PowerShell to get additional metdata from a language block, such as comment help.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        internal static CommentMetadata GetCommentMeta(string path, int lineNumber, int offset)
        {
            var context = RunspaceContext.CurrentThread;
            if (lineNumber < 0 || RunspaceContext.CurrentThread.IsScope(RunspaceScope.None) || context.Source.SourceContentCache == null)
                return new CommentMetadata();

            var lines = context.Source.SourceContentCache;
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

        private static ILanguageBlock[] GetLanguageBlock(RunspaceContext context, Source[] sources)
        {
            var results = new List<ILanguageBlock>();
            results.AddRange(GetPowerShellRule(context, sources));
            results.AddRange(ReadYamlObjects(sources, context));
            results.AddRange(ReadJsonObjects(sources, context));
            return results.ToArray();
        }

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private static ILanguageBlock[] GetPowerShellRule(RunspaceContext context, Source[] sources)
        {
            var results = new Collection<ILanguageBlock>();
            var ps = context.GetPowerShell();

            try
            {
                context.Writer.EnterScope("[Discovery.Rule]");
                context.PushScope(RunspaceScope.Source);

                // Process scripts
                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        if (file.Type != SourceType.Script)
                            continue;

                        ps.Commands.Clear();
                        context.VerboseRuleDiscovery(path: file.Path);
                        context.EnterSourceScope(source: file);

                        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(file.Path, out Token[] tokens, out ParseError[] errors);
                        var visitor = new RuleLanguageAst(PipelineContext.CurrentThread);
                        scriptAst.Visit(visitor);

                        if (visitor.Errors != null && visitor.Errors.Count > 0)
                        {
                            foreach (var record in visitor.Errors)
                                context.WriteError(record);

                            continue;
                        }
                        if (errors != null && errors.Length > 0)
                        {
                            foreach (var error in errors)
                                context.WriteError(error);

                            continue;
                        }

                        // Invoke script
                        ps.AddScript(string.Concat("& '", file.Path, "'"), true);
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
                }
            }
            finally
            {
                context.Writer.ExitScope();
                context.PopScope(RunspaceScope.Source);
                context.ExitSourceScope();
                ps.Runspace = null;
                ps.Dispose();
            }
            return results.ToArray();
        }

        private static ILanguageBlock[] ReadYamlObjects(Source[] sources, RunspaceContext context)
        {
            var result = new Collection<ILanguageBlock>();
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithTypeConverter(new PSObjectYamlTypeConverter())
                .WithNodeTypeResolver(new PSOptionYamlTypeResolver())
                .WithNodeDeserializer(
                    inner => new ResourceNodeDeserializer(new LanguageExpressionDeserializer(inner)),
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

                        context.VerboseRuleDiscovery(path: file.Path);
                        context.EnterSourceScope(source: file);
                        using var reader = new StreamReader(file.Path);
                        var parser = new YamlDotNet.Core.Parser(reader);
                        parser.TryConsume<StreamStart>(out _);
                        while (parser.Current is DocumentStart)
                        {
                            var item = d.Deserialize<ResourceObject>(parser: parser);
                            if (item == null || item.Block == null)
                                continue;

                            result.Add(item.Block);
                        }
                    }
                }
            }
            finally
            {
                context.Writer?.ExitScope();
                context.PopScope(RunspaceScope.Resource);
                context.ExitSourceScope();
            }
            return result.Count == 0 ? Array.Empty<ILanguageBlock>() : result.ToArray();
        }

        private static ILanguageBlock[] ReadJsonObjects(Source[] sources, RunspaceContext context)
        {
            var result = new Collection<ILanguageBlock>();

            var deserializer = new JsonSerializer();

            deserializer.Converters.Add(new ResourceObjectJsonConverter());
            deserializer.Converters.Add(new FieldMapJsonConverter());

            try
            {
                context.Writer?.EnterScope("[Discovery.Resource]");
                context.PushScope(RunspaceScope.Resource);

                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        if (file.Type == SourceType.Json)
                        {
                            context.VerboseRuleDiscovery(file.Path);
                            context.EnterSourceScope(file);

                            using var reader = new JsonTextReader(new StreamReader(file.Path));

                            // Consume lines until start of array is found
                            while (reader.TokenType != JsonToken.StartArray)
                            {
                                reader.Read();
                            }

                            if (reader.TokenType == JsonToken.StartArray && reader.Read())
                            {
                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var value = deserializer.Deserialize<ResourceObject>(reader);

                                    if (value?.Block != null)
                                    {
                                        result.Add(value.Block);
                                    }

                                    reader.Read();
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                context.Writer?.ExitScope();
                context.PopScope(RunspaceScope.Resource);
                context.ExitSourceScope();
            }

            return result.Count == 0 ? Array.Empty<ILanguageBlock>() : result.ToArray();
        }

        public static void InvokeRuleBlock(RunspaceContext context, RuleBlock ruleBlock, RuleRecord ruleRecord)
        {
            RunspaceContext.CurrentThread = context;
            var condition = ruleBlock.Condition;
            context.VerboseObjectStart();

            try
            {
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
        }

        /// <summary>
        /// Convert matching langauge blocks to rules.
        /// </summary>
        private static Rule[] ToRuleV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            // Index rules by RuleId
            var results = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<RuleBlock>())
                {
                    if (!results.ContainsKey(block.RuleId))
                    {
                        results[block.RuleId] = new Rule
                        {
                            RuleId = block.RuleId,
                            RuleName = block.RuleName,
                            Source = block.Source,
                            Tag = block.Tag,
                            Info = block.Info,
                            DependsOn = block.DependsOn
                        };
                    }
                }

                foreach (var block in blocks.OfType<RuleV1>())
                {
                    if (!results.ContainsKey(block.Id))
                    {
                        context.EnterSourceScope(block.Source);
                        var info = GetHelpInfo(context, block.Name);
                        results[block.Id] = new Rule
                        {
                            RuleId = block.Id,
                            RuleName = block.Name,
                            Source = block.Source,
                            Tag = block.Metadata.Tags,
                            Info = info,
                            DependsOn = null // TODO: No support for DependsOn yet
                        };
                    }
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static RuleBlock[] ToRuleBlockV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            // Index rules by RuleId
            var results = new Dictionary<string, RuleBlock>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks)
                {
                    if (block is RuleBlock script && !results.ContainsKey(script.RuleId))
                    {
                        results[script.RuleId] = script;
                    }
                    else if (block is RuleV1 yaml && !results.ContainsKey(yaml.Id))
                    {
                        context.EnterSourceScope(yaml.Source);
                        var info = GetHelpInfo(context, yaml.Name) ?? new RuleHelpInfo(yaml.Name, yaml.Name, yaml.Source.ModuleName)
                        {
                            Synopsis = yaml.Synopsis
                        };
                        results[yaml.Id] = new RuleBlock
                        (
                            source: yaml.Source,
                            ruleName: yaml.Name,
                            info: info,
                            condition: new RuleVisitor(yaml.Source.ModuleName, yaml.Id, yaml.Spec),
                            tag: yaml.Metadata.Tags,
                            dependsOn: null,  // TODO: No support for DependsOn yet
                            configuration: null, // TODO: No support for rule configuration use module or workspace config
                            extent: null,
                            errorPreference: ActionPreference.Stop
                        );
                    }
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static RuleHelpInfo[] ToRuleHelp(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            // Index rules by RuleId
            var results = new Dictionary<string, RuleHelpInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<RuleBlock>())
                {
                    // Ignore rule blocks that don't match
                    if (!Match(context, block))
                        continue;

                    if (!results.ContainsKey(block.RuleId))
                        results[block.RuleId] = block.Info;
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static Baseline[] ToBaselineV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            if (blocks == null)
                return Array.Empty<Baseline>();

            // Index baselines by BaselineId
            var results = new Dictionary<string, Baseline>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<Baseline>().ToArray())
                {
                    // Ignore baselines that don't match
                    if (!Match(context, block))
                        continue;

                    if (!results.ContainsKey(block.BaselineId))
                        results[block.BaselineId] = block;
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static ModuleConfigV1[] ToModuleConfigV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            if (blocks == null)
                return Array.Empty<ModuleConfigV1>();

            // Index configurations by Name
            var results = new Dictionary<string, ModuleConfigV1>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<ModuleConfigV1>().ToArray())
                {
                    if (!results.ContainsKey(block.Name))
                        results[block.Name] = block;
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        /// <summary>
        /// Get conventions.
        /// </summary>
        private static IConvention[] GetConventions(ILanguageBlock[] blocks, RunspaceContext context)
        {
            // Index by Id
            var index = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<IConvention>(blocks.Length);
            try
            {
                foreach (var block in blocks.OfType<ScriptBlockConvention>().ToArray())
                {
                    // Ignore blocks that don't match
                    if (!Match(context, block))
                        continue;

                    if (!index.Contains(block.Id))
                        results.Add(block);
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return Sort(context, results.ToArray());
        }

        private static SelectorV1[] ToSelectorV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            if (blocks == null)
                return Array.Empty<SelectorV1>();

            // Index selectors by Id
            var results = new Dictionary<string, SelectorV1>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<SelectorV1>().ToArray())
                {
                    // Ignore selectors that don't match
                    if (!Match(context, block))
                        continue;

                    if (!results.ContainsKey(block.Id))
                        results[block.Id] = block;
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static void Import(ILanguageBlock[] blocks, RunspaceContext context)
        {
            foreach (var resource in blocks.OfType<IResource>().ToArray())
                context.Pipeline.Import(resource);
        }

        private static void Import(IConvention[] blocks, RunspaceContext context)
        {
            foreach (var resource in blocks)
                context.Import(resource);
        }

        private static bool Match(RunspaceContext context, RuleBlock resource)
        {
            context.EnterSourceScope(resource.Source);
            var filter = context.LanguageScope.GetFilter(ResourceKind.Rule);
            return filter == null || filter.Match(resource);
        }

        private static bool Match(RunspaceContext context, Rule resource)
        {
            context.EnterSourceScope(resource.Source);
            var filter = context.LanguageScope.GetFilter(ResourceKind.Rule);
            return filter == null || filter.Match(resource);
        }

        private static bool Match(RunspaceContext context, Baseline resource)
        {
            context.EnterSourceScope(resource.Source);
            var filter = context.LanguageScope.GetFilter(ResourceKind.Baseline);
            return filter == null || filter.Match(resource);
        }

        private static bool Match(RunspaceContext context, ScriptBlockConvention block)
        {
            context.EnterSourceScope(block.Source);
            var filter = context.LanguageScope.GetFilter(ResourceKind.Convention);
            return filter == null || filter.Match(block);
        }

        private static bool Match(RunspaceContext context, SelectorV1 resource)
        {
            context.EnterSourceScope(source: resource.Source);
            var filter = context.LanguageScope.GetFilter(ResourceKind.Selector);
            return filter == null || filter.Match(resource);
        }

        private static IConvention[] Sort(RunspaceContext context, IConvention[] conventions)
        {
            Array.Sort(conventions, new ConventionComparer(context));
            return conventions;
        }

        internal static RuleHelpInfo GetHelpInfo(RunspaceContext context, string name)
        {
            if (string.IsNullOrEmpty(context.Source.File.HelpPath))
                return null;

            var helpFileName = string.Concat(name, Markdown_Extension);
            var path = context.GetLocalizedPath(helpFileName);
            if (path == null || !TryDocument(path, out RuleDocument document))
                return null;

            return new RuleHelpInfo(name: name, displayName: document.Name ?? name, moduleName: context.Source.File.ModuleName)
            {
                Synopsis = document.Synopsis?.Text,
                Description = document.Description?.Text,
                Recommendation = document.Recommendation?.Text ?? document.Synopsis?.Text,
                Notes = document.Notes?.Text,
                Links = GetLinks(document.Links),
                Annotations = document.Annotations?.ToHashtable()
            };
        }

        private static bool TryDocument(string path, out RuleDocument document)
        {
            document = null;
            var markdown = File.ReadAllText(path);
            if (string.IsNullOrEmpty(markdown))
                return false;

            var reader = new MarkdownReader(yamlHeaderOnly: false);
            var stream = reader.Read(markdown, path);
            var lexer = new RuleLexer();
            document = lexer.Process(stream);
            return document != null;
        }

        private static RuleHelpInfo.Link[] GetLinks(Link[] links)
        {
            if (links == null || links.Length == 0)
                return null;

            var result = new RuleHelpInfo.Link[links.Length];
            for (var i = 0; i < links.Length; i++)
                result[i] = new RuleHelpInfo.Link(links[i].Name, links[i].Uri);

            return result;
        }
    }
}
