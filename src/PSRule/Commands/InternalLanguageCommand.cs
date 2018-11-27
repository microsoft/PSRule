using PSRule.Rules;
using System.Management.Automation;

namespace PSRule.Commands
{
    public abstract class InternalLanguageCommand : PSCmdlet
    {

        protected RuleResult GetResult()
        {
            return GetVariableValue("Rule") as RuleResult;
        }
    }
}