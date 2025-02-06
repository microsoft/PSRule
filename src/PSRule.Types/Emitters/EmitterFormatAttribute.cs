// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Emitters;

/// <summary>
/// An attribute that binds an emitter to a specific format.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EmitterFormatAttribute(string format) : Attribute
{
    /// <summary>
    /// The format this emitter is bound to.
    /// </summary>
    public string Format { get; } = format;
}
