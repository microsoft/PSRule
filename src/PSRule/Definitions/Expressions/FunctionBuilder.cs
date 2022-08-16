// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PSRule.Definitions.Expressions
{
    internal delegate object ExpressionFnOuter(IExpressionContext context);
    internal delegate object ExpressionFn(IExpressionContext context, object[] args);

    internal delegate ExpressionFnOuter ExpressionBuilderFn(IExpressionContext context, LanguageExpression.PropertyBag properties);

    internal abstract class FunctionReader
    {
        public abstract bool TryProperty(out string propertyName);
    }

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
            _Current = new LanguageExpression.PropertyBag();
            _Stack.Push(_Current);
        }

        internal void Add(string name, object value)
        {
            _Current.Add(name, value);
        }

        public ExpressionFnOuter Pop()
        {
            var properties = _Stack.Pop();
            _Current = _Stack.Count > 0 ? _Stack.Peek() : null;
            return TryFunction(properties, out var descriptor) ? descriptor.Fn(null, properties) : null;
        }

        private bool TryFunction(LanguageExpression.PropertyBag properties, out IFunctionDescriptor descriptor)
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

    internal sealed class FunctionFactory
    {
        private readonly Dictionary<string, IFunctionDescriptor> _Descriptors;

        public FunctionFactory()
        {
            _Descriptors = new Dictionary<string, IFunctionDescriptor>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in Functions.Builtin)
                With(d);
        }

        public bool TryDescriptor(string name, out IFunctionDescriptor descriptor)
        {
            return _Descriptors.TryGetValue(name, out descriptor);
        }

        public void With(IFunctionDescriptor descriptor)
        {
            _Descriptors.Add(descriptor.Name, descriptor);
        }
    }

    /// <summary>
    /// A structure describing a specific function.
    /// </summary>
    [DebuggerDisplay("Function: {Name}")]
    internal sealed class FunctionDescriptor : IFunctionDescriptor
    {
        public FunctionDescriptor(string name, ExpressionBuilderFn fn)
        {
            Name = name;
            Fn = fn;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public ExpressionBuilderFn Fn { get; }
    }

    /// <summary>
    /// A structure describing a specific function.
    /// </summary>
    internal interface IFunctionDescriptor
    {
        /// <summary>
        /// The name of the function.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The function delegate.
        /// </summary>
        ExpressionBuilderFn Fn { get; }
    }
}
