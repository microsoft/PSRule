// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Definitions;

/// <summary>
/// Detailed rule records for PSRule v2.
/// </summary>
public interface IDetailedRuleResultV2 : IRuleResultV2
{
    /// <summary>
    /// Custom data set by the rule for this target object.
    /// </summary>
    Hashtable Data { get; }

    /// <summary>
    /// Detailed information about the rule result.
    /// </summary>
    IResultDetail Detail { get; }

    /// <summary>
    /// A set of custom fields bound for the target object.
    /// </summary>
    Hashtable Field { get; }

    /// <summary>
    /// The bound name of the target.
    /// </summary>
    string TargetName { get; }

    /// <summary>
    /// The bound type of the target.
    /// </summary>
    string TargetType { get; }
}
