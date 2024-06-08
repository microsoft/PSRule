// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

internal interface IPathToken
{
    PathTokenType Type { get; }

    PathTokenOption Option { get; }

    object Arg { get; }

    T As<T>();
}
