using PSRule.Pipeline;
using PSRule.Runtime;
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
        [Alias("AllowedValue")]
        public PSObject[] Value { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter Not { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void BeginProcessing()
        {
            _Comparer = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        }

        protected override void ProcessRecord()
        {
            var targetObject = InputObject ?? GetTargetObject();
            bool expected = !Not;
            bool match = false;

            // Pass with any match, or (-Not) fail with any match

            if (ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: targetObject, name: Field, caseSensitive: false, value: out object fieldValue))
            {
                for (var i = 0; i < Value.Length && !match; i++)
                {
                    if (fieldValue is string && Value[i].BaseObject is string)
                    {
                        if (_Comparer.Equals(fieldValue, Value[i].BaseObject))
                        {
                            match = true;
                        }
                    }
                    else if (Value[i]?.BaseObject == fieldValue)
                    {
                        match = true;
                    }
                }
            }

            var result = expected == match;

            PipelineContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.Within, outcome: result);
            WriteObject(result);
        }
    }
}
