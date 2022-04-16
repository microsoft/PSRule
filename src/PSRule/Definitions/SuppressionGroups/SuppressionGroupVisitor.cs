// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Definitions.SuppressionGroups
{
    internal sealed class SuppressionGroupVisitor
    {
        private readonly LanguageExpressionOuterFn _Fn;

        public ResourceId Id { get; }

        public SourceFile Source { get; }

        public Guid InstanceId { get; }

        public string[] Rule { get; }

        public SuppressionGroupVisitor(ResourceId id, SourceFile source, ISuppressionGroupSpec spec)
        {
            Id = id;
            Source = source;
            InstanceId = Guid.NewGuid();
            Rule = spec.Rule;
            var builder = new LanguageExpressionBuilder();
            _Fn = builder
                .WithRule(Rule)
                .Build(spec.If);
        }

        public bool Match(object o)
        {
            var context = new ExpressionContext(Source);
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            return _Fn(context, o).GetValueOrDefault(false);
        }
    }
}