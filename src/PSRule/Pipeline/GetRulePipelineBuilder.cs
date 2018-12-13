using PSRule.Configuration;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to construct a get pipeline.
    /// </summary>
    public sealed class GetRulePipelineBuilder
    {
        private string[] _Path;
        private PSRuleOption _Option;
        private RuleFilter _Filter;
        private PipelineLogger _Logger;

        internal GetRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
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

        public GetRulePipeline Build()
        {
            return new GetRulePipeline(_Logger, _Option, _Path, _Filter);
        }
    }
}
