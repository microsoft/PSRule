using PSRule.Configuration;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct an invoke pipeline.
    /// </summary>
    public sealed class InvokeRulePipelineBuilder
    {
        private string[] _Path;
        private PSRuleOption _Option;
        private RuleFilter _Filter;
        private RuleOutcome _Outcome;
        private PipelineLogger _Logger;
        private ResultFormat _ResultFormat;

        internal InvokeRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
            _ResultFormat = ResultFormat.Detail;
        }

        public void FilterBy(string[] ruleName, Hashtable tag)
        {
            _Filter = new RuleFilter(ruleName, tag);
        }

        public void Source(string[] path)
        {
            _Path = path;
        }

        public void Option(PSRuleOption option)
        {
            _Option = option.Clone();
        }

        public void Limit(RuleOutcome outcome)
        {
            _Outcome = outcome;
        }

        public void As(ResultFormat resultFormat)
        {
            _ResultFormat = resultFormat;
        }

        public void UseCommandRuntime(ICommandRuntime commandRuntime)
        {
            _Logger.OnWriteVerbose = commandRuntime.WriteVerbose;
            _Logger.OnWriteWarning = commandRuntime.WriteWarning;
            _Logger.OnWriteError = commandRuntime.WriteError;
        }

        public void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.OnWriteVerbose = commandRuntime.WriteVerbose;
            _Logger.OnWriteWarning = commandRuntime.WriteWarning;
            _Logger.OnWriteError = commandRuntime.WriteError;
        }

        public InvokeRulePipeline Build()
        {
            return new InvokeRulePipeline(_Logger, _Option, _Path, _Filter, _Outcome, _ResultFormat);
        }
    }
}
