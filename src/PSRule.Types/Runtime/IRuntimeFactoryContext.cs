// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A context for a runtime factory.
/// </summary>
public interface IRuntimeFactoryContext
{
    /// <summary>
    /// Configure services for the runtime factory.
    /// </summary>
    void ConfigureServices(Action<IRuntimeServiceCollection> configure);
}
