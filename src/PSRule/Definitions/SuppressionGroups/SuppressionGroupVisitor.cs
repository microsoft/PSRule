// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Definitions.Expressions;
using PSRule.Resources;

namespace PSRule.Definitions.SuppressionGroups
{
    internal sealed class SuppressionGroupVisitor
    {
        private readonly LanguageExpressionOuterFn _Fn;

        public Guid InstanceId { get; }

        public string Module { get; }

        public string Id { get; }

        public SuppressionGroupVisitor(string module, string id, ISuppressionGroupSpec spec)
        {
            Module = module;
            Id = id;
            InstanceId = Guid.NewGuid();
            var builder = new LanguageExpressionBuilder();
            _Fn = builder
                .WithRule(spec.Rule)
                .Build(spec.If);
        }

        public bool Match(object o)
        {
            var context = new ExpressionContext(Module);
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            return _Fn(context, o).GetValueOrDefault(false);
        }
    }
}