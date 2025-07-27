// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// Extensions for <see cref="ITargetObject"/>.
/// </summary>
public static class ITargetObjectExtensions
{
    /// <summary>
    /// Try to get the name of a target object, and the object path of the name property.
    /// </summary>
    /// <param name="targetObject">The target object.</param>
    /// <param name="name">The name of the target object.</param>
    /// <param name="path">The object path to the property that sets the name.</param>
    /// <returns>Returns <c>true</c> if the name was set, otherwise <c>false</c>.</returns>
    public static bool TryGetName(this ITargetObject targetObject, out string? name, out string? path)
    {
        name = null;
        path = null;
        if (targetObject is null) throw new ArgumentNullException(nameof(targetObject));
        if (targetObject.Name == null || string.IsNullOrWhiteSpace(targetObject.Name)) return false;

        name = targetObject.Name;
        // path = targetObject.Name.Path;
        return true;
    }

    /// <summary>
    /// Try to get the type of a target object, and the object path of the type property.
    /// </summary>
    /// <param name="targetObject">The target object.</param>
    /// <param name="type">The type of the target object.</param>
    /// <param name="path">The object path to the property that sets the type.</param>
    /// <returns>Returns <c>true</c> if the type was set, otherwise <c>false</c>.</returns>
    public static bool TryGetType(this ITargetObject targetObject, out string? type, out string? path)
    {
        type = null;
        path = null;
        if (targetObject is null) throw new ArgumentNullException(nameof(targetObject));
        if (targetObject.Type == null || string.IsNullOrWhiteSpace(targetObject.Type)) return false;

        type = targetObject.Type;
        // path = targetObject.Type.Path;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetObject"></param>
    /// <param name="name">The source identifier.</param>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetSource(this ITargetObject targetObject, string name, out TargetSourceInfo? source)
    {
        source = null;
        if (targetObject is null) throw new ArgumentNullException(nameof(targetObject));
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (targetObject.Source == null) return false;

        source = targetObject.Source.FirstOrDefault(s => s.Type == name);
        return source != null;
    }
}
