// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace PSRule.BuildTool;

internal sealed class ClientContext(bool debug)
{
    public ILogger Logger { get; } = LoggerFactory.Create(builder =>
    {
        builder.SetMinimumLevel(debug ? LogLevel.Debug : LogLevel.Information);
        builder.AddConsole();
    }).CreateLogger<ClientContext>();
}
