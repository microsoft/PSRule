// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Log diagnostic messages at runtime.
/// </summary>
/// <typeparam name="TCategoryName">The type name to use for the logger category.</typeparam>
public interface ILogger<out TCategoryName> : ILogger
{

}
