using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipeline : RulePipeline
    {
        private readonly RuleOutcome _Outcome;
        private readonly StreamManager _StreamManager;
        private readonly DependencyGraph<RuleBlock> _RuleGraph;

        // A per rule summary of rules that have been processed and the outcome
        private readonly Dictionary<string, RuleSummaryRecord> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly RuleSuppressionFilter _SuppressionFilter;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal InvokeRulePipeline(StreamManager streamManager, PSRuleOption option, RuleSource[] source, RuleFilter filter, RuleOutcome outcome, PipelineContext context)
            : base(context, option, source, filter)
        {
            _StreamManager = streamManager;
            _RuleGraph = HostHelper.GetRuleBlockGraph(source: _Source, filter: _Filter);
            RuleCount = _RuleGraph.Count;

            if (RuleCount == 0)
            {
                _Context.WarnRuleNotFound();
            }

            _Outcome = outcome;
            _Summary = new Dictionary<string, RuleSummaryRecord>();
            _ResultFormat = option.Output.As.Value;
            _SuppressionFilter = new RuleSuppressionFilter(option.Suppression);
        }

        public int RuleCount { get; private set; }

        public void Begin()
        {
            _StreamManager.Begin();
        }

        public void Process(PSObject[] targetObjects)
        {
            foreach (var targetObject in targetObjects)
            {
                _StreamManager.Process(targetObject);
            }

            while (_StreamManager.Next(out PSObject next))
            {
                var result = ProcessTargetObject(next);

                _StreamManager.Output(result);
            }
        }

        public void Process(PSObject targetObject)
        {
            _StreamManager.Process(targetObject);

            while (_StreamManager.Next(out PSObject next))
            {
                var result = ProcessTargetObject(next);

                _StreamManager.Output(result);
            }
        }

        public void End()
        {
            _StreamManager.End(_Summary.Values.Where(r => _Outcome == RuleOutcome.All || (r.Outcome & _Outcome) > 0));
        }

        private InvokeResult ProcessTargetObject(PSObject targetObject)
        {
            _Context.SetTargetObject(targetObject: targetObject);

            var result = new InvokeResult(_Context.TargetName);

            var ruleCounter = 0;

            // Process rule blocks ordered by dependency graph
            foreach (var ruleBlockTarget in _RuleGraph.GetSingleTarget())
            {
                // Enter rule block scope
                var ruleRecord = _Context.EnterRuleBlock(ruleBlock: ruleBlockTarget.Value);
                ruleCounter++;

                try
                {
                    // Check if dependency failed
                    if (ruleBlockTarget.Skipped)
                    {
                        ruleRecord.OutcomeReason = RuleOutcomeReason.DependencyFail;
                    }
                    // Check for suppression
                    else if (_SuppressionFilter.Match(ruleName: ruleBlockTarget.Value.RuleName, targetName: _Context.TargetName))
                    {
                        ruleRecord.OutcomeReason = RuleOutcomeReason.Suppressed;
                    }
                    else
                    {
                        HostHelper.InvokeRuleBlock(context: _Context, ruleBlock: ruleBlockTarget.Value, ruleRecord: ruleRecord);

                        if (ruleRecord.OutcomeReason == RuleOutcomeReason.PreconditionFail)
                        {
                            ruleCounter--;
                        }
                    }

                    // Report outcome to dependency graph
                    if (ruleRecord.Outcome == RuleOutcome.Pass)
                    {
                        ruleBlockTarget.Pass();
                    }
                    else if (ruleRecord.Outcome == RuleOutcome.Fail || ruleRecord.Outcome == RuleOutcome.Error)
                    {
                        ruleBlockTarget.Fail();
                    }

                    AddToSummary(ruleBlock: ruleBlockTarget.Value, outcome: ruleRecord.Outcome);

                    if (ShouldOutput(ruleRecord.Outcome))
                    {
                        result.Add(ruleRecord);
                    }
                }
                finally
                {
                    // Exit rule block scope
                    _Context.ExitRuleBlock();
                }
            }

            if (ruleCounter == 0)
            {
                _Context.WarnObjectNotProcessed();
            }

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
                _Context.Pass();
            }
            else if (outcome == RuleOutcome.Fail)
            {
                s.Fail++;
                _Context.Fail();
            }
            else if (outcome == RuleOutcome.Error)
            {
                s.Error++;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _RuleGraph.Dispose();
                }

                _Disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
