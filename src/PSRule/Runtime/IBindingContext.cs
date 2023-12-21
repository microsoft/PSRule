// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime.ObjectPath;

namespace PSRule.Runtime;

internal interface IBindingContext
{
    bool GetPathExpression(string path, out PathExpression expression);

    void CachePathExpression(string path, PathExpression expression);
}
