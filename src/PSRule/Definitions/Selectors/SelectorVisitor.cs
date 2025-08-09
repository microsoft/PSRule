// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Data;
using PSRule.Definitions.Expressions;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Selectors;

[DebuggerDisplay("Id: {Id}")]
internal sealed class SelectorVisitor
{
    private readonly LanguageExpressionOuterFn _Fn;

    public SelectorVisitor(string apiVersion, ResourceId id, ISourceFile source, ISelectorSpec spec)
    {
        Id = id;
        Source = source;
        InstanceId = Guid.NewGuid();

        switch (spec)
        {
            case ISelectorV1Spec v1:
                _Fn = new LanguageExpressionBuilder(Id)
                    .Build(v1.If);
                break;

            case ISelectorV2Spec v2:
                _Fn = new LanguageExpressionBuilder(Id)
                    .WithType(v2.Type)
                    .Build(v2.If);
                break;

            default:
                throw new UnknownSpecificationException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.InvalidResourceSpecification, nameof(ISelectorSpec), id));
        }
    }

    public Guid InstanceId { get; }

    public ResourceId Id { get; }

    public ISourceFile Source { get; }

    public bool If(IExpressionContext expressionContext, ITargetObject o)
    {
        var context = new ExpressionContext(expressionContext, Source, ResourceKind.Selector, o);
        context.Logger.LogDebug(EventId.None, PSRuleResources.SelectorMatchTrace, Id);
        return _Fn(context, o).GetValueOrDefault(false);
    }
}
