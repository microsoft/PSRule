// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Data;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule.Emitters;

/// <summary>
/// 
/// </summary>
/// <param name="format"></param>
/// <param name="objectPath"></param>
/// <param name="shouldEmitFile"></param>
internal abstract class BaseEmitterContext(InputFormat format, string objectPath, bool shouldEmitFile) : IEmitterContext
{
    /// <inheritdoc/>
    public InputFormat Format { get; } = format;

    public string ObjectPath { get; } = objectPath;

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
        if (typeof(IEnumerable).IsAssignableFrom(nestedType))
        {
            var result = new List<TargetObject>();
            foreach (var item in nestedObject as IEnumerable)
                result.Add(new TargetObject(PSObject.AsPSObject(item)));

            return [.. result];
        }
        else
        {
            return new TargetObject[] { new(PSObject.AsPSObject(nestedObject), new TargetSourceCollection(targetObject.Source)) };
        }
    }
}
