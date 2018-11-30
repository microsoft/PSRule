using PSRule.Configuration;
using PSRule.Host;
using PSRule.Rules;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipeline : RulePipeline
    {
        private readonly RuleResultOutcome _Outcome;
        private readonly DependencyGraph<RuleBlock> _RuleGraph;
        private readonly PipelineContext _Context;

        internal InvokeRulePipeline(PipelineLogger logger, PSRuleOption option, string[] path, RuleFilter filter, RuleResultOutcome outcome)
            : base(option, path, filter)
        {
            _Outcome = outcome;
            _Context = PipelineContext.New(logger);
            _RuleGraph = HostHelper.GetRuleBlockGraph(_Option, null, _Path, _Filter);
        }

        public IEnumerable<RuleResult> Process(PSObject o)
        {
            try
            {
                var results = new List<RuleResult>();

                foreach (var target in _RuleGraph.GetSingleTarget())
                {
                    var result = (target.Skipped) ? new RuleResult(target.Value.Id) : HostHelper.InvokeRuleBlock(_Option, target.Value, o);

                    if (result.Status == RuleResultOutcome.Passed || result.Status == RuleResultOutcome.Inconclusive)
                    {
                        target.Pass();
                    }
                    else if (result.Status == RuleResultOutcome.Failed || result.Status == RuleResultOutcome.Error)
                    {
                        target.Fail();
                    }

                    if (ShouldOutput(result.Status))
                    {
                        results.Add(result);
                    }
                }

                return results;
            }
            finally
            {
                
            }
        }

        private bool ShouldOutput(RuleResultOutcome outcome)
        {
            return _Outcome == RuleResultOutcome.All | (outcome & _Outcome) > 0;
        }
    }
}
