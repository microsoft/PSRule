using PSRule.Pipeline;
using System.Management.Automation;

namespace PSRule.Runtime
{
    /// <summary>
    /// A set of rule properties that are exposed at runtime through the $Rule variable.
    /// </summary>
    public sealed class Rule
    {
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
