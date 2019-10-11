// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;
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
        public static Rule[] GetRule(Source[] source, PipelineContext context, bool includeDependencies)
        {
            var builder = new DependencyGraphBuilder<Rule>(includeDependencies: includeDependencies);
            builder.Include(items: ToRule(GetLanguageBlock(sources: source), context), filter: (b) => Match(context, b));
            return builder.GetItems();
        }

        public static RuleHelpInfo[] GetRuleHelp(Source[] source, PipelineContext context)
        {
            return ToRuleHelp(GetLanguageBlock(sources: source), context);
        }

        public static DependencyGraph<RuleBlock> GetRuleBlockGraph(Source[] source, PipelineContext context)
        {
            var builder = new DependencyGraphBuilder<RuleBlock>(includeDependencies: true);
            builder.Include(items: GetLanguageBlock(sources: source).OfType<RuleBlock>(), filter: (b) => Match(context, b));
            return builder.Build();
        }

        public static IEnumerable<Baseline> GetBaseline(Source[] source, PipelineContext context)
        {
            return ToBaseline(ReadYamlObjects(source, context), context);
        }

        public static void ImportResource(Source[] source, PipelineContext context)
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
            var context = PipelineContext.CurrentThread;
            if (lineNumber < 0 || context.ExecutionScope == ExecutionScope.None || context.Source.SourceContentCache == null)
                return new CommentMetadata();

            var lines = context.Source.SourceContentCache;
            var i = lineNumber;
            var comments = new List<string>();

            // Back track lines with comments immediately before block
            for (; i >= 0 && lines[i].Contains("#"); i--)
            {
                comments.Insert(0, lines[i]);
            }

            // Check if any comments were found
            var metadata = new CommentMetadata();
            if (comments.Count > 0)
            {
                foreach (var comment in comments)
                {
                    if (comment.StartsWith("# Description: "))
                        metadata.Synopsis = comment.Substring(15);

                    if (comment.StartsWith("# Synopsis: "))
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
        private static IEnumerable<ILanguageBlock> GetLanguageBlock(Source[] sources)
        {
            var results = new Collection<ILanguageBlock>();

            var runspace = PipelineContext.CurrentThread.GetRunspace();
            var ps = PowerShell.Create();

            try
            {
                ps.Runspace = runspace;
                PipelineContext.EnableLogging(ps);
                PipelineContext.CurrentThread.Logger.EnterScope("[Discovery.Rule]");
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Script;

                // Process scripts

                foreach (var source in sources)
                {
                    foreach (var file in source.File)
                    {
                        if (file.Type != RuleSourceType.Script)
                        {
                            continue;
                        }

                        ps.Commands.Clear();

                        PipelineContext.CurrentThread.VerboseRuleDiscovery(path: file.Path);
                        PipelineContext.CurrentThread.EnterSourceScope(source: file);

                        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(file.Path, out Token[] tokens, out ParseError[] errors);
                        var visitor = new RuleLanguageAst(PipelineContext.CurrentThread);
                        scriptAst.Visit(visitor);

                        if (visitor.Errors != null && visitor.Errors.Count > 0)
                        {
                            foreach (var record in visitor.Errors)
                            {
                                PipelineContext.CurrentThread.WriteError(record);
                            }
                            continue;
                        }

                        if (errors != null && errors.Length > 0)
                        {
                            foreach (var error in errors)
                            {
                                PipelineContext.CurrentThread.WriteError(error);
                            }
                            continue;
                        }

                        // Invoke script
                        ps.AddScript(string.Concat("& '", file.Path, "'"), true);
                        var invokeResults = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            // Discovery has errors so skip this file
                            continue;
                        }

                        foreach (var ir in invokeResults)
                        {
                            if (ir.BaseObject is RuleBlock)
                            {
                                var block = ir.BaseObject as RuleBlock;
                                results.Add(block);
                            }
                        }
                    }
                }
            }
            finally
            {
                PipelineContext.CurrentThread.Logger.ExitScope();
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.None;
                PipelineContext.CurrentThread.ExitSourceScope();
                ps.Runspace = null;
                ps.Dispose();
            }

            return results;
        }

        private static IEnumerable<ILanguageBlock> ReadYamlObjects(Source[] sources, PipelineContext context)
        {
            var result = new Collection<ILanguageBlock>();
            var d = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithNodeDeserializer(
                    inner => new LanguageBlockDeserializer(inner),
                    s => s.InsteadOf<ObjectNodeDeserializer>())
                .Build();

            try
            {
                PipelineContext.CurrentThread.Logger?.EnterScope("[Discovery.Resource]");
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.Yaml;
                foreach (var source in sources)
                {


                    foreach (var file in source.File)
                    {
                        if (file.Type != RuleSourceType.Yaml)
                        {
                            continue;
                        }
                        PipelineContext.CurrentThread.VerboseRuleDiscovery(path: file.Path);
                        PipelineContext.CurrentThread.EnterSourceScope(source: file);
                        using (var reader = new StreamReader(file.Path))
                        {
                            var parser = new YamlDotNet.Core.Parser(reader);
                            parser.Expect<StreamStart>();

                            while (parser.Accept<DocumentStart>())
                            {
                                var item = d.Deserialize<ResourceObject>(parser: parser);
                                if (item == null || item.Block == null)
                                {
                                    continue;
                                }
                                result.Add(item.Block);
                            }
                            if (result.Count == 0)
                            {
                                return null;
                            }
                            return result.ToArray();
                        }
                    }
                }
            }
            finally
            {
                PipelineContext.CurrentThread.Logger?.ExitScope();
                PipelineContext.CurrentThread.ExecutionScope = ExecutionScope.None;
                PipelineContext.CurrentThread.ExitSourceScope();
            }
            return result;
        }

        public static void InvokeRuleBlock(PipelineContext context, RuleBlock ruleBlock, RuleRecord ruleRecord)
        {
            PipelineContext.CurrentThread = context;
            var ps = ruleBlock.Condition;
            ps.Streams.ClearStreams();
            context.VerboseObjectStart();

            var invokeResult = ps.Invoke<RuleConditionResult>().FirstOrDefault();

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
                context.WarnRuleInconclusive(ruleId: ruleRecord.RuleId);
            }
            else
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.Processed;
                ruleRecord.Outcome = invokeResult.AllOf() ? RuleOutcome.Pass : RuleOutcome.Fail;
            }
            context.VerboseConditionResult(pass: invokeResult.Pass, count: invokeResult.Count, outcome: ruleRecord.Outcome);
        }

        /// <summary>
        /// Convert matching langauge blocks to rules.
        /// </summary>
        private static Rule[] ToRule(IEnumerable<ILanguageBlock> blocks, PipelineContext context)
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

        private static RuleHelpInfo[] ToRuleHelp(IEnumerable<ILanguageBlock> blocks, PipelineContext context)
        {
            // Index rules by RuleId
            var results = new Dictionary<string, RuleHelpInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<RuleBlock>())
                {
                    // Ignore rule blocks that don't match
                    if (!Match(context, block))
                    {
                        continue;
                    }
                    if (!results.ContainsKey(block.RuleId))
                    {
                        results[block.RuleId] = block.Info;
                    }
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static Baseline[] ToBaseline(IEnumerable<ILanguageBlock> blocks, PipelineContext context)
        {
            // Index baselines by BaselineId
            var results = new Dictionary<string, Baseline>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var block in blocks.OfType<Baseline>().ToArray())
                {
                    // Ignore baselines that don't match
                    if (!Match(context, block))
                    {
                        continue;
                    }
                    if (!results.ContainsKey(block.BaselineId))
                    {
                        results[block.BaselineId] = block;
                    }
                }
            }
            finally
            {
                context.ExitSourceScope();
            }
            return results.Values.ToArray();
        }

        private static void Import(IEnumerable<ILanguageBlock> blocks, PipelineContext context)
        {
            foreach (var resource in blocks.OfType<IResource>().ToArray())
            {
                context.Import(resource);
            }
        }

        private static bool Match(PipelineContext context, RuleBlock resource)
        {
            var scope = context.EnterSourceScope(source: resource.Source);
            return scope.Filter.Match(resource.RuleName, resource.Tag);
        }

        private static bool Match(PipelineContext context, Rule resource)
        {
            var scope = context.EnterSourceScope(source: resource.Source);
            return scope.Filter.Match(resource.RuleName, resource.Tag);
        }

        private static bool Match(PipelineContext context, Baseline resource)
        {
            var scope = context.EnterSourceScope(source: resource.Source);
            return scope.Filter.Match(resource.Name, null);
        }
    }
}
