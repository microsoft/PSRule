// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions.Selectors;

internal interface ISelector : ILanguageBlock
{

}

[DebuggerDisplay("Id: {Id}")]
internal sealed class SelectorVisitor : ISelector
{
    private readonly LanguageExpressionOuterFn _Fn;
    private readonly RunspaceContext _Context;

    public SelectorVisitor(RunspaceContext context, ResourceId id, SourceFile source, LanguageIf expression)
    {
        _Context = context;
        Id = id;
        Source = source;
        InstanceId = Guid.NewGuid();
        var builder = new LanguageExpressionBuilder();
        _Fn = builder.Build(expression);
    }

    public Guid InstanceId { get; }

    public ResourceId Id { get; }

    public SourceFile Source { get; }

    public bool Match(object o)
    {
        var context = new ExpressionContext(_Context, Source, ResourceKind.Selector, o);
        context.Debug(PSRuleResources.SelectorMatchTrace, Id);
        return _Fn(context, o).GetValueOrDefault(false);
    }
}
