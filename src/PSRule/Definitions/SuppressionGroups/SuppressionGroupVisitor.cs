// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;
using PSRule.Definitions.Expressions;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.SuppressionGroups;

[DebuggerDisplay("{Id.Value}")]
internal sealed class SuppressionGroupVisitor
{
    private readonly LanguageExpressionOuterFn _Fn;
    private readonly SuppressionInfo _Info;
    private readonly LegacyRunspaceContext _Context;

    public SuppressionGroupVisitor(LegacyRunspaceContext context, string apiVersion, ResourceId id, ISourceFile source, ISuppressionGroupSpec spec, IResourceHelpInfo info)
    {
        _Context = context;
        ApiVersion = apiVersion;
        Id = id;
        Source = source;
        InstanceId = Guid.NewGuid();
        Info = info;
        _Info = new SuppressionInfo(id, info);

        switch (spec)
        {
            case ISuppressionGroupV1Spec v1:
                Rule = v1.Rule;
                _Fn = new LanguageExpressionBuilder(Id)
                    .WithRule(Rule)
                    .Build(v1.If);
                break;

            case ISuppressionGroupV2Spec v2:
                Rule = v2.Rule;
                _Fn = new LanguageExpressionBuilder(Id)
                    .WithRule(Rule)
                    .WithType(v2.Type)
                    .Build(v2.If);
                break;

            default:
                throw new UnknownSpecificationException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InvalidResourceSpecification, nameof(ISuppressionGroupSpec), id));
        }
    }

    /// <summary>
    /// Tracking information about the suppression.
    /// </summary>
    private sealed class SuppressionInfo(ResourceId id, IResourceHelpInfo info) : ISuppressionInfo
    {
        private readonly IResourceHelpInfo _Info = info;

        public ResourceId Id { get; } = id;

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

    public string ApiVersion { get; }

    public ResourceId Id { get; }

    public IResourceHelpInfo Info { get; }

    public ISourceFile Source { get; }

    public Guid InstanceId { get; }

    public string[] Rule { get; }

    public bool TryMatch(ResourceId ruleId, ITargetObject o, out ISuppressionInfo? suppression)
    {
        suppression = null;
        var context = new ExpressionContext(_Context, Source, ResourceKind.SuppressionGroup, o, ruleId);
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
