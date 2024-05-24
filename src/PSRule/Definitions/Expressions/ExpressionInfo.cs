// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal sealed class ExpressionInfo
{
    private readonly string _Path;

    public ExpressionInfo(string path)
    {
        _Path = path;
    }
}
