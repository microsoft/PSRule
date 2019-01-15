using PSRule.Pipeline;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The Exists keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Exists)]
    internal sealed class AssertExistsCommand : RuleKeyword
    {
        public AssertExistsCommand()
        {
            CaseSensitive = false;
            Not = false;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string[] Field { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter Not { get; set; }

        protected override void ProcessRecord()
        {
            var targetObject = GetTargetObject();

            bool expected = !Not;
            bool actual = Not;

            for (var i = 0; i < Field.Length && actual != expected; i++)
            {
                actual = ObjectHelper.GetField(targetObject: targetObject, name: Field[i], caseSensitive: CaseSensitive, value: out object fieldValue);

                if (actual == expected)
                {
                    if (expected)
                    {
                        PipelineContext.CurrentThread.WriteVerbose($"[Exists] -- The field {Field[i]} exists");
                    }
                }
            }

            if (!actual)
            {
                PipelineContext.CurrentThread.WriteVerbose($"[Exists] -- The field(s) {string.Join(", ", Field)} do not exist");
            }

            WriteObject(expected == actual);
        }
    }
}
