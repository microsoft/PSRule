// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Dynamic;

namespace PSRule.Runtime;

internal sealed class DynamicPropertyBinder : GetMemberBinder
{
    internal DynamicPropertyBinder(string name, bool ignoreCase)
        : base(name, ignoreCase) { }

    public override DynamicMetaObject? FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
    {
        return null;
    }
}
