using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public sealed class InvokeRulePipelineBuilder
    {
        private string[] _Path;
        private PSRuleOption _Option;
        private RuleFilter _Filter;
        private RuleResultOutcome _Outcome;
        private PipelineLogger _Logger;

        internal InvokeRulePipelineBuilder()
        {
            _Logger = new PipelineLogger();
        }

        public void FilterBy(string[] name, Hashtable tag)
        {
            _Filter = new RuleFilter(name, tag);
        }

        public void Source(string[] path)
        {
            _Path = path;
        }

        public void Option(PSRuleOption option)
        {
            _Option = option;
        }

        public void Limit(RuleResultOutcome outcome)
        {
            _Outcome = outcome;
        }

        //public void WriteVerbose(ActionPreference preference, Func<string> callback)
        //{

        //}

        //public void WriteError(ActionPreference preference, Func<ErrorRecord> callback)
        //{

        //}

        //public void WriteWarning(ActionPreference preference, Func<string> callback)
        //{

        //}

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
            return new InvokeRulePipeline(_Logger, _Option, _Path, _Filter, _Outcome);
        }
    }
}
