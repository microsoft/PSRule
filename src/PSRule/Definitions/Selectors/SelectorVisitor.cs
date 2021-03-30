// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Resources;
using System;
using System.Diagnostics;

namespace PSRule.Definitions.Selectors
{
    [DebuggerDisplay("Id: {Id}")]
    internal sealed class SelectorVisitor
    {
        private readonly SelectorExpressionOuterFn _Fn;

        public SelectorVisitor(string id, SelectorIf expression)
        {
            Id = id;
            InstanceId = Guid.NewGuid();
            var builder = new SelectorExpressionBuilder();
            _Fn = builder.Build(expression);
        }

        public Guid InstanceId { get; }

        public string Id { get; }

        public bool Match(object o)
        {
            var context = new SelectorContext();
            context.Debug(PSRuleResources.SelectorMatchTrace, Id);
            return _Fn(context, o);
        }
    }
}
