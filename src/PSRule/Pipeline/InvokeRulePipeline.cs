// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Host;
using PSRule.Resources;
using PSRule.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public interface IInvokePipelineBuilder : IPipelineBuilder
    {
        void Limit(RuleOutcome outcome);

        void InputPath(string[] path);
    }

    internal abstract class InvokePipelineBuilderBase : PipelineBuilderBase, IInvokePipelineBuilder
    {
        protected RuleOutcome Outcome;
        protected string[] _InputPath;
        private VisitTargetObject _VisitTargetObject;

        protected BindTargetMethod _BindTargetNameHook;
        protected BindTargetMethod _BindTargetTypeHook;
        protected BindTargetMethod _BindFieldHook;

        protected InvokePipelineBuilderBase(Source[] source)
            : base(source)
        {
            Outcome = RuleOutcome.Processed;
            _InputPath = null;
            _VisitTargetObject = PipelineReceiverActions.PassThru;
            _BindTargetNameHook = PipelineHookActions.BindTargetName;
            _BindTargetTypeHook = PipelineHookActions.BindTargetType;
            _BindFieldHook = PipelineHookActions.BindField;
        }

        public void Limit(RuleOutcome outcome)
        {
            Outcome = outcome;
        }

        public void InputPath(string[] path)
        {
            _InputPath = path;
        }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            base.Configure(option);

            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
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

            if (option.Pipeline.BindTargetName != null && option.Pipeline.BindTargetName.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                    throw new PipelineConfigurationException(optionName: "BindTargetName", message: PSRuleResources.ConstrainedTargetBinding);

                foreach (var action in option.Pipeline.BindTargetName)
                {
                    _BindTargetNameHook = AddBindTargetAction(action, _BindTargetNameHook);
                }
            }

            if (option.Pipeline.BindTargetType != null && option.Pipeline.BindTargetType.Count > 0)
            {
                // Do not allow custom binding functions to be used with constrained language mode
                if (Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                    throw new PipelineConfigurationException(optionName: "BindTargetType", message: PSRuleResources.ConstrainedTargetBinding);

                foreach (var action in option.Pipeline.BindTargetType)
                {
                    _BindTargetTypeHook = AddBindTargetAction(action, _BindTargetTypeHook);
                }
            }

            if (option.Suppression.Count > 0)
                Option.Suppression = new SuppressionOption(option.Suppression);

            ConfigureLogger(Option);
            return this;
        }

        public sealed override IPipeline Build()
        {
            return new InvokeRulePipeline(PrepareContext(_BindTargetNameHook, _BindTargetTypeHook, _BindFieldHook), Source, PrepareReader(), PrepareWriter(), Outcome);
        }

        private BindTargetMethod AddBindTargetAction(BindTargetFunc action, BindTargetMethod previous)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            return (propertyNames, caseSensitive, targetObject) => action(propertyNames, caseSensitive, targetObject, previous);
        }

        private BindTargetMethod AddBindTargetAction(BindTargetName action, BindTargetMethod previous)
        {
            return AddBindTargetAction((parameterNames, caseSensitive, targetObject, next) =>
            {
                var targetType = action(targetObject);
                return string.IsNullOrEmpty(targetType) ? next(parameterNames, caseSensitive, targetObject) : targetType;
            }, previous);
        }

        private void AddVisitTargetObjectAction(VisitTargetObjectAction action)
        {
            // Nest the previous write action in the new supplied action
            // Execution chain will be: action -> previous -> previous..n
            var previous = _VisitTargetObject;
            _VisitTargetObject = (targetObject) => action(targetObject, previous);
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
            else if (Option.Input.Format == InputFormat.Detect && _InputPath != null)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.DetectInputFormat(sourceObject, next);
                });
            }

            return new PipelineReader(_VisitTargetObject, _InputPath);
        }
    }

    /// <summary>
    /// A helper to construct the pipeline for Invoke-PSRule.
    /// </summary>
    internal sealed class InvokeRulePipelineBuilder : InvokePipelineBuilderBase
    {
        internal InvokeRulePipelineBuilder(Source[] source)
            : base(source) { }
    }

    internal sealed class InvokeRulePipeline : RulePipeline, IPipeline
    {
        private readonly RuleOutcome _Outcome;
        private readonly DependencyGraph<RuleBlock> _RuleGraph;

        // A per rule summary of rules that have been processed and the outcome
        private readonly Dictionary<string, RuleSummaryRecord> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly RuleSuppressionFilter _SuppressionFilter;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal InvokeRulePipeline(PipelineContext context, Source[] source, PipelineReader reader, PipelineWriter writer, RuleOutcome outcome)
            : base(context, source, reader, writer)
        {
            HostHelper.ImportResource(source: Source, context: context);
            _RuleGraph = HostHelper.GetRuleBlockGraph(source: Source, context: context);
            RuleCount = _RuleGraph.Count;

            if (RuleCount == 0)
                Context.WarnRuleNotFound();

            _Outcome = outcome;
            _Summary = new Dictionary<string, RuleSummaryRecord>();
            _ResultFormat = context.Option.Output.As.Value;
            _SuppressionFilter = new RuleSuppressionFilter(context.Option.Suppression);
        }

        public int RuleCount { get; private set; }

        public override void Process(PSObject targetObject)
        {
            Reader.Enqueue(targetObject);
            while (Reader.TryDequeue(out PSObject next))
            {
                var result = ProcessTargetObject(next);
                Writer.Write(result, false);
            }
        }

        public override void End()
        {
            if (_ResultFormat == ResultFormat.Summary)
                Writer.Write(_Summary.Values.Where(r => _Outcome == RuleOutcome.All || (r.Outcome & _Outcome) > 0), true);

            Writer.End();
        }

        private InvokeResult ProcessTargetObject(PSObject targetObject)
        {
            Context.SetTargetObject(targetObject);
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
                    if (Context.ShouldFilter())
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

        private bool ShouldOutput(RuleOutcome outcome)
        {
            return _ResultFormat == ResultFormat.Detail &&
                (_Outcome == RuleOutcome.All || (outcome & _Outcome) > 0);
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
