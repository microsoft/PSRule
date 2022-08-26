// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.SuppressionGroups
{
    internal sealed class SuppressionGroupVisitor
    {
        private readonly LanguageExpressionOuterFn _Fn;
        private readonly SuppressionInfo _Info;
        private readonly RunspaceContext _Context;

        public SuppressionGroupVisitor(RunspaceContext context, ResourceId id, SourceFile source, ISuppressionGroupSpec spec, IResourceHelpInfo info)
        {
            _Context = context;
            Id = id;
            Source = source;
            InstanceId = Guid.NewGuid();
            Rule = spec.Rule;
            _Info = new SuppressionInfo(id, info);
            _Fn = new LanguageExpressionBuilder()
                .WithRule(Rule)
                .Build(spec.If);
        }

        /// <summary>
        /// Tracking information about the suppression.
        /// </summary>
        private sealed class SuppressionInfo : ISuppressionInfo
        {
            private readonly IResourceHelpInfo _Info;

            public SuppressionInfo(ResourceId id, IResourceHelpInfo info)
            {
                Id = id;
                _Info = info;
            }

            public ResourceId Id { get; }

            public InfoString Synopsis => _Info.Synopsis;

            public int Count { get; private set; }

            public override string ToString()
            {
                return Id.Value;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is SuppressionInfo info &&
                    Id.Equals(info.Id);
            }

            internal void Hit()
            {
                Count++;
            }
        }

        public ResourceId Id { get; }

        public SourceFile Source { get; }

        public Guid InstanceId { get; }

        public string[] Rule { get; }

        public bool TryMatch(object o, out ISuppressionInfo suppression)
        {
            suppression = null;
            var context = new ExpressionContext(_Context, Source, ResourceKind.SuppressionGroup, o);
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            if (_Fn(context, o).GetValueOrDefault(false))
            {
                _Info.Hit();
                suppression = _Info;
                return true;
            }
            return false;
        }
    }
}
