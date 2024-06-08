// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

internal interface ITokenReader
{
    IPathToken Current { get; }

    bool Next(out IPathToken token);

    bool Consume(PathTokenType type);

    bool Peak(out IPathToken token);
}
