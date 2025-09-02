// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Server;
using PSRule.EditorServices.Handlers;
using PSRule.Runtime;

namespace PSRule.EditorServices.Hosting;

/// <summary>
/// The LSP server for PSRule integration.
/// </summary>
internal sealed class LspServer : IDisposable
{
    private readonly LanguageServer _Server;

    private bool _Disposed;

    public LspServer(Action<LanguageServerOptions> configure)
    {
        _Server = LanguageServer.PreInit(options =>
        {
            WithServices(options);
            WithHandlers(options);
            WithEvents(options);

            configure(options);
        });
    }

    /// <summary>
    /// Run the language server and block until exit.
    /// </summary>
    public async Task RunWaitAsync(CancellationToken cancellationToken)
    {
        await RunAsync(cancellationToken);
        await WaitForExitAsync();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await _Server.Initialize(cancellationToken);

        _Server.LogInfo($"Language server running on processId: {System.Environment.ProcessId}");
    }

    /// <summary>
    /// Returns a task that can be awaited to block calls until the server exits.
    /// </summary>
    public Task WaitForExitAsync()
    {
        return _Server.WaitForExit;
    }

    /// <summary>
    /// Register services for dependency injection.
    /// </summary>
    private void WithServices(LanguageServerOptions options)
    {
        options.WithServices(services =>
        {
            services.AddSingleton<ILogger>(new ServerLogger(GetLanguageServer));
        });
    }

    /// <summary>
    /// Register request handlers.
    /// </summary>
    private static void WithHandlers(LanguageServerOptions options)
    {
        options.WithHandler<UpgradeDependencyCommandHandler>();
        options.WithHandler<RunAnalysisCommandHandler>();
        options.WithHandler<WorkspaceChangeWatcherHandler>();
        options.WithHandler<ConfigurationChangeHandler>();
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

    private ILanguageServer? GetLanguageServer()
    {
        return _Server;
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
