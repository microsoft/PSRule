// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// An interface for implementing a runtime factory.
/// </summary>
public interface IRuntimeFactory
{
    /// <summary>
    /// Call the runtime factory to configure services.
    /// </summary>
    void Configure(IRuntimeFactoryContext context);
}
