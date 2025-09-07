// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions;

public interface IDependencyNode<T>
{
    T Value { get; }

    bool Skipped { get; }

    // bool Failed { get; }

    // bool Passed { get; }

    void Pass();

    void Fail();
}
