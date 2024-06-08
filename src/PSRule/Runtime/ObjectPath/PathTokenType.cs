// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

internal enum PathTokenType
{
    None = 0,

    /// <summary>
    /// Token: $
    /// </summary>
    RootRef,

    /// <summary>
    /// Token: @
    /// </summary>
    CurrentRef,

    /// <summary>
    /// Token: .Name
    /// </summary>
    DotSelector,

    /// <summary>
    /// Token: [index]
    /// </summary>
    IndexSelector,

    /// <summary>
    /// Token: [*]
    /// </summary>
    IndexWildSelector,

    StartFilter,
    ComparisonOperator,
    Boolean,
    EndFilter,
    String,
    Integer,
    LogicalOperator,

    StartGroup,
    EndGroup,

    /// <summary>
    /// Token: !
    /// </summary>
    NotOperator,

    /// <summary>
    /// Token: ..
    /// </summary>
    DescendantSelector,

    /// <summary>
    /// Token: .*
    /// </summary>
    DotWildSelector,

    ArraySliceSelector,
    UnionIndexSelector,
    UnionQuotedMemberSelector,
}
