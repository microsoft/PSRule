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
        private readonly DependencyGraph<RuleBlock> _RuleGraph;

        // A per rule summary of rules that have been processed and the outcome
        private readonly Dictionary<string, RuleSummaryRecord> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly RuleSuppressionFilter _SuppressionFilter;

        internal InvokeRulePipeline(PSRuleOption option, string[] path, RuleFilter filter, RuleOutcome outcome, ResultFormat resultFormat, PipelineContext context)
            : base(context, option, path, filter)
        {
            _Outcome = outcome;
            _RuleGraph = HostHelper.GetRuleBlockGraph(_Option, _Path, _Filter);
            _Summary = new Dictionary<string, RuleSummaryRecord>();
            _ResultFormat = resultFormat;
            _SuppressionFilter = new RuleSuppressionFilter(_Option.Suppression);
            RuleCount = _RuleGraph.Count;

            if (RuleCount == 0)
            {
                _Context.WarnRuleNotFound();
            }
        }

        public int RuleCount { get; private set; }

        public InvokeResult Process(PSObject targetObject)
        {
            return ProcessTargetObject(targetObject);
        }

        public IEnumerable<InvokeResult> Process(PSObject[] targetObjects)
        {
            var results = new List<InvokeResult>();

            foreach (var targetObject in targetObjects)
            {
                results.Add(ProcessTargetObject(targetObject));
            }

            return results;
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
            _Context.TargetObject(targetObject);

            var result = new InvokeResult();

            var ruleCounter = 0;

            // Process rule blocks ordered by dependency graph
            foreach (var ruleBlockTarget in _RuleGraph.GetSingleTarget())
            {
                // Enter rule block scope
                _Context.Enter(ruleBlockTarget.Value);
                ruleCounter++;

                try
                {
                    var ruleRecord = new RuleRecord(
                        ruleId: ruleBlockTarget.Value.RuleId,
                        ruleName: ruleBlockTarget.Value.RuleName,
                        targetObject: targetObject,
                        targetName: _Context.TargetName,
                        tag: ruleBlockTarget.Value.Tag
                    );

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
                    _Context.Exit();
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
    }
}
