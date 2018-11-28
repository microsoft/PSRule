using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSRule.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, "Match")]
    internal sealed class AssertMatchCommand : InternalLanguageCommand
    {
        private Regex[] _Expressions;

        public AssertMatchCommand()
        {
            CaseSensitive = false;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string Field { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string[] Expression { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter CaseSensitive { get; set; }

        protected override void BeginProcessing()
        {
            // Setup regex expressions
            _Expressions = new Regex[Expression.Length];
            var regexOption = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            for (var i = 0; i < _Expressions.Length; i++)
            {
                _Expressions[i] = new Regex(Expression[i], regexOption);
            }
        }

        protected override void ProcessRecord()
        {
            var inputObject = GetVariableValue("InputObject") ?? GetVariableValue("TargetObject");

            var result = false;

            if (GetField(inputObject, Field, out object fieldValue))
            {
                for (var i = 0; i < _Expressions.Length && !result; i++)
                {
                    if (_Expressions[i].IsMatch(fieldValue.ToString()))
                    {
                        result = true;
                    }
                }
            }

            WriteObject(result);
        }
    }
}
