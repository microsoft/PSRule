// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Runtime;

#pragma warning disable CS9113 // Parameter is unused for unit tests.

/// <summary>
/// Define a factory that has a constructor that can not be resolved.
/// </summary>
[RuntimeFactory]
public sealed class BadRuntimeFactory(TestObject testObject) : IRuntimeFactory
{
    public void Configure(IRuntimeFactoryContext context)
    {
        throw new System.NotImplementedException();
    }
}

#pragma warning restore CS9113 // Parameter is unused for unit tests.
