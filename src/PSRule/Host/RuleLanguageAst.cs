// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Language;
using PSRule.Definitions;
using PSRule.Runtime;

namespace PSRule.Host;

internal sealed class RuleLanguageAst(ILogger? logger) : AstVisitor
{
    private const string PARAMETER_NAME = "Name";
    private const string PARAMETER_REF = "Ref";
    private const string PARAMETER_ALIAS = "Alias";
    private const string PARAMETER_BODY = "Body";
    private const string PARAMETER_ERRORACTION = "ErrorAction";
    private const string RULE_KEYWORD = "Rule";

    private readonly StringComparer _Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly ILogger? _Logger = logger;

    public bool HadErrors { get; private set; }

    private sealed class ParameterBindResult
    {
        public ParameterBindResult()
        {
            Bound = new Dictionary<string, CommandElementAst>(StringComparer.OrdinalIgnoreCase);
            Unbound = new List<CommandElementAst>();
            _Offset = 0;
        }

        public Dictionary<string, CommandElementAst> Bound;
        public List<CommandElementAst> Unbound;

        private int _Offset;

        public bool Has<TAst>(string parameterName, out TAst parameterValue) where TAst : CommandElementAst
        {
            var result = Bound.TryGetValue(parameterName, out var value) && value is TAst;
            parameterValue = result ? value as TAst : null;
            return result;
        }

        public bool Has<TAst>(string parameterName, int position, out TAst value) where TAst : CommandElementAst
        {
            // Try bound
            if (Has<TAst>(parameterName, out value))
            {
                _Offset++;
                return true;
            }
            var relative = position - _Offset;
            var result = Unbound.Count > relative && relative >= 0 && position >= 0 && Unbound[relative] is TAst;
            value = result ? Unbound[relative] as TAst : null;
            return result;
        }
    }

    public override AstVisitAction VisitCommand(CommandAst commandAst)
    {
        return IsRule(commandAst) ? VisitRule(commandAst) : base.VisitCommand(commandAst);
    }

    /// <summary>
    /// Visit a rule.
    /// </summary>
    private AstVisitAction VisitRule(CommandAst commandAst)
    {
        return NotNested(commandAst) &&
            TryBindParameters(commandAst, out var bindResult) &&
            VisitNameParameter(commandAst, bindResult) &&
            VisitBodyParameter(commandAst, bindResult) &&
            VisitRefParameter(commandAst, bindResult) &&
            VisitAliasParameter(commandAst, bindResult) &&
            VisitErrorAction(commandAst, bindResult) ? base.VisitCommand(commandAst) : AstVisitAction.SkipChildren;
    }

    /// <summary>
    /// Determines if the rule has a Body parameter.
    /// </summary>
    private bool VisitBodyParameter(CommandAst commandAst, ParameterBindResult bindResult)
    {
        if (bindResult.Has(PARAMETER_BODY, 1, out ScriptBlockExpressionAst _))
            return true;

        HadErrors = true;
        _Logger?.LogRuleParameterNotFound(PARAMETER_BODY, ReportExtent(commandAst.Extent));
        return false;
    }

    /// <summary>
    /// Determines if the rule has a Name parameter.
    /// </summary>
    private bool VisitNameParameter(CommandAst commandAst, ParameterBindResult bindResult)
    {
        if (bindResult.Has(PARAMETER_NAME, 0, out StringConstantExpressionAst value))
            return IsNameValid(value);

        HadErrors = true;
        _Logger?.LogRuleParameterNotFound(PARAMETER_NAME, ReportExtent(commandAst.Extent));
        return false;
    }

    private bool VisitRefParameter(CommandAst commandAst, ParameterBindResult bindResult)
    {
        if (!bindResult.Has(PARAMETER_REF, -1, out StringConstantExpressionAst value))
            return true;

        return IsNameValid(value);
    }

    private bool VisitAliasParameter(CommandAst commandAst, ParameterBindResult bindResult)
    {
        if (!bindResult.Has(PARAMETER_ALIAS, -1, out ArrayLiteralAst value) || value.Elements.Count == 0)
            return true;

        return IsNameValid(value);
    }

    /// <summary>
    /// Determines if the rule name is valid.
    /// </summary>
    private bool IsNameValid(StringConstantExpressionAst name)
    {
        if (ResourceValidator.IsNameValid(name.Value))
            return true;

        HadErrors = true;
        _Logger?.LogInvalidResourceName(name.Value, ReportExtent(name.Extent));
        return false;
    }

    private bool IsNameValid(ArrayLiteralAst arrayAst)
    {
        for (var i = 0; i < arrayAst.Elements.Count; i++)
            if (arrayAst.Elements[i] == null || (arrayAst.Elements[i] is StringConstantExpressionAst value && IsNameValid(value)))
                return false;

        return true;
    }

    /// <summary>
    /// Determines if the rule is nested in another rule.
    /// </summary>
    private bool NotNested(CommandAst commandAst)
    {
        if (GetParentBlock(commandAst)?.Parent == null)
            return true;

        HadErrors = true;
        _Logger?.LogInvalidRuleNesting(ReportExtent(commandAst.Extent));
        return false;
    }

    /// <summary>
    /// Determine if the rule has allowed ErrorAction options.
    /// </summary>
    private bool VisitErrorAction(CommandAst commandAst, ParameterBindResult bindResult)
    {
        if (!bindResult.Has(PARAMETER_ERRORACTION, -1, out StringConstantExpressionAst value))
            return true;

        if (!Enum.TryParse(value.Value, out ActionPreference result) || (result == ActionPreference.Ignore || result == ActionPreference.Stop))
            return true;

        HadErrors = true;
        _Logger?.LogInvalidErrorAction(value.Value, ReportExtent(commandAst.Extent));
        return false;
    }

    /// <summary>
    /// Determines if the command is a rule definition.
    /// </summary>
    private bool IsRule(CommandAst commandAst)
    {
        return _Comparer.Equals(commandAst.GetCommandName(), RULE_KEYWORD);
    }

    private static bool TryBindParameters(CommandAst commandAst, out ParameterBindResult bindResult)
    {
        bindResult = BindParameters(commandAst);
        return bindResult != null;
    }

    private static ParameterBindResult BindParameters(CommandAst commandAst)
    {
        var result = new ParameterBindResult();
        var i = 1;
        var next = 2;
        for (; i < commandAst.CommandElements.Count; i++, next++)
        {
            // Is named parameter
            if (commandAst.CommandElements[i] is CommandParameterAst parameter && next < commandAst.CommandElements.Count)
            {
                result.Bound.Add(parameter.ParameterName, commandAst.CommandElements[next]);
                i++;
                next++;
            }
            else
            {
                result.Unbound.Add(commandAst.CommandElements[i]);
            }
        }
        return result;
    }

    private static string ReportExtent(IScriptExtent extent)
    {
        return string.Concat(extent.File, " line ", extent.StartLineNumber);
    }

    private static ScriptBlockAst GetParentBlock(Ast ast)
    {
        var block = ast;
        while (block != null && block is not ScriptBlockAst)
            block = block.Parent;

        return (ScriptBlockAst)block;
    }
}
