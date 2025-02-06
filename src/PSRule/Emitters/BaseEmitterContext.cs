// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Emitters;

#nullable enable

/// <summary>
/// A base implementation of <see cref="IEmitterContext"/>.
/// </summary>
/// <param name="stringFormat">A configured format for string objects.</param>
/// <param name="objectPath">An object path to use.</param>
/// <param name="shouldEmitFile">Determines if files should be emitted as objects.</param>
internal abstract class BaseEmitterContext(string? stringFormat, string? objectPath, bool shouldEmitFile) : IEmitterContext
{
    /// <inheritdoc/>
    public string? StringFormat { get; } = NormalizeStringFormat(stringFormat);

    public string? ObjectPath { get; } = objectPath;

    /// <inheritdoc/>
    public bool ShouldEmitFile { get; } = shouldEmitFile;

    /// <inheritdoc/>
    public void Emit(ITargetObject value)
    {
        if (ObjectPath == null)
        {
            Enqueue(value);
        }
        else
        {
            foreach (var targetObject in ReadObjectPath(value, ObjectPath, caseSensitive: false))
                Enqueue(targetObject);
        }
    }

    /// <inheritdoc/>
    public abstract bool ShouldQueue(string path);

    protected abstract void Enqueue(ITargetObject value);

    private static ITargetObject[] ReadObjectPath(ITargetObject targetObject, string objectPath, bool caseSensitive)
    {
        if (!ObjectHelper.GetPath(
            bindingContext: null,
            targetObject: targetObject.Value,
            path: objectPath,
            caseSensitive: caseSensitive,
            value: out object nestedObject))
            return [];

        var nestedType = nestedObject.GetType();
        if (typeof(IEnumerable).IsAssignableFrom(nestedType) && nestedObject is IEnumerable items)
        {
            var result = new List<TargetObject>();
            foreach (var item in items)
                result.Add(new TargetObject(PSObject.AsPSObject(item)));

            return [.. result];
        }
        else
        {
            return new TargetObject[] { new(PSObject.AsPSObject(nestedObject), new TargetSourceCollection(targetObject.Source)) };
        }
    }

    private static string? NormalizeStringFormat(string? format)
    {
        return format == null || string.IsNullOrWhiteSpace(format) ? null : format.ToLowerInvariant();
    }
}

#nullable restore
