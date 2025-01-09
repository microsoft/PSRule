// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace PSRule.EditorServices.Hosting;

/// <summary>
/// Extension methods for working with <see cref="ILanguageServer"/>.
/// </summary>
internal static class ILanguageServerExtensions
{
    public static void SendServerReady(this ILanguageServer server)
    {
        server.SendProgress(new ProgressParams
        {
            Token = new ProgressToken("server/ready"),
            Value = new JValue(Engine.GetVersion()),
        });
    }
}
