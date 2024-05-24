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
    /// <param name="context">A context object for the emitter.</param>
    /// <param name="o">The object to visit.</param>
    /// <returns>Returns <c>true</c> when the emitter processed the object and <c>false</c> when it did not.</returns>
    bool Visit(IEmitterContext context, object o);

    /// <summary>
    /// Determines if the emitter accepts the specified object type.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    bool Accepts(IEmitterContext context, Type type);

    ///// <summary>
    ///// Configure the emitter using an options instance.
    ///// </summary>
    ///// <param name="option"></param>
    ///// <returns></returns>
    //bool Configure(PSRuleOption option);
}
