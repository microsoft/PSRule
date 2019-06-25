using PSRule.Annotations;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PSRule.Host
{
    internal static class HostHelper
    {
        public static IEnumerable<Rule> GetRule(RuleSource[] source, RuleFilter filter)
        {
            return ToRule(GetLanguageBlock(sources: source), filter);
        }

        public static IEnumerable<RuleHelpInfo> GetRuleHelp(RuleSource[] source, RuleFilter filter)
        {
            return ToRuleHelp(GetLanguageBlock(sources: source), filter);
        }

        public static DependencyGraph<RuleBlock> GetRuleBlockGraph(RuleSource[] source, RuleFilter filter)
        {
            var builder = new DependencyGraphBuilder<RuleBlock>();
            builder.Include(items: GetLanguageBlock(sources: source).OfType<RuleBlock>(), filter: (b) => filter == null || filter.Match(b));
            return builder.Build();
        }

        /// <summary>
        /// Called from PowerShell to get additional metdata from a language block, such as comment help.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static BlockMetadata GetCommentMeta(string path, int lineNumber, int offset)
        {
            if (lineNumber == 1)
            {
                return new BlockMetadata();
            }

            var lines = File.ReadAllLines(path, Encoding.UTF8);

            var i = lineNumber - 1;
            var comments = new List<string>();

            for (; i >= 0; i--)
            {
                if (lines[i].Contains("#"))
                {
                    comments.Insert(0, lines[i]);
                }
            }

            var metadata = new BlockMetadata();

            // Check if any comments were found
            if (comments.Count > 0)
            {
                foreach (var comment in comments)
                {
                    if (comment.StartsWith("# Description: "))
                    {
                        metadata.Synopsis = comment.Substring(15);
                    }

                    if (comment.StartsWith("# Synopsis: "))
                    {
                        metadata.Synopsis = comment.Substring(12);
                    }
                }

            }

            return metadata;
        }

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        private static IEnumerable<ILanguageBlock> GetLanguageBlock(RuleSource[] sources)
        {
            var results = new Collection<ILanguageBlock>();

            var runspace = PipelineContext.CurrentThread.GetRunspace();
            var ps = PowerShell.Create();

            try
            {
                ps.Runspace = runspace;
                PipelineContext.EnableLogging(ps);

                // Process scripts

                foreach (var source in sources)
                {
                    ps.Commands.Clear();

                    if (!File.Exists(source.Path))
                    {
                        throw new FileNotFoundException(PSRuleResources.ScriptNotFound, source.Path);
                    }

                    PipelineContext.CurrentThread.Source = source;
                    PipelineContext.CurrentThread.VerboseRuleDiscovery(path: source.Path);
                    //PipelineContext.CurrentThread.UseSource(source: source);

                    // Invoke script
                    ps.AddScript(string.Concat("& '", source.Path, "'"), true);
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
                        //else if (ir.BaseObject is ILanguageBlock)
                        //{
                        //    var block = ir.BaseObject as ILanguageBlock;
                        //    results.Add(block);
                        //}
                    }
                }
            }
            finally
            {
                PipelineContext.CurrentThread.Source = null;
                ps.Runspace = null;
                ps.Dispose();
            }

            return results;
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
        /// <param name="blocks"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static Rule[] ToRule(IEnumerable<ILanguageBlock> blocks, RuleFilter filter)
        {
            // Index deployments by environment/name
            var results = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase);

            foreach (var block in blocks.OfType<RuleBlock>())
            {
                // Ignore deployment blocks that don't match
                if (filter != null && !filter.Match(block))
                {
                    continue;
                }

                if (!results.ContainsKey(block.RuleId))
                {
                    results[block.RuleId] = new Rule
                    {
                        RuleId = block.RuleId,
                        RuleName = block.RuleName,
                        SourcePath = block.Source.Path,
                        ModuleName = block.Source.ModuleName,
                        Tag = block.Tag,
                        Info = block.Info
                    };
                }
            }

            return results.Values.ToArray();
        }

        private static RuleHelpInfo[] ToRuleHelp(IEnumerable<ILanguageBlock> blocks, RuleFilter filter)
        {
            // Index deployments by environment/name
            var results = new Dictionary<string, RuleHelpInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var block in blocks.OfType<RuleBlock>())
            {
                // Ignore deployment blocks that don't match
                if (filter != null && !filter.Match(block))
                {
                    continue;
                }

                if (!results.ContainsKey(block.RuleId))
                {
                    results[block.RuleId] = block.Info;
                }
            }

            return results.Values.ToArray();
        }
    }
}
