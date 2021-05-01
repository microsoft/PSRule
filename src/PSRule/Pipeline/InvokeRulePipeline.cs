// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Data;
using PSRule.Host;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public interface IInvokePipelineBuilder : IPipelineBuilder
    {
        void InputPath(string[] path);

        void ResultVariable(string variableName);
    }

    internal abstract class InvokePipelineBuilderBase : PipelineBuilderBase, IInvokePipelineBuilder
    {
        protected InputFileInfo[] _InputPath;
        protected string _ResultVariableName;

        protected InvokePipelineBuilderBase(Source[] source, HostContext hostContext)
            : base(source, hostContext)
        {
            _InputPath = null;
        }

        public void InputPath(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            var basePath = PSRuleOption.GetWorkingPath();
            var ignoreGitPath = Option.Input.IgnoreGitPath ?? InputOption.Default.IgnoreGitPath.Value;
            var filter = PathFilterBuilder.Create(basePath, Option.Input.PathIgnore, ignoreGitPath);
            if (Option.Input.Format == InputFormat.File)
                filter.UseGitIgnore();

            var builder = new InputPathBuilder(GetOutput(), basePath, "*", filter.Build());
            builder.Add(path);
            _InputPath = builder.Build();
        }

        public void ResultVariable(string variableName)
        {
            _ResultVariableName = variableName;
        }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            base.Configure(option);

            Option.Execution.InconclusiveWarning = option.Execution.InconclusiveWarning ?? ExecutionOption.Default.InconclusiveWarning;
            Option.Execution.NotProcessedWarning = option.Execution.NotProcessedWarning ?? ExecutionOption.Default.NotProcessedWarning;

            Option.Logging.RuleFail = option.Logging.RuleFail ?? LoggingOption.Default.RuleFail;
            Option.Logging.RulePass = option.Logging.RulePass ?? LoggingOption.Default.RulePass;
            Option.Logging.LimitVerbose = option.Logging.LimitVerbose;
            Option.Logging.LimitDebug = option.Logging.LimitDebug;

            Option.Output.As = option.Output.As ?? OutputOption.Default.As;
            Option.Output.Culture = GetCulture(option.Output.Culture);
            Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
            Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
            Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;

            if (option.Rule != null)
                Option.Rule = new RuleOption(option.Rule);

            if (option.Configuration != null)
                Option.Configuration = new ConfigurationOption(option.Configuration);

            ConfigureBinding(option);
            Option.Requires = new RequiresOption(option.Requires);
            if (option.Suppression.Count > 0)
                Option.Suppression = new SuppressionOption(option.Suppression);

            return this;
        }

        public override IPipeline Build()
        {
            if (!RequireModules() || !RequireSources())
                return null;

            return new InvokeRulePipeline(PrepareContext(BindTargetNameHook, BindTargetTypeHook, BindFieldHook), Source, PrepareWriter(), Option.Output.Outcome.Value);
        }

        protected override PipelineReader PrepareReader()
        {
            if (!string.IsNullOrEmpty(Option.Input.ObjectPath))
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ReadObjectPath(sourceObject, next, Option.Input.ObjectPath, true);
                });
            }

            if (Option.Input.Format == InputFormat.Yaml)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromYaml(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Json)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromJson(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Markdown)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromMarkdown(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.PowerShellData)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromPowerShellData(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.File)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromGitHead(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Detect && _InputPath != null)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.DetectInputFormat(sourceObject, next);
                });
            }
            return new PipelineReader(VisitTargetObject, _InputPath);
        }
    }

    /// <summary>
    /// A helper to construct the pipeline for Invoke-PSRule.
    /// </summary>
    internal sealed class InvokeRulePipelineBuilder : InvokePipelineBuilderBase
    {
        internal InvokeRulePipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext) { }
    }

    internal sealed class InvokeRulePipeline : RulePipeline, IPipeline
    {
        private readonly RuleOutcome _Outcome;
        private readonly DependencyGraph<RuleBlock> _RuleGraph;

        // A per rule summary of rules that have been processed and the outcome
        private readonly Dictionary<string, RuleSummaryRecord> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly RuleSuppressionFilter _SuppressionFilter;
        private readonly List<InvokeResult> _Completed;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal InvokeRulePipeline(PipelineContext context, Source[] source, PipelineWriter writer, RuleOutcome outcome)
            : base(context, source, context.Reader, writer)
        {
            HostHelper.ImportResource(Source, Context);
            _RuleGraph = HostHelper.GetRuleBlockGraph(Source, Context);
            RuleCount = _RuleGraph.Count;
            if (RuleCount == 0)
                Context.WarnRuleNotFound();

            _Outcome = outcome;
            _Summary = new Dictionary<string, RuleSummaryRecord>();
            _ResultFormat = context.Option.Output.As.Value;
            _SuppressionFilter = new RuleSuppressionFilter(context.Option.Suppression);
            _Completed = new List<InvokeResult>();
        }

        public int RuleCount { get; private set; }

        public override void Process(PSObject sourceObject)
        {
            try
            {
                Reader.Enqueue(sourceObject);
                while (Reader.TryDequeue(out PSObject next))
                {
                    var result = ProcessTargetObject(next);
                    _Completed.Add(result);
                    Writer.WriteObject(result, false);
                }
            }
            catch (Exception)
            {
                End();
                throw;
            }
        }

        public override void End()
        {
            if (_Completed.Count > 0)
            {
                var completed = _Completed.ToArray();
                _Completed.Clear();
                Context.End(completed);
            }

            if (_ResultFormat == ResultFormat.Summary)
                Writer.WriteObject(_Summary.Values.Where(r => _Outcome == RuleOutcome.All || (r.Outcome & _Outcome) > 0).ToArray(), true);

            Writer.End();
        }

        private InvokeResult ProcessTargetObject(PSObject targetObject)
        {
            try
            {
                Context.EnterTargetObject(targetObject);
                var result = new InvokeResult();
                var ruleCounter = 0;

                // Process rule blocks ordered by dependency graph
                foreach (var ruleBlockTarget in _RuleGraph.GetSingleTarget())
                {
                    // Enter rule block scope
                    var ruleRecord = Context.EnterRuleBlock(ruleBlock: ruleBlockTarget.Value);
                    ruleCounter++;

                    try
                    {
                        if (Pipeline.ShouldFilter())
                            continue;

                        // Check if dependency failed
                        if (ruleBlockTarget.Skipped)
                        {
                            ruleRecord.OutcomeReason = RuleOutcomeReason.DependencyFail;
                        }
                        // Check for suppression
                        else if (_SuppressionFilter.Match(ruleName: ruleRecord.RuleName, targetName: ruleRecord.TargetName))
                        {
                            ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                        }
                        else
                        {
                            HostHelper.InvokeRuleBlock(context: Context, ruleBlock: ruleBlockTarget.Value, ruleRecord: ruleRecord);
                            if (ruleRecord.OutcomeReason == RuleOutcomeReason.PreconditionFail)
                                ruleCounter--;
                        }

                        // Report outcome to dependency graph
                        if (ruleRecord.Outcome == RuleOutcome.Pass)
                            ruleBlockTarget.Pass();
                        else if (ruleRecord.Outcome == RuleOutcome.Fail || ruleRecord.Outcome == RuleOutcome.Error)
                            ruleBlockTarget.Fail();

                        AddToSummary(ruleBlock: ruleBlockTarget.Value, outcome: ruleRecord.Outcome);
                        if (ShouldOutput(ruleRecord.Outcome))
                            result.Add(ruleRecord);
                    }
                    finally
                    {
                        // Exit rule block scope
                        Context.ExitRuleBlock();
                    }
                }

                if (ruleCounter == 0)
                    Context.WarnObjectNotProcessed();

                return result;
            }
            finally
            {
                Context.ExitTargetObject();
            }
        }

        private bool ShouldOutput(RuleOutcome outcome)
        {
            return _Outcome == RuleOutcome.All || (outcome & _Outcome) > 0;
        }

        /// <summary>
        /// Add rule result to summary.
        /// </summary>
        private void AddToSummary(RuleBlock ruleBlock, RuleOutcome outcome)
        {
            if (!_Summary.TryGetValue(ruleBlock.RuleId, out RuleSummaryRecord s))
            {
                s = new RuleSummaryRecord(
                    ruleId: ruleBlock.RuleId,
                    ruleName: ruleBlock.RuleName,
                    tag: ruleBlock.Tag,
                    info: ruleBlock.Info
                );
                _Summary.Add(ruleBlock.RuleId, s);
            }

            if (outcome == RuleOutcome.Pass)
            {
                s.Pass++;
                Context.Pass();
            }
            else if (outcome == RuleOutcome.Fail)
            {
                s.Fail++;
                Context.Fail();
            }
            else if (outcome == RuleOutcome.Error)
                s.Error++;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                    _RuleGraph.Dispose();

                _Disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
