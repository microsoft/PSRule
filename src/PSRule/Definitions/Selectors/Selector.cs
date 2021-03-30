// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System.Collections.Generic;
using System.Diagnostics;

namespace PSRule.Definitions.Selectors
{
    [Spec(Specs.V1, Specs.Selector)]
    internal sealed class SelectorV1 : InternalResource<SelectorV1Spec>
    {
        public SelectorV1(string apiVersion, SourceFile source, ResourceMetadata metadata, ResourceHelpInfo info, SelectorV1Spec spec)
            : base(ResourceKind.Selector, apiVersion, source, metadata, info, spec) { }
    }

    internal sealed class SelectorV1Spec : Spec
    {
        public SelectorIf If { get; set; }
    }

    internal abstract class SelectorExpression
    {
        public SelectorExpression(SelectorExpresssionDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        internal sealed class PropertyBag : KeyMapDictionary<object>
        {
            public PropertyBag()
                : base() { }
        }

        public SelectorExpresssionDescriptor Descriptor { get; }
    }

    [DebuggerDisplay("Selector If")]
    internal sealed class SelectorIf : SelectorExpression
    {
        public SelectorIf(SelectorExpression expression)
            : base(null)
        {
            Expression = expression;
        }

        public SelectorExpression Expression { get; set; }
    }

    [DebuggerDisplay("Selector {Descriptor.Name}")]
    internal sealed class SelectorOperator : SelectorExpression
    {
        internal SelectorOperator(SelectorExpresssionDescriptor descriptor)
            : base(descriptor)
        {
            Children = new List<SelectorExpression>();
        }

        public List<SelectorExpression> Children { get; }

        public void Add(SelectorExpression item)
        {
            Children.Add(item);
        }
    }

    [DebuggerDisplay("Selector {Descriptor.Name}")]
    internal sealed class SelectorCondition : SelectorExpression
    {
        internal SelectorCondition(SelectorExpresssionDescriptor descriptor, PropertyBag properties)
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
