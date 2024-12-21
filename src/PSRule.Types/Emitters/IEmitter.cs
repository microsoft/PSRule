// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Emitters;

/// <summary>
/// An emitter for processing input.
/// </summary>
public interface IEmitter : IDisposable
{
    /// <summary>
    /// Visit an object and emit any input objects for processing.
    /// </summary>
    /// <param name="context">The current context for the emitter.</param>
    /// <param name="o">The object to visit.</param>
    /// <returns>Returns <c>true</c> when the emitter processed the object and <c>false</c> when it did not.</returns>
    bool Visit(IEmitterContext context, object o);

    /// <summary>
    /// Determines if the emitter accepts the specified object type.
    /// </summary>
    /// <param name="context">The current context for the emitter.</param>
    /// <param name="type">The type of object.</param>
    /// <returns>Returns <c>true</c> if the emitter supports processing the object type and <c>false</c> if it does not.</returns>
    bool Accepts(IEmitterContext context, Type type);
}
