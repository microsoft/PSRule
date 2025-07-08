// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule;

/// <summary>
/// The PSRule engine.
/// </summary>
public static partial class Engine
{
    private static readonly string[] _Capabilities = [
        "api-v1",
        "api-2025-01-01"
    ];

    private const string _Version = "3.0.0";

    /// <summary>
    /// The version of PSRule.
    /// </summary>
    public static string GetVersion()
    {
        return _Version;
    }

    /// <summary>
    /// Get the intrinsic capabilities of the engine.
    /// </summary>
    /// <returns>Returns a list of capability identifiers.</returns>
    internal static string[] GetIntrinsicCapability()
    {
        return _Capabilities;
    }
}
