using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PSRule.Host
{
    internal sealed class HostHelper
    {
        public static IEnumerable<Rule> GetRule(PSRuleOption option, string[] scriptPaths, RuleFilter filter)
        {
            return ToRule(GetLanguageBlock(option, scriptPaths), filter);
        }

        public static DependencyGraph<RuleBlock> GetRuleBlockGraph(PSRuleOption option, string[] scriptPaths, RuleFilter filter)
        {
            var builder = new DependencyGraphBuilder<RuleBlock>();
            builder.Include(items: GetLanguageBlock(option, scriptPaths).OfType<RuleBlock>(), filter: (b) => filter == null || filter.Match(b));
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
                        metadata.Description = comment.Substring(15);
                    }
                }

            }

            return metadata;
        }

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scriptPaths"></param>
        /// <returns></returns>
        private static IEnumerable<ILanguageBlock> GetLanguageBlock(PSRuleOption option, string[] scriptPaths)
        {
            var results = new Collection<ILanguageBlock>();
            var state = HostState.CreateSessionState();

            // Set PowerShell language mode
            state.LanguageMode = option.Execution.LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

            // Configure runspace
            var runspace = RunspaceFactory.CreateRunspace(state);
            runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;

            if (Runspace.DefaultRunspace == null)
            {
                Runspace.DefaultRunspace = runspace;
            }

            runspace.Open();
            runspace.SessionStateProxy.PSVariable.Set(new RuleVariable("Rule"));
            runspace.SessionStateProxy.PSVariable.Set(new TargetObjectVariable("TargetObject"));
            runspace.SessionStateProxy.PSVariable.Set("ErrorActionPreference", ActionPreference.Continue);
            runspace.SessionStateProxy.PSVariable.Set("WarningPreference", ActionPreference.Continue);
            runspace.SessionStateProxy.PSVariable.Set("VerbosePreference", ActionPreference.Continue);

            var ps = PipelineContext.CurrentThread.GetPowerShell();

            ps.Runspace = runspace;
            ps.Streams.Error.DataAdded += Error_DataAdded;
            ps.Streams.Warning.DataAdded += Warning_DataAdded;
            ps.Streams.Verbose.DataAdded += Verbose_DataAdded;
            ps.Streams.Information.DataAdded += Information_DataAdded;

            // Process scripts

            foreach (var path in scriptPaths)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The script was not found.", path);
                }

                PipelineContext.CurrentThread.WriteVerbose($"[PSRule][D] -- Discovering rules in: {path}");

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Can't find file", path);
                }

                // Invoke script
                ps.AddScript(path, true);
                var invokeResults = ps.Invoke();

                if (ps.HadErrors)
                {
                    throw new Exception(ps.Streams.Error[0].Exception.Message, ps.Streams.Error[0].Exception);
                }

                foreach (var ir in invokeResults)
                {
                    if (ir.BaseObject is ILanguageBlock)
                    {
                        var block = ir.BaseObject as ILanguageBlock;

                        results.Add(block);
                    }
                }
            }

            return results;
        }

        private static void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<InformationRecord>;
            var record = collection[e.Index];

            PipelineContext.CurrentThread.WriteInformation(informationRecord: record);
        }

        private static void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<VerboseRecord>;
            var record = collection[e.Index];

            PipelineContext.CurrentThread.WriteVerbose(record.Message, usePrefix: false);
        }

        private static void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<WarningRecord>;
            var record = collection[e.Index];

            PipelineContext.CurrentThread.WriteWarning(message: record.Message);
        }

        private static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var collection = sender as PSDataCollection<ErrorRecord>;
            var record = collection[e.Index];

            PipelineContext.CurrentThread.WriteError(errorRecord: record);
        }

        public static void InvokeRuleBlock(PipelineContext context, RuleBlock ruleBlock, RuleRecord ruleRecord)
        {
            try
            {
                var ps = PipelineContext.CurrentThread.GetPowerShell();
                ps.Commands.Clear();

                context.WriteVerboseObjectStart();

                context._Rule = ruleRecord;

                if (ruleBlock.If != null)
                {
                    if (!ruleBlock.If.Invoke())
                    {
                        ruleRecord.OutcomeReason = RuleOutcomeReason.PreconditionFail;
                        return;
                    }
                }

                //var ps2 = ruleBlock.Body.GetPowerShell();
                //ps2.

                //ps.AddCommand(new CommandInfo { })
                ps.AddScript(ruleBlock.Body.ToString(), useLocalScope: true);
                
                var invokeResult = new RuleConditionResult(ps.Invoke());

                if (invokeResult == null || invokeResult.Count == 0)
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.Inconclusive;
                    ruleRecord.Outcome = RuleOutcome.Fail;
                    context.WarnRuleInconclusive(ruleId: ruleRecord.RuleId);
                }
                else
                {
                    ruleRecord.OutcomeReason = RuleOutcomeReason.Processed;
                    ruleRecord.Outcome = invokeResult.AllOf ? RuleOutcome.Pass : RuleOutcome.Fail;
                }

                context.WriteVerboseConditionResult(pass: invokeResult?.Pass, count: invokeResult?.Count, outcome: ruleRecord.Outcome);

                return;
            }
            finally
            {
                PipelineContext.CurrentThread._Rule = null;
            }
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
                        SourcePath = block.SourcePath,
                        Description = block.Description,
                        Tag = block.Tag
                    };
                }
            }

            return results.Values.ToArray();
        }
    }
}
