// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Rules;

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
            var ignoreRepositoryCommon = Option.Input.IgnoreRepositoryCommon ?? InputOption.Default.IgnoreRepositoryCommon.Value;
            var filter = PathFilterBuilder.Create(basePath, Option.Input.PathIgnore, ignoreGitPath, ignoreRepositoryCommon);
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
            Option.Execution.SuppressedRuleWarning = option.Execution.SuppressedRuleWarning ?? ExecutionOption.Default.SuppressedRuleWarning;
            Option.Execution.InvariantCultureWarning = option.Execution.InvariantCultureWarning ?? ExecutionOption.Default.InvariantCultureWarning;

            Option.Logging.RuleFail = option.Logging.RuleFail ?? LoggingOption.Default.RuleFail;
            Option.Logging.RulePass = option.Logging.RulePass ?? LoggingOption.Default.RulePass;
            Option.Logging.LimitVerbose = option.Logging.LimitVerbose;
            Option.Logging.LimitDebug = option.Logging.LimitDebug;

            Option.Output.As = option.Output.As ?? OutputOption.Default.As;
            Option.Output.Culture = GetCulture(option.Output.Culture);
            Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
            Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
            Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;
            Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);

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

        public override IPipeline Build(IPipelineWriter writer = null)
        {
            return !RequireModules() || !RequireSources()
                ? null
                : (IPipeline)new InvokeRulePipeline(PrepareContext(BindTargetNameHook, BindTargetTypeHook, BindFieldHook), Source, writer ?? PrepareWriter(), Option.Output.Outcome.Value);
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

        private readonly bool _IsSummary;
        private readonly SuppressionFilter _SuppressionFilter;
        private readonly SuppressionFilter _SuppressionGroupFilter;
        private readonly List<InvokeResult> _Completed;

        // Track whether Dispose has been called.
        private bool _Disposed;

        internal InvokeRulePipeline(PipelineContext context, Source[] source, IPipelineWriter writer, RuleOutcome outcome)
            : base(context, source, context.Reader, writer)
        {
            _RuleGraph = HostHelper.GetRuleBlockGraph(Source, Context);
            RuleCount = _RuleGraph.Count;
            if (RuleCount == 0)
                Context.WarnRuleNotFound();

            _Outcome = outcome;
            _IsSummary = context.Option.Output.As.Value == ResultFormat.Summary;
            _Summary = _IsSummary ? new Dictionary<string, RuleSummaryRecord>() : null;
            var allRuleBlocks = _RuleGraph.GetAll();
            var resourceIndex = new ResourceIndex(allRuleBlocks);
            _SuppressionFilter = new SuppressionFilter(Context, context.Option.Suppression, resourceIndex);
            _SuppressionGroupFilter = new SuppressionFilter(Pipeline.SuppressionGroup, resourceIndex);

            _Completed = new List<InvokeResult>();
        }

        public int RuleCount { get; private set; }

        public override void Process(PSObject sourceObject)
        {
            try
            {
                Reader.Enqueue(sourceObject);
                while (Reader.TryDequeue(out var next))
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

            if (_IsSummary)
                Writer.WriteObject(_Summary.Values.Where(r => _Outcome == RuleOutcome.All || (r.Outcome & _Outcome) > 0).ToArray(), true);

            Writer.End();
        }

        private InvokeResult ProcessTargetObject(TargetObject targetObject)
        {
            try
            {
                Context.EnterTargetObject(targetObject);
                var result = new InvokeResult();
                var ruleCounter = 0;
                var suppressedRuleCounter = 0;
                var suppressionGroupCounter = new Dictionary<string, int>();

                // Process rule blocks ordered by dependency graph
                foreach (var ruleBlockTarget in _RuleGraph.GetSingleTarget())
                {
                    // Enter rule block scope
                    var ruleRecord = Context.EnterRuleBlock(ruleBlock: ruleBlockTarget.Value);
                    ruleCounter++;

                    try
                    {
                        if (Context.Binding.ShouldFilter)
                            continue;

                        // Check if dependency failed
                        if (ruleBlockTarget.Skipped)
                        {
                            ruleRecord.OutcomeReason = RuleOutcomeReason.DependencyFail;
                        }
                        // Check for suppression
                        else if (_SuppressionFilter.Match(id: ruleBlockTarget.Value.Id, targetName: ruleRecord.TargetName))
                        {
                            ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                            suppressedRuleCounter++;

                            if (!_IsSummary)
                                Context.WarnRuleSuppressed(ruleId: ruleRecord.RuleId);
                        }
                        // Check for suppression group
                        else if (_SuppressionGroupFilter.TrySuppressionGroup(ruleId: ruleRecord.RuleId, targetObject, out var suppressionGroupId))
                        {
                            ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                            if (!_IsSummary)
                                Context.WarnRuleSuppressionGroup(ruleId: ruleRecord.RuleId, suppressionGroupId);
                            else
                                suppressionGroupCounter[suppressionGroupId] = suppressionGroupCounter.TryGetValue(suppressionGroupId, out var count) ? ++count : 1;
                        }
                        else
                        {
                            HostHelper.InvokeRuleBlock(context: Context, ruleBlock: ruleBlockTarget.Value, ruleRecord: ruleRecord);
                            if (ruleRecord.OutcomeReason == RuleOutcomeReason.PreconditionFail)
                                ruleCounter--;
                        }

                        // Report outcome to dependency graph
                        if (ruleRecord.Outcome == RuleOutcome.Pass)
                        {
                            ruleBlockTarget.Pass();
                            Context.Pass();
                        }
                        else if (ruleRecord.Outcome == RuleOutcome.Fail)
                        {
                            ruleBlockTarget.Fail();
                            Context.Fail();
                        }
                        else if (ruleRecord.Outcome == RuleOutcome.Error)
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

                if (_IsSummary)
                {
                    if (suppressedRuleCounter > 0)
                        Context.WarnRuleCountSuppressed(ruleCount: suppressedRuleCounter);

                    foreach (var keyValuePair in suppressionGroupCounter)
                        Context.WarnRuleSuppressionGroupCount(ruleCount: keyValuePair.Value, suppressionGroupId: keyValuePair.Key);
                }
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
            if (!_IsSummary)
                return;

            if (!_Summary.TryGetValue(ruleBlock.Id.Value, out var s))
            {
                s = new RuleSummaryRecord(
                    ruleId: ruleBlock.Id.Value,
                    ruleName: ruleBlock.Name,
                    tag: ruleBlock.Tag,
                    info: ruleBlock.Info
                );
                _Summary.Add(ruleBlock.Id.Value, s);
            }
            s.Add(outcome);
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
