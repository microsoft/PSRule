// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Runtime.Binding;

public interface ITargetBindingResult
{
    /// <summary>
    /// The bound TargetName of the target object.
    /// </summary>
    string TargetName { get; }

    string TargetNamePath { get; }

    /// <summary>
    /// The bound TargetType of the target object.
    /// </summary>
    string TargetType { get; }

    string TargetTypePath { get; }

    /// <summary>
    /// Additional bound fields of the target object.
    /// </summary>
    Hashtable Field { get; }

    bool ShouldFilter { get; }
}
