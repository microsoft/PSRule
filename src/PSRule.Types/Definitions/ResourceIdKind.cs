// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

/// <summary>
/// Additional information about the type of identifier if available.
/// </summary>
public enum ResourceIdKind
{
    /// <summary>
    /// Not specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// The identifier is a primary resource identifier.
    /// </summary>
    Id = 2,

    /// <summary>
    /// The identifier is a opaque reference resource identifier.
    /// </summary>
    Ref = 3,

    /// <summary>
    /// The identifier is an alias resource identifier.
    /// </summary>
    Alias = 4,
}
