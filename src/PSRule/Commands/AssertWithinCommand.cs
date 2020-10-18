// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;
using System;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Commands
{
    /// <summary>
    /// The Within keyword.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Assert, RuleLanguageNouns.Within)]
    internal sealed class AssertWithinCommand : RuleKeyword
    {
        private StringComparer _Comparer;
        private WildcardPattern[] _LikePattern;

        public AssertWithinCommand()
        {
            CaseSensitive = false;
            Like = false;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string Field { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        [Alias("AllowedValue")]
        [AllowNull()]
        public PSObject[] Value { get; set; }

        [Parameter(Mandatory = false)]
        public string Reason { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter Not { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter Like { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void BeginProcessing()
        {
            _Comparer = CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            BuildPattern();
        }

        protected override void ProcessRecord()
        {
            if (!IsRuleScope())
                throw RuleScopeException(LanguageKeywords.Within);

            var targetObject = InputObject ?? GetTargetObject();
            bool expected = !Not;
            bool match = false;
            string found = string.Empty;

            // Pass with any match, or (-Not) fail with any match

            if (ObjectHelper.GetField(bindingContext: PipelineContext.CurrentThread, targetObject: targetObject, name: Field, caseSensitive: false, value: out object fieldValue))
            {
                for (var i = 0; (Value == null || i < Value.Length) && !match; i++)
                {
                    // Null compare
                    if (fieldValue == null || Value == null || Value[i] == null)
                    {
                        if (fieldValue == null && (Value == null || Value[i] == null))
                        {
                            match = true;
                            RunspaceContext.CurrentThread.VerboseConditionMessage(condition: RuleLanguageNouns.Within, message: PSRuleResources.WithinTrue, args: fieldValue);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // String compare
                    else if (fieldValue is string strValue && Value[i].BaseObject is string)
                    {
                        if ((_LikePattern == null && _Comparer.Equals(Value[i].BaseObject, strValue)) || (_LikePattern != null && _LikePattern[i].IsMatch(strValue)))
                        {
                            match = true;
                            RunspaceContext.CurrentThread.VerboseConditionMessage(condition: RuleLanguageNouns.Within, message: PSRuleResources.WithinTrue, args: strValue);
                            found = Value[i].BaseObject.ToString();
                        }
                    }
                    // Everything else
                    else if (Value[i].Equals(fieldValue))
                    {
                        match = true;
                        RunspaceContext.CurrentThread.VerboseConditionMessage(condition: RuleLanguageNouns.Within, message: PSRuleResources.WithinTrue, args: fieldValue);
                        found = Value[i].ToString();
                    }
                }
            }

            var result = expected == match;
            RunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.Within, outcome: result);
            if (!(result || TryReason(Reason)))
            {
                WriteReason(Not ? string.Format(Thread.CurrentThread.CurrentCulture, ReasonStrings.WithinNot, found) : ReasonStrings.Within);
            }
            WriteObject(result);
        }

        private void BuildPattern()
        {
            if (!Like || Value.Length == 0)
                return;

            if (TryExpressionCache())
                return;

            _LikePattern = new WildcardPattern[Value.Length];
            for (var i = 0; i < _LikePattern.Length; i++)
            {
                if (!TryStringValue(Value[i], out string value))
                {
                    throw new RuleException(PSRuleResources.WithinLikeNotString);
                }
                _LikePattern[i] = WildcardPattern.Get(value, CaseSensitive ? WildcardOptions.None : WildcardOptions.IgnoreCase);
            }
            PipelineContext.CurrentThread.ExpressionCache[MyInvocation.PositionMessage] = _LikePattern;
        }

        private bool TryExpressionCache()
        {
            if (!PipelineContext.CurrentThread.ExpressionCache.TryGetValue(MyInvocation.PositionMessage, out object cacheValue))
                return false;

            _LikePattern = (WildcardPattern[])cacheValue;
            return true;
        }

        private static bool TryStringValue(PSObject o, out string value)
        {
            value = null;
            if (o == null || !(o.BaseObject is string))
                return false;
            value = o.BaseObject.ToString();
            return true;
        }
    }
}
