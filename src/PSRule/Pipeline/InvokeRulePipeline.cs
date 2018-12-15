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

        // A per summary of rules being processes and outcome
        private readonly Dictionary<string, RuleSummaryRecord> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly PipelineContext _Context;

        internal InvokeRulePipeline(PipelineLogger logger, PSRuleOption option, string[] path, RuleFilter filter, RuleOutcome outcome, ResultFormat resultFormat)
            : base(option, path, filter)
        {
            _Outcome = outcome;
            _Context = PipelineContext.New(logger);
            _RuleGraph = HostHelper.GetRuleBlockGraph(_Option, null, _Path, _Filter);
            _Summary = new Dictionary<string, RuleSummaryRecord>();
            _ResultFormat = resultFormat;
        }

        public IEnumerable<RuleRecord> Process(PSObject targetObject)
        {
            _Context.Next();
            return ProcessRule(targetObject);
        }

        public IEnumerable<RuleRecord> Process(PSObject[] targetObjects)
        {
            var results = new List<RuleRecord>();

            foreach (var targetObject in targetObjects)
            {
                _Context.Next();

                foreach (var result in Process(targetObject))
                {
                    results.Add(result);
                }
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

        private IEnumerable<RuleRecord> ProcessRule(PSObject targetObject)
        {
            var results = new List<RuleRecord>();

            foreach (var target in _RuleGraph.GetSingleTarget())
            {
                _Context.Enter(target);

                try
                {
                    var result = (target.Skipped) ? new RuleRecord(target.Value.RuleId, reason: RuleOutcomeReason.DependencyFail) : HostHelper.InvokeRuleBlock(_Option, target.Value, targetObject);

                    if (result.Outcome == RuleOutcome.Pass)
                    {
                        target.Pass();
                    }
                    else if (result.Outcome == RuleOutcome.Fail || result.Outcome == RuleOutcome.Error)
                    {
                        target.Fail();
                    }

                    AddToSummary(ruleBlock: target.Value, targetName: result.TargetName, outcome: result.Outcome);

                    if (ShouldOutput(result.Outcome))
                    {
                        results.Add(result);
                    }
                }
                finally
                {
                    _Context.Exit();
                }
            }

            return results;
        }

        private bool ShouldOutput(RuleOutcome outcome)
        {
            return _ResultFormat == ResultFormat.Detail &&
                (_Outcome == RuleOutcome.All || (outcome & _Outcome) > 0);
        }

        private void AddToSummary(RuleBlock ruleBlock, string targetName, RuleOutcome outcome)
        {
            if (!_Summary.TryGetValue(ruleBlock.RuleId, out RuleSummaryRecord s))
            {
                s = new RuleSummaryRecord(ruleBlock.RuleId);
                s.Tag = ruleBlock.Tag?.ToHashtable();

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
