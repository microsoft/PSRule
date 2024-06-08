// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// The type of NameToken.
/// </summary>
internal enum NameTokenType
{
    /// <summary>
    /// The token represents a field/ property of an object.
    /// </summary>
    Field = 0,

    /// <summary>
    /// The token is an index in an object.
    /// </summary>
    Index = 1,

    /// <summary>
    /// The token is a reference to the parent object. Can only be the first token.
    /// </summary>
    Self = 2
}
