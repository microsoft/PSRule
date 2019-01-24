using PSRule.Pipeline;
using System.Dynamic;
using System.Management.Automation;

namespace PSRule.Host
{
    /// <summary>
    /// A set of rule configuration values that are exposed at runtime and automatically failback to defaults when not set in configuration.
    /// </summary>
    public sealed class RuntimeRuleConfigurationView : DynamicObject
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // Get from baseline configuration
            if (PipelineContext.CurrentThread.Option.Baseline.Configuration.TryGetValue(binder.Name, out object value))
            {
                result = value;
                return true;
            }

            // Check if value exists in Rule definition defaults
            if (PipelineContext.CurrentThread.RuleBlock == null || PipelineContext.CurrentThread.RuleBlock.Configuration == null || !PipelineContext.CurrentThread.RuleBlock.Configuration.ContainsKey(binder.Name))
            {
                result = null;
                return false;
            }

            // Get from rule default
            result = PipelineContext.CurrentThread.RuleBlock.Configuration[binder.Name];
            return true;
        }
    }

    /// <summary>
    /// A set of rule properties that are exposed at runtime through the $Rule variable.
    /// </summary>
    public sealed class RuntimeRuleView
    {
        public RuntimeRuleView()
        {
            Configuration = new RuntimeRuleConfigurationView();
        }

        public readonly RuntimeRuleConfigurationView Configuration;

        public string RuleName
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.RuleName;
            }
        }

        public string RuleId
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.RuleId;
            }
        }

        public PSObject TargetObject
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetObject;
            }
        }

        public string TargetName
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetName;
            }
        }
    }
}
