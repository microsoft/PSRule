// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace PSRule.Host
{
    internal static class HostHelper
    {
        public static Rule[] GetRule(Source[] source, RunspaceContext context, bool includeDependencies)
        {
            var builder = new DependencyGraphBuilder<Rule>(includeDependencies: includeDependencies);
            builder.Include(items: ToRule(GetLanguageBlock(context, source), context), filter: (b) => Match(context, b));
            return builder.GetItems();
        }

        public static RuleHelpInfo[] GetRuleHelp(Source[] source, RunspaceContext context)
        {
            return ToRuleHelp(GetLanguageBlock(context, source), context);
        }

        public static DependencyGraph<RuleBlock> GetRuleBlockGraph(Source[] source, RunspaceContext context)
        {
            var builder = new DependencyGraphBuilder<RuleBlock>(includeDependencies: true);
            builder.Include(items: GetLanguageBlock(context, source).OfType<RuleBlock>(), filter: (b) => Match(context, b));
            return builder.Build();
        }

        /// <summary>
        /// Read YAML objects and return baselines.
        /// </summary>
        public static IEnumerable<Baseline> GetBaseline(Source[] source, RunspaceContext context)
        {
            return ToBaseline(ReadYamlObjects(source, context), context);
        }

        /// <summary>
        /// Read YAML objects and return module configurations.
        /// </summary>
        public static IEnumerable<ModuleConfig> GetModuleConfig(Source[] source, RunspaceContext context)
        {
            return ToModuleConfig(ReadYamlObjects(source, context), context);
        }

        public static void ImportResource(Source[] source, RunspaceContext context)
        {
            Import(ReadYamlObjects(source, context), context);
        }

        /// <summary>
        /// Called from PowerShell to get additional metdata from a language block, such as comment help.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static CommentMetadata GetCommentMeta(string path, int lineNumber, int offset)
        {
            var context = RunspaceContext.CurrentThread;
            if (lineNumber < 0 || context.Pipeline.ExecutionScope == ExecutionScope.None || context.Source.SourceContentCache == null)
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

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private static IEnumerable<ILanguageBlock> GetLanguageBlock(RunspaceContext context, Source[] sources)
        {
            var results = new Collection<ILanguageBlock>();
            var ps = context.GetPowerShell();

            try
            {
                context.Writer.EnterScope("[Discovery.Rule]");
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Script;

                // Process scripts
                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        if (file.Type != RuleSourceType.Script)
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
                            {
                                context.WriteError(record);
                            }
                            continue;
                        }
                        if (errors != null && errors.Length > 0)
                        {
                            foreach (var error in errors)
                            {
                                context.WriteError(error);
                            }
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
                            if (ir.BaseObject is RuleBlock block)
                                results.Add(block);
                        }
                    }
                }
            }
            finally
            {
                context.Writer.ExitScope();
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.None;
                context.ExitSourceScope();
                ps.Runspace = null;
                ps.Dispose();
            }
            return results;
        }

        private static IEnumerable<ILanguageBlock> ReadYamlObjects(Source[] sources, RunspaceContext context)
        {
            var result = new Collection<ILanguageBlock>();
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new FieldMapYamlTypeConverter())
                .WithNodeDeserializer(
                    inner => new LanguageBlockDeserializer(inner),
                    s => s.InsteadOf<ObjectNodeDeserializer>())
                .Build();

            try
            {
                context.Writer?.EnterScope("[Discovery.Resource]");
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Yaml;
                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        if (file.Type != RuleSourceType.Yaml)
                            continue;

                        context.VerboseRuleDiscovery(path: file.Path);
                        context.EnterSourceScope(source: file);
                        using (var reader = new StreamReader(file.Path))
                        {
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
            }
            finally
            {
                context.Writer?.ExitScope();
                context.Pipeline.ExecutionScope = ExecutionScope.None;
                context.ExitSourceScope();
            }
            return result.Count == 0 ? Array.Empty<ILanguageBlock>() : result.ToArray();
        }

        public static void InvokeRuleBlock(RunspaceContext context, RuleBlock ruleBlock, RuleRecord ruleRecord)
        {
            RunspaceContext.CurrentThread = context;
            var ps = ruleBlock.Condition;
            ps.Streams.ClearStreams();
            context.VerboseObjectStart();

            try
            {
                var invokeResult = GetResult(ps.Invoke<Runtime.RuleConditionResult>());
                if (invokeResult == null)
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.PreconditionFail;
                    return;
                }
                else if (invokeResult.HadErrors || ps.HadErrors)
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

        private static Runtime.RuleConditionResult GetResult(Collection<Runtime.RuleConditionResult> value)
        {
            if (value == null || value.Count == 0)
                return null;

            return value[0];
        }

        /// <summary>
        /// Convert matching langauge blocks to rules.
        /// </summary>
        private static Rule[] ToRule(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
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

        private static Baseline[] ToBaseline(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
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

        private static ModuleConfig[] ToModuleConfig(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            if (blocks == null)
                return Array.Empty<ModuleConfig>();

            // Index configurations by Name
            var results = new Dictionary<string, ModuleConfig>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<ModuleConfig>().ToArray())
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

        private static void Import(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
        {
            foreach (var resource in blocks.OfType<IResource>().ToArray())
                context.Pipeline.Import(resource);
        }

        private static bool Match(RunspaceContext context, RuleBlock resource)
        {
            var scope = context.EnterSourceScope(source: resource.Source);
            return scope.Filter.Match(resource.RuleName, resource.Tag);
        }

        private static bool Match(RunspaceContext context, Rule resource)
        {
            var scope = context.EnterSourceScope(source: resource.Source);
            return scope.Filter.Match(resource.RuleName, resource.Tag);
        }

        private static bool Match(RunspaceContext context, Baseline resource)
        {
            var scope = context.EnterSourceScope(source: resource.Source);
            return scope.Filter.Match(resource.Name, null);
        }
    }
}
