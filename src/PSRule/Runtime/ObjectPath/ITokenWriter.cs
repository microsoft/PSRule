// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

internal interface ITokenWriter
{
    IPathToken? Last { get; }

    void Add(IPathToken token);
}
