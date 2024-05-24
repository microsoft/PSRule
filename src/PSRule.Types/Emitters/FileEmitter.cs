// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Emitters;

/// <summary>
/// An emitter that implements emitting objects from file streams.
/// </summary>
public abstract class FileEmitter : BaseEmitter
{
    /// <inheritdoc/>
    public sealed override bool Accepts(IEmitterContext context, Type type)
    {
        return context != null && type != null &&
            (typeof(IFileInfo).IsAssignableFrom(type) || type == typeof(string));
    }

    /// <inheritdoc/>
    public sealed override bool Visit(IEmitterContext context, object o)
    {
        if (context == null || o == null) return false;

        if (o is IFileInfo info && AcceptsFilePath(context, info))
            return VisitFile(context, info.GetFileStream());

        return o is string s && AcceptsString(context) ? VisitString(context, s) : false;
    }

    /// <summary>
    /// Determines if the emitter accepts a file based on it's path.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    protected abstract bool AcceptsFilePath(IEmitterContext context, IFileInfo info);

    /// <summary>
    /// Determines if the emitter accepts an input string as content.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual bool AcceptsString(IEmitterContext context)
    {
        return false;
    }

    /// <summary>
    /// Visit a specific file.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    protected abstract bool VisitFile(IEmitterContext context, IFileStream stream);

    /// <summary>
    /// Visit a string.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    protected virtual bool VisitString(IEmitterContext context, string content)
    {
        return false;
    }
}
