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
        private readonly Dictionary<string, SummaryResult> _Summary;

        private readonly ResultFormat _ResultFormat;
        private readonly PipelineContext _Context;

        internal InvokeRulePipeline(PipelineLogger logger, PSRuleOption option, string[] path, RuleFilter filter, RuleOutcome outcome, ResultFormat resultFormat)
            : base(option, path, filter)
        {
            _Outcome = outcome;
            _Context = PipelineContext.New(logger);
            _RuleGraph = HostHelper.GetRuleBlockGraph(_Option, null, _Path, _Filter);
            _Summary = new Dictionary<string, SummaryResult>();
            _ResultFormat = resultFormat;
        }

        public IEnumerable<DetailResult> Process(PSObject o)
        {
            try
            {
                return ProcessRule(o);
            }
            finally
            {
                
            }
        }

        public IEnumerable<DetailResult> Process(PSObject[] targets)
        {
            var results = new List<DetailResult>();

            foreach (var target in targets)
            {
                foreach (var result in Process(target))
                {
                    results.Add(result);
                }
            }

            return results;
        }

        public IEnumerable<SummaryResult> GetSummary()
        {
            return _Summary.Values;
        }

        private IEnumerable<DetailResult> ProcessRule(PSObject o)
        {
            var results = new List<DetailResult>();

            foreach (var target in _RuleGraph.GetSingleTarget())
            {
                var result = (target.Skipped) ? new DetailResult(target.Value.Id) : HostHelper.InvokeRuleBlock(_Option, target.Value, o);

                if (result.Status == RuleOutcome.Passed || result.Status == RuleOutcome.Inconclusive)
                {
                    target.Pass();
                }
                else if (result.Status == RuleOutcome.Failed || result.Status == RuleOutcome.Error)
                {
                    target.Fail();
                }

                AddToSummary(ruleId: result.RuleName, targetName: result.TargetName, outcome: result.Status);

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

        private void AddToSummary(string ruleId, string targetName, RuleOutcome outcome)
        {
            if (!_Summary.TryGetValue(ruleId, out SummaryResult s))
            {
                s = new SummaryResult(ruleId);

                _Summary.Add(ruleId, s);
            }

            if (outcome == RuleOutcome.Passed)
            {
                s.Pass++;
            }
            else if (outcome == RuleOutcome.Failed)
            {
                s.Fail++;
            }
        }
    }
}
