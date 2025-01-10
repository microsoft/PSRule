// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Server;

namespace PSRule.EditorServices.Hosting;

/// <summary>
/// The LSP server for PSRule integration.
/// </summary>
internal sealed class LspServer(Action<LanguageServerOptions> configure) : IDisposable
{
    private readonly LanguageServer _Server = LanguageServer.PreInit(options =>
    {
        WithHandlers(options);
        WithEvents(options);

        configure(options);
    });

    private bool _Disposed;

    /// <summary>
    /// Run the language server and block until exit.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await _Server.Initialize(cancellationToken);

        _Server.LogInfo($"Language server running on processId: {System.Environment.ProcessId}");

        await _Server.WaitForExit;
    }

    /// <summary>
    /// Register request handlers.
    /// </summary>
    private static void WithHandlers(LanguageServerOptions options)
    {
        // options.WithHandler<GetVersionHandler>();
    }

    /// <summary>
    /// Hook up event listeners.
    /// </summary>
    private static void WithEvents(LanguageServerOptions options)
    {
        options.OnInitialized((server, request, response, token) =>
        {
            server.SendServerReady();
            return Task.CompletedTask;
        });
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Server.Dispose();
            }
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
