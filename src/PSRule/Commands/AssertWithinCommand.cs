using System;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// The Within keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Within)]
    internal sealed class AssertWithinCommand : RuleKeyword
    {
        private StringComparer _Comparer;

        public AssertWithinCommand()
        {
            CaseSensitive = false;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string Field { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public PSObject[] List { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter CaseSensitive { get; set; }

        protected override void BeginProcessing()
        {
            _Comparer = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        }

        protected override void ProcessRecord()
        {
            var inputObject = GetVariableValue("InputObject") ?? GetVariableValue("TargetObject");

            var result = false;

            if (GetField(inputObject, Field, out object fieldValue))
            {
                foreach (var item in List)
                {
                    if (fieldValue is string && item.BaseObject is string)
                    {
                        if (_Comparer.Equals(fieldValue, item.BaseObject))
                        {
                            result = true;
                        }
                    }
                    else if (item == fieldValue)
                    {
                        result = true;
                    }
                }
            }

            WriteObject(result);
        }
    }
}
