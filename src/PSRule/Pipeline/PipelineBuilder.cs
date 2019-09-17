using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public static class PipelineBuilder
    {
        public static InvokeRulePipelineBuilder Invoke()
        {
            return new InvokeRulePipelineBuilder();
        }

        public static GetRulePipelineBuilder Get()
        {
            return new GetRulePipelineBuilder();
        }

        public static GetRuleHelpPipelineBuilder GetHelp()
        {
            return new GetRuleHelpPipelineBuilder();
        }

        public static RuleSourceBuilder RuleSource()
        {
            return new RuleSourceBuilder();
        }

        public static GetBaselinePipelineBuilder GetBaseline()
        {
            return new GetBaselinePipelineBuilder();
        }
    }

    public abstract class PipelineBuilderBase
    {
        private readonly PipelineLogger _Logger;
        protected readonly PSRuleOption _Option;
        private Source[] _Source;
        private string[] _Include;
        private Hashtable _Tag;
        private BaselineOption _Baseline;

        protected PipelineBuilderBase()
        {
            _Logger = new PipelineLogger();
            _Option = new PSRuleOption();
        }

        public void Source(Source[] source)
        {
            _Source = source;
        }

        public void Name(string[] name)
        {
            if (name == null || name.Length == 0)
                return;

            _Include = name;
        }

        public void Tag(Hashtable tag)
        {
            if (tag == null || tag.Count == 0)
                return;

            _Tag = tag;
        }

        public virtual void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            _Logger.UseCommandRuntime(commandRuntime);
        }

        public void UseExecutionContext(EngineIntrinsics executionContext)
        {
            _Logger.UseExecutionContext(executionContext);
        }

        protected void ConfigureLogger(PSRuleOption option)
        {
            _Logger.Configure(option);
        }

        protected Source[] GetSource()
        {
            return _Source;
        }

        /// <summary>
        /// Use a baseline, either by name or by path.
        /// </summary>
        public void UseBaseline(BaselineOption baseline)
        {
            if (baseline == null)
                return;

            _Baseline = baseline;
        }

        internal PipelineContext PrepareContext(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType)
        {
            var unresolved = new Dictionary<string, ResourceRef>(StringComparer.OrdinalIgnoreCase);
            if (_Baseline != null && _Baseline is BaselineOption.BaselineRef baselineRef)
                unresolved.Add(baselineRef.Name, new BaselineRef(baselineRef.Name, BaselineContext.ScopeType.Explicit));

            for (var i = 0; i < _Source.Length; i++)
            {
                if (_Source[i].Module != null && _Source[i].Module.Baseline != null)
                    unresolved.Add(_Source[i].Module.Baseline, new BaselineRef(_Source[i].Module.Baseline, BaselineContext.ScopeType.Module));
            }

            return PipelineContext.New(
                logger: _Logger,
                option: _Option,
                binder: new TargetBinder(bindTargetName: bindTargetName, bindTargetType: bindTargetType),
                baseline: GetBaselineContext(),
                unresolved: unresolved
            );
        }

        private BaselineContext GetBaselineContext()
        {
            var result = new BaselineContext();
            var scope = new BaselineContext.BaselineContextScope(type: BaselineContext.ScopeType.Workspace, moduleName: null, option: _Option);
            result.Add(scope);

            scope = new BaselineContext.BaselineContextScope(type: BaselineContext.ScopeType.Parameter, include: _Include, tag: _Tag);
            result.Add(scope);

            return result;
        }
    }
}
