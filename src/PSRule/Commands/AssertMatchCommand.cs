using PSRule.Pipeline;
using PSRule.Runtime;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PSRule.Commands
{
    /// <summary>
    /// The Match keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Match)]
    internal sealed class AssertMatchCommand : RuleKeyword
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
            var targetObject = GetTargetObject();

            var result = false;

            if (ObjectHelper.GetField(targetObject: targetObject, name: Field, caseSensitive: false, value: out object fieldValue))
            {
                for (var i = 0; i < _Expressions.Length && !result; i++)
                {
                    if (_Expressions[i].IsMatch(fieldValue.ToString()))
                    {
                        result = true;
                    }
                }
            }

            PipelineContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.Match, outcome: result);

            WriteObject(result);
        }
    }
}
