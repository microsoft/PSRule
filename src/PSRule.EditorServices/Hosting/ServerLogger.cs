// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using PSRule.Runtime;

namespace PSRule.EditorServices.Hosting;

/// <summary>
/// Diagnostic logging client to send logs to the server client.
/// </summary>
internal sealed class ServerLogger(Func<ILanguageServer?> languageServer) : ILogger
{
    private readonly Func<ILanguageServer?> _LanguageServer = languageServer;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == LogLevel.Information ||
            logLevel == LogLevel.Warning ||
            logLevel == LogLevel.Error ||
            logLevel == LogLevel.Debug;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_LanguageServer == null)
            return;

        var logger = _LanguageServer();
        if (logger == null)
            return;

        switch (logLevel)
        {
            case LogLevel.Debug:
            case LogLevel.Information:
                logger.LogInfo(formatter(state, exception));
                break;

            case LogLevel.Warning:
                logger.LogWarning(formatter(state, exception));
                break;

            case LogLevel.Error:
                logger.LogError(formatter(state, exception));
                break;
        }
    }
}
