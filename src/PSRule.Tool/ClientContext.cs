// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool;

internal sealed class ClientContext
{
    public ClientContext()
    {
        Path = AppDomain.CurrentDomain.BaseDirectory;
    }

    public string Path { get; }
}
