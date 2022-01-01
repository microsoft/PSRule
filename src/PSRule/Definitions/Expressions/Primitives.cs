// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using PSRule.Pipeline;

namespace PSRule.Definitions.Expressions
{
    internal interface ILanguageExpresssionDescriptor
    {
        string Name { get; }

        LanguageExpressionType Type { get; }

        LanguageExpression CreateInstance(SourceFile source, LanguageExpression.PropertyBag properties);
    }

    internal sealed class LanguageExpresssionDescriptor : ILanguageExpresssionDescriptor
    {
        public LanguageExpresssionDescriptor(string name, LanguageExpressionType type, LanguageExpressionFn fn)
        {
            Name = name;
            Type = type;
            Fn = fn;
        }

        public string Name { get; }

        public LanguageExpressionType Type { get; }

        public LanguageExpressionFn Fn { get; }

        public LanguageExpression CreateInstance(SourceFile source, LanguageExpression.PropertyBag properties)
        {
            return Type switch
            {
                LanguageExpressionType.Operator => new LanguageOperator(this),
                LanguageExpressionType.Condition => new LanguageCondition(this, properties),
                _ => null
            };
        }
    }

    internal abstract class LanguageExpression
    {
        public LanguageExpression(LanguageExpresssionDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        internal sealed class PropertyBag : KeyMapDictionary<object>
        {
            public PropertyBag()
                : base() { }
        }

        public LanguageExpresssionDescriptor Descriptor { get; }
    }

    [DebuggerDisplay("Selector If")]
    internal sealed class LanguageIf : LanguageExpression
    {
        public LanguageIf(LanguageExpression expression)
            : base(null)
        {
            Expression = expression;
        }

        public LanguageExpression Expression { get; set; }
    }

    [DebuggerDisplay("Selector {Descriptor.Name}")]
    internal sealed class LanguageOperator : LanguageExpression
    {
        internal LanguageOperator(LanguageExpresssionDescriptor descriptor)
            : base(descriptor)
        {
            Children = new List<LanguageExpression>();
        }

        public List<LanguageExpression> Children { get; }

        public void Add(LanguageExpression item)
        {
            Children.Add(item);
        }
    }

    [DebuggerDisplay("Selector {Descriptor.Name}")]
    internal sealed class LanguageCondition : LanguageExpression
    {
        internal LanguageCondition(LanguageExpresssionDescriptor descriptor, PropertyBag properties)
            : base(descriptor)
        {
            Property = properties ?? new PropertyBag();
        }

        public PropertyBag Property { get; }

        internal void Add(PropertyBag properties)
        {
            Property.AddUnique(properties);
        }
    }
}
