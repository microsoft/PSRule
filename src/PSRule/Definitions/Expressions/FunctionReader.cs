// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Expressions;

internal abstract class FunctionReader
{
    public abstract bool TryProperty(out string propertyName);
}
