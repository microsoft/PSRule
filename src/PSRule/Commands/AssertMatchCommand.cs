// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Text.RegularExpressions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Commands;

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
        Not = false;
    }

    [Parameter(Mandatory = true, Position = 0)]
    public string Field { get; set; }

    [Parameter(Mandatory = true, Position = 1)]
    public string[] Expression { get; set; }

    [Parameter(Mandatory = false)]
    public string Reason { get; set; }

    [Parameter(Mandatory = false)]
    [PSDefaultValue(Value = false)]
    public SwitchParameter CaseSensitive { get; set; }

    [Parameter(Mandatory = false)]
    [PSDefaultValue(Value = false)]
    public SwitchParameter Not { get; set; }

    [Parameter(Mandatory = false, ValueFromPipeline = true)]
    public PSObject InputObject { get; set; }

    protected override void BeginProcessing()
    {
        // Setup regex expressions
        _Expressions = new Regex[Expression.Length];
        var regexOption = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

        for (var i = 0; i < _Expressions.Length; i++)
            _Expressions[i] = new Regex(Expression[i], regexOption, TimeSpan.FromSeconds(5));
    }

    protected override void ProcessRecord()
    {
        if (!IsRuleScope())
            throw RuleScopeException(LanguageKeywords.Match);

        var targetObject = InputObject ?? GetTargetObjectValue();
        var expected = !Not;
        var match = false;
        var found = string.Empty;

        // Pass with any match, or (-Not) fail with any match

        if (ObjectHelper.GetPath(
            bindingContext: PipelineContext.CurrentThread,
            targetObject: targetObject,
            path: Field,
            caseSensitive: false,
            value: out object fieldValue))
        {
            var s = fieldValue.ToString();
            for (var i = 0; i < _Expressions.Length && !match; i++)
            {
                if (_Expressions[i].IsMatch(s))
                {
                    match = true;
                    LegacyRunspaceContext.CurrentThread.VerboseConditionMessage(
                        condition: RuleLanguageNouns.Match,
                        message: PSRuleResources.MatchTrue,
                        args: fieldValue);
                    found = Expression[i];
                }
            }
        }

        var result = expected == match;
        LegacyRunspaceContext.CurrentThread.VerboseConditionResult(condition: RuleLanguageNouns.Match, outcome: result);
        if (!(result || TryReason(null, Reason, null)))
        {
            WriteReason(
                path: null,
                text: Not ? ReasonStrings.MatchNot : ReasonStrings.Match,
                args: Not ? found : string.Join(", ", Expression)
            );
        }
        WriteObject(result);
    }
}
