// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Runtime.ObjectPath;

[DebuggerDisplay("Type = {Type}, Arg = {Arg}")]
internal sealed class PathToken : IPathToken
{
    public static readonly PathToken RootRef = new(PathTokenType.RootRef);
    public static readonly PathToken CurrentRef = new(PathTokenType.CurrentRef);

    public PathTokenType Type { get; }

    public PathTokenOption Option { get; }

    public PathToken(PathTokenType type)
    {
        Type = type;
    }

    public PathToken(PathTokenType type, object arg, PathTokenOption option = PathTokenOption.None)
    {
        Type = type;
        Arg = arg;
        Option = option;
    }

    public object Arg { get; }

    public T? As<T>()
    {
        return Arg is T result ? result : default;
    }
}
