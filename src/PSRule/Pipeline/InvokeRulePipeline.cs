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
        private readonly PipelineStream _Stream;
        private readonly DependencyGraph<RuleBlock> _RuleGraph;

        // A per rule summary of rules that have been processed and the outcome
        private readonly Dictionary<string, RuleSummaryRecord> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly RuleSuppressionFilter _SuppressionFilter;
        private readonly bool _ReturnBoolean;

        // Track whether Dispose has been called.
        private bool _Disposed = false;

        internal InvokeRulePipeline(PipelineStream stream, PSRuleOption option, string[] path, RuleFilter filter, RuleOutcome outcome, ResultFormat resultFormat, PipelineContext context, bool returnBoolean)
            : base(context, option, path, filter)
        {
            _Stream = stream;
            _RuleGraph = HostHelper.GetRuleBlockGraph(_Option, _Path, _Filter);
            RuleCount = _RuleGraph.Count;

            if (RuleCount == 0)
            {
                _Context.WarnRuleNotFound();
            }

            _Outcome = outcome;
            _Summary = new Dictionary<string, RuleSummaryRecord>();
            _ResultFormat = resultFormat;
            _SuppressionFilter = new RuleSuppressionFilter(option.Suppression);
            _ReturnBoolean = returnBoolean;
        }

        public int RuleCount { get; private set; }

        public IPipelineStream GetStream()
        {
            return _Stream;
        }

        public void Process(PSObject[] targetObjects)
        {
            foreach (var targetObject in targetObjects)
            {
                _Stream.Process(targetObject);
            }

            while (_Stream.Next(out PSObject next))
            {
                var result = ProcessTargetObject(next);

                if (_ReturnBoolean)
                {
                    _Stream.Output(result.AsBoolean(), false);
                }
                else
                {
                    _Stream.Output(result.AsRecord(), true);
                }
            }
        }

        public void Process(PSObject targetObject)
        {
            _Stream.Process(targetObject);

            while (_Stream.Next(out PSObject next))
            {
                var result = ProcessTargetObject(next);

                if (_ReturnBoolean)
                {
                    _Stream.Output(result.AsBoolean(), false);
                }
                else
                {
                    _Stream.Output(result.AsRecord(), true);
                }
            }
        }

        public IEnumerable<RuleSummaryRecord> GetSummary()
        {
            foreach (var s in _Summary.Values.ToArray())
            {
                if (_Outcome == RuleOutcome.All || (s.Outcome & _Outcome) > 0)
                {
                    yield return s;
                }
            }
        }

        private InvokeResult ProcessTargetObject(PSObject targetObject)
        {
            _Context.SetTargetObject(targetObject: targetObject);

            var result = new InvokeResult();

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

                    AddToSummary(ruleBlock: ruleBlockTarget.Value, targetName: ruleRecord.TargetName, outcome: ruleRecord.Outcome);

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
        private void AddToSummary(RuleBlock ruleBlock, string targetName, RuleOutcome outcome)
        {
            if (!_Summary.TryGetValue(ruleBlock.RuleId, out RuleSummaryRecord s))
            {
                s = new RuleSummaryRecord(ruleBlock.RuleId, ruleBlock.RuleName)
                {
                    Tag = ruleBlock.Tag?.ToHashtable()
                };

                _Summary.Add(ruleBlock.RuleId, s);
            }

            if (outcome == RuleOutcome.Pass)
            {
                s.Pass++;
            }
            else if (outcome == RuleOutcome.Fail)
            {
                s.Fail++;
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
