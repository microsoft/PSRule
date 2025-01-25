// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Client;
using PSRule.CommandLine;
using PSRule.EditorServices.Hosting;

namespace PSRule.EditorServices;

internal sealed class LanguageServerTestContainer(LspServer server, LanguageClient client) : IDisposable
{
    private bool _Disposed;

    public LspServer Server { get; } = server;
    public LanguageClient Client { get; } = client;

    public static async Task<LanguageServerTestContainer> CreateAsync(string? workingPath = default, CancellationToken cancellationToken = default)
    {
        var clientPipe = new Pipe();
        var serverPipe = new Pipe();

        // Setup server.
        var server = new LspServer(options =>
        {
            options.WithInput(serverPipe.Reader)
                .WithOutput(clientPipe.Writer)
                .WithServices(services =>
                {
                    services.AddSingleton(new ClientContext(InvocationContext(), null, false, false, workingPath));

                });
        });

        // Setup client.
        var client = LanguageClient.PreInit(options =>
        {
            options.WithInput(clientPipe.Reader)
                .WithOutput(serverPipe.Writer);
        });

        // Wait for the server and client to initialize and connect.
        await Task.WhenAll(
            server.RunAsync(cancellationToken),
            client.Initialize(cancellationToken)
        );

        // Don't wait until exit, fire and forget on this thread.
        _ = server.WaitForExitAsync();

        return new(server, client);
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                Client.Dispose();
                Server.Dispose();
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

    private static InvocationContext InvocationContext(IConsole? console = null)
    {
        var p = new Parser();
        var result = p.Parse(string.Empty);
        return new InvocationContext(result, console);
    }
}
