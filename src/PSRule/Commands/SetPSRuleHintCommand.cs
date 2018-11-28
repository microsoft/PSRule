using System.Management.Automation;

namespace PSRule.Commands
{
    [Cmdlet(VerbsCommon.Set, "PSRuleHint")]
    internal sealed class SetPSRuleHintCommand : InternalLanguageCommand
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string Message { get; set; }

        [Parameter(Mandatory = false)]
        public string TargetName { get; set; }

        protected override void ProcessRecord()
        {
            var result = GetResult();

            if (MyInvocation.BoundParameters.ContainsKey("Message"))
            {
                result.Message = Message;
            }

            if (MyInvocation.BoundParameters.ContainsKey("TargetName"))
            {
                result.TargetName = TargetName;
            }
        }
    }
}
