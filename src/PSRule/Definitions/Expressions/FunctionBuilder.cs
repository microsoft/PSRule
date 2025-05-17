// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal delegate object? ExpressionFnOuter(IExpressionContext context);
internal delegate object? ExpressionFn(IExpressionContext context, object[] args);

internal delegate ExpressionFnOuter ExpressionBuilderFn(IExpressionContext context, LanguageExpression.PropertyBag properties);

internal sealed class FunctionBuilder
{
    private readonly Stack<LanguageExpression.PropertyBag> _Stack;
    private readonly FunctionFactory _Functions;

    private LanguageExpression.PropertyBag _Current;

    internal FunctionBuilder() : this(new FunctionFactory()) { }

    internal FunctionBuilder(FunctionFactory expressionFactory)
    {
        _Functions = expressionFactory;
        _Stack = new Stack<LanguageExpression.PropertyBag>();
    }

    public void Push()
    {
        _Current = [];
        _Stack.Push(_Current);
    }

    internal void Add(string name, object value)
    {
        _Current.Add(name, value);
    }

    public ExpressionFnOuter? Pop()
    {
        var properties = _Stack.Pop();
        _Current = _Stack.Count > 0 ? _Stack.Peek() : null;
        return TryFunction(properties, out var descriptor) ? descriptor.Fn(null, properties) : null;
    }

    private bool TryFunction(LanguageExpression.PropertyBag properties, out IFunctionDescriptor? descriptor)
    {
        descriptor = null;
        foreach (var property in properties)
        {
            if (_Functions.TryDescriptor(property.Key, out descriptor))
                return true;
        }
        return false;
    }
}
