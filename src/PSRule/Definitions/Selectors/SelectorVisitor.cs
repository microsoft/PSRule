// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Expressions;
using PSRule.Resources;
using System;
using System.Diagnostics;

namespace PSRule.Definitions.Selectors
{
    [DebuggerDisplay("Id: {Id}")]
    internal sealed class SelectorVisitor
    {
        private readonly LanguageExpressionOuterFn _Fn;

        public SelectorVisitor(string id, LanguageIf expression)
        {
            Id = id;
            InstanceId = Guid.NewGuid();
            var builder = new LanguageExpressionBuilder();
            _Fn = builder.Build(expression);
        }

        public Guid InstanceId { get; }

        public string Id { get; }

        public bool Match(object o)
        {
            var context = new ExpressionContext();
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            return _Fn(context, o).GetValueOrDefault(false);
        }
    }
}
