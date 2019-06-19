using PSRule.Pipeline;
using PSRule.Resources;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSRule.Commands
{
    internal sealed class RuleLanguageAst : AstVisitor
    {
        private readonly string _RuleName;
        private readonly StringComparer _Comparer;

        internal List<ErrorRecord> Errors;

        internal RuleLanguageAst(string ruleName, PipelineContext context)
        {
            _RuleName = ruleName;
            _Comparer = StringComparer.OrdinalIgnoreCase;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            string commandName = commandAst.GetCommandName();

            if (_Comparer.Compare(commandName, "Rule") == 0)
            {
                ReportError(message: string.Format(PSRuleResources.InvalidRuleNesting, _RuleName), errorId: "PSRule.Runtime.InvalidRuleNesting");
            }

            return base.VisitCommand(commandAst);
        }

        private void ReportError(string message, string errorId)
        {
            if (Errors == null)
            {
                Errors = new List<ErrorRecord>();
            }

            Errors.Add(new ErrorRecord(
                exception: new RuleRuntimeException(message: message),
                errorId: errorId,
                errorCategory: ErrorCategory.InvalidOperation,
                targetObject: null
            ));
        }
    }
}
