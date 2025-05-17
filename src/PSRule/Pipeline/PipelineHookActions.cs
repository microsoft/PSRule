// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Management.Automation;
using System.Text;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Pipeline;

internal delegate bool ShouldProcess(string target, string action);

/// <summary>
/// Define built-in binding hooks.
/// </summary>
internal static class PipelineHookActions
{
    internal static readonly (BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField) Default = (PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField);
    internal static readonly (BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField) Empty = (null, null, null);

    private const string Property_TargetName = "TargetName";
    private const string Property_Name = "Name";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
    public static string? BindTargetName(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, out string path)
    {
        path = null;
        if (targetObject == null)
            return null;

        if (preferTargetInfo && TryGetInfoTargetName(targetObject, out var targetName))
            return targetName;

        if (propertyNames != null)
            return propertyNames.Any(n => n.Contains('.'))
                ? NestedTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetNameBinding, out path)
                : CustomTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetNameBinding, out path);

        return DefaultTargetNameBinding(targetObject);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
    public static string? BindTargetType(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, out string path)
    {
        path = null;
        if (targetObject == null)
            return null;

        if (preferTargetInfo && TryGetInfoTargetType(targetObject, out var targetType))
            return targetType;

        if (propertyNames != null)
            return propertyNames.Any(n => n.Contains('.'))
                ? NestedTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetTypeBinding, out path)
                : CustomTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetTypeBinding, out path);

        return DefaultTargetTypeBinding(targetObject);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameter preferTargetInfo is required for matching the delegate type.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
    public static string? BindField(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, out string path)
    {
        path = null;
        if (targetObject == null)
            return null;

        if (propertyNames != null)
            return propertyNames.Any(n => n.Contains('.'))
                ? NestedTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultFieldBinding, out path)
                : CustomTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultFieldBinding, out path);

        return DefaultFieldBinding(targetObject);
    }

    /// <summary>
    /// Get the TargetName of the object by looking for a TargetName or Name property.
    /// </summary>
    /// <param name="targetObject">A PSObject to bind.</param>
    /// <returns>The TargetName of the object.</returns>
    private static string DefaultTargetNameBinding(object targetObject)
    {
        return TryGetInfoTargetName(targetObject, out var targetName) ||
            TryGetTargetName(targetObject, propertyName: Property_TargetName, targetName: out targetName) ||
            TryGetTargetName(targetObject, propertyName: Property_Name, targetName: out targetName)
            ? targetName
            : GetUnboundObjectTargetName(targetObject);
    }

    /// <summary>
    /// Get the TargetName of the object by using any of the specified property names.
    /// </summary>
    /// <param name="propertyNames">One or more property names to use to bind TargetName.</param>
    /// <param name="caseSensitive">Determines if binding properties are case-sensitive.</param>
    /// <param name="targetObject">A PSObject to bind.</param>
    /// <param name="next">The next delegate function to check if all of the property names can not be found.</param>
    /// <param name="path">The object path that was used for binding.</param>
    /// <returns>The TargetName of the object.</returns>
    private static string CustomTargetPropertyBinding(string[] propertyNames, bool caseSensitive, object targetObject, BindTargetName next, out string path)
    {
        path = null;
        string targetName = null;
        for (var i = 0; i < propertyNames.Length && targetName == null; i++)
        {
            targetName = ValueAsString(targetObject, propertyName: propertyNames[i], caseSensitive: caseSensitive);
            if (targetName != null)
                path = propertyNames[i];
        }
        // If TargetName is found return, otherwise continue to next delegate
        return targetName ?? next(targetObject);
    }

    /// <summary>
    /// Get the TargetName of the object by using any of the specified property names.
    /// </summary>
    /// <param name="propertyNames">One or more property names to use to bind TargetName.</param>
    /// <param name="caseSensitive">Determines if binding properties are case-sensitive.</param>
    /// <param name="targetObject">A PSObject to bind.</param>
    /// <param name="next">The next delegate function to check if all of the property names can not be found.</param>
    /// <param name="path">The object path that was used for binding.</param>
    /// <returns>The TargetName of the object.</returns>
    private static string NestedTargetPropertyBinding(string[] propertyNames, bool caseSensitive, object targetObject, BindTargetName next, out string path)
    {
        path = null;
        string targetName = null;
        var score = int.MaxValue;
        for (var i = 0; i < propertyNames.Length && score > propertyNames.Length; i++)
        {
            if (ObjectHelper.GetPath(
                bindingContext: PipelineContext.CurrentThread,
                targetObject: targetObject,
                path: propertyNames[i],
                caseSensitive: caseSensitive,
                value: out object value))
            {
                path = propertyNames[i];
                targetName = value.ToString();
                score = i;
            }
        }
        // If TargetName is found return, otherwise continue to next delegate
        return targetName ?? next(targetObject);
    }

    /// <summary>
    /// Calculate a hash for an object to use as TargetName.
    /// </summary>
    /// <param name="targetObject">A PSObject to hash.</param>
    /// <returns>The TargetName of the object.</returns>
    private static string GetUnboundObjectTargetName(object targetObject)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.None,
            MaxDepth = 1024,
            Culture = CultureInfo.InvariantCulture
        };

        settings.Converters.Insert(0, new PSObjectJsonConverter());
        var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(targetObject, settings));
        var name = PipelineContext.CurrentThread.ObjectHashAlgorithm.GetDigest(json);
        return name.Substring(0, name.Length > 50 ? 50 : name.Length);
    }

    /// <summary>
    /// Try to get TargetName from specified property.
    /// </summary>
    private static bool TryGetTargetName(object targetObject, string propertyName, out string targetName)
    {
        targetName = ValueAsString(targetObject, propertyName, false);
        return targetName != null;
    }

    /// <summary>
    /// Get the TargetType by reading TypeNames of the PSObject.
    /// </summary>
    /// <param name="targetObject">A PSObject to bind.</param>
    /// <returns>The TargetObject of the object.</returns>
    private static string DefaultTargetTypeBinding(object targetObject)
    {
        return TryGetInfoTargetType(targetObject, out var targetType) ? targetType : GetTypeNames(targetObject);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Avoid nested conditional expressions that increase complexity.")]
    private static string? GetTypeNames(object targetObject)
    {
        if (targetObject == null)
            return null;

        return targetObject is PSObject pso ? pso.TypeNames[0] : targetObject.GetType().FullName;
    }

    private static string? DefaultFieldBinding(object targetObject)
    {
        return null;
    }

    private static bool TryGetInfoTargetName(object targetObject, out string targetName)
    {
        targetName = null;
        var baseObject = ExpressionHelpers.GetBaseObject(targetObject);
        if (targetObject is PSObject pso && pso.TryTargetInfo(out var targetInfoMember) && targetInfoMember.TargetName != null)
            targetName = targetInfoMember.TargetName;

        if (baseObject is ITargetInfo info)
            targetName = info.TargetName;

        return targetName != null;
    }

    private static bool TryGetInfoTargetType(object targetObject, out string targetType)
    {
        targetType = null;
        var baseObject = ExpressionHelpers.GetBaseObject(targetObject);
        if (targetObject is PSObject pso && pso.TryTargetInfo(out var targetInfoMember) && targetInfoMember.TargetType != null)
            targetType = targetInfoMember.TargetType;

        if (baseObject is ITargetInfo info)
            targetType = info.TargetType;

        return targetType != null;
    }

    private static string? ValueAsString(object o, string propertyName, bool caseSensitive)
    {
        return ObjectHelper.GetPath(bindingContext: null, targetObject: o, path: propertyName, caseSensitive: caseSensitive, value: out object value) && value != null ? value.ToString() : null;
    }
}
