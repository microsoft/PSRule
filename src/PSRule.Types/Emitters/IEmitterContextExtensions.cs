// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Emitters;

/// <summary>
/// 
/// </summary>
public static class IEmitterContextExtensions
{
    /// <summary>
    /// Emit each target object in a collection.
    /// </summary>
    /// <param name="context">An <seealso cref="IEmitter"/> context.</param>
    /// <param name="value">A collection of <seealso cref="ITargetObject"/>.</param>
    public static void Emit(this IEmitterContext context, IEnumerable<ITargetObject> value)
    {
        if (context == null || value == null)
            return;

        foreach (var item in value)
        {
            if (item == null)
                return;

            context.Emit(item);
        }
    }
}
