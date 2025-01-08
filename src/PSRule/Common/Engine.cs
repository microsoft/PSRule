// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule;

internal static partial class Engine
{
    private static readonly string[] _Capabilities = [
        "api-v1",
        "api-2025-01-01"
    ];

    internal static string GetVersion()
    {
        return _Version;
    }

    internal static string[] GetIntrinsicCapability()
    {
        return _Capabilities;
    }
}
