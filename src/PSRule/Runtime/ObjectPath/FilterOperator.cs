// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

internal enum FilterOperator
{
    None = 0,

    // Comparison
    Equal,
    NotEqual,
    LessOrEqual,
    Less,
    GreaterOrEqual,
    Greater,
    RegEx,

    // Logical
    Or,
    And,
}
