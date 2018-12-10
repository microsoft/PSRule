using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;
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

        public IEnumerable<RuleRecord> Process(PSObject o)
        {
            try
            {
                return ProcessRule(o);
            }
            finally
            {
                
            }
        }

        public IEnumerable<RuleRecord> Process(PSObject[] targets)
        {
            var results = new List<RuleRecord>();

            foreach (var target in targets)
            {
                foreach (var result in Process(target))
                {
                    results.Add(result);
                }
            }

            return results;
        }

        public IEnumerable<RuleSummaryRecord> GetSummary()
        {
            return _Summary.Values;
        }

        private IEnumerable<RuleRecord> ProcessRule(PSObject o)
        {
            var results = new List<RuleRecord>();

            foreach (var target in _RuleGraph.GetSingleTarget())
            {
                var result = (target.Skipped) ? new RuleRecord(target.Value.Id) : HostHelper.InvokeRuleBlock(_Option, target.Value, o);

                if (result.Status == RuleOutcome.Passed || result.Status == RuleOutcome.Inconclusive)
                {
                    target.Pass();
                }
                else if (result.Status == RuleOutcome.Failed || result.Status == RuleOutcome.Error)
                {
                    target.Fail();
                }

                AddToSummary(ruleBlock: target.Value, targetName: result.TargetName, outcome: result.Status);

                if (ShouldOutput(result.Status))
                {
                    results.Add(result);
                }
            }

            return results;
        }

        private bool ShouldOutput(RuleOutcome outcome)
        {
            return _ResultFormat == ResultFormat.Detail &&
                (_Outcome == RuleOutcome.All | (outcome & _Outcome) > 0);
        }

        private void AddToSummary(RuleBlock ruleBlock, string targetName, RuleOutcome outcome)
        {
            if (!_Summary.TryGetValue(ruleBlock.Id, out RuleSummaryRecord s))
            {
                s = new RuleSummaryRecord(ruleBlock.Id);
                s.Tag = ruleBlock.Tag?.ToHashtable();

                _Summary.Add(ruleBlock.Id, s);
            }

            if (outcome == RuleOutcome.Passed)
            {
                s.Pass++;
            }
            else if (outcome == RuleOutcome.Failed)
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
