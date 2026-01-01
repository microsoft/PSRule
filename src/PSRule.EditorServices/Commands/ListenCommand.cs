// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Pipes;
using Microsoft.Extensions.DependencyInjection;
using OmniSharp.Extensions.LanguageServer.Server;
using PSRule.CommandLine;
using PSRule.EditorServices.Hosting;
using PSRule.EditorServices.Models;

namespace PSRule.EditorServices.Commands;

/// <summary>
/// The listen command for running the persistent language server.
/// </summary>
internal sealed class ListenCommand
{
    private const string LOCAL_PIPE_SERVER_NAME = ".";
    private const string WINDOWS_PIPE_PREFIX = @"\\.\pipe\";

    // Timeout in minutes to wait for the debugger to attach.
    private const int DEBUGGER_ATTACH_TIMEOUT = 5;

    // Time in milliseconds to wait between debugger attach checks.
    private const int DEBUGGER_ATTACH_CYCLE_WAIT_TIME = 500;

    private const int ERROR_SUCCESS = 0;
    private const int ERROR_INVALID_CONFIGURATION = 901;
    private const int ERROR_SERVER_EXCEPTION = 902;
    private const int ERROR_DEBUGGER_ATTACH_TIMEOUT = 903;

    /// <summary>
    /// Run the listen command.
    /// This command will start a language server and block until exit.
    /// </summary>
    public static async Task<int> ListenAsync(ListenOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
    {
        if (operationOptions.WaitForDebugger)
        {
            if (!await WaitForDebuggerAsync(cancellationToken))
                return ERROR_DEBUGGER_ATTACH_TIMEOUT;

            System.Diagnostics.Debugger.Break();
        }

        var server = await GetServerAsync(operationOptions, clientContext, cancellationToken);
        if (server == null)
            return ERROR_INVALID_CONFIGURATION;

        try
        {
            await server.RunWaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            clientContext.Console.Out.WriteLine(ex.Message);
            return ERROR_SERVER_EXCEPTION;
        }

        return ERROR_SUCCESS;
    }

    private static async Task<LspServer?> GetServerAsync(ListenOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken)
    {
        // Create a server with a named pipe client stream.
        if (operationOptions.Pipe is { } pipeName)
        {
            var clientPipe = await ConnectNamedPipeClientStreamAsync(pipeName, cancellationToken);

            return new LspServer(options =>
            {
                WithServices(options, clientContext);

                options.WithInput(clientPipe)
                    .WithOutput(clientPipe)
                    .RegisterForDisposal(clientPipe);
            });
        }
        else if (operationOptions.Stdio)
        {
            return new LspServer(options =>
            {
                WithServices(options, clientContext);

                options.WithInput(clientContext.Console.OpenStandardInput())
                    .WithOutput(clientContext.Console.OpenStandardOutput());
            });
        }

        return null;
    }

    /// <summary>
    /// Connect to a named pipe client stream.
    /// </summary>
    private static async Task<NamedPipeClientStream> ConnectNamedPipeClientStreamAsync(string pipeName, CancellationToken cancellationToken)
    {
        pipeName = pipeName.StartsWith(WINDOWS_PIPE_PREFIX) ? pipeName[WINDOWS_PIPE_PREFIX.Length..] : pipeName;

        var clientPipe = new NamedPipeClientStream(LOCAL_PIPE_SERVER_NAME, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await clientPipe.ConnectAsync(cancellationToken);

        return clientPipe;
    }

    /// <summary>
    /// Wait up to 5 minutes for the debugger to attach.
    /// </summary>
    private static async Task<bool> WaitForDebuggerAsync(CancellationToken cancellationToken)
    {
        try
        {
            var debuggerTimeoutToken = CancellationTokenSource.CreateLinkedTokenSource
            (
                cancellationToken,
                new CancellationTokenSource(TimeSpan.FromMinutes(DEBUGGER_ATTACH_TIMEOUT)).Token
            ).Token;

            while (!System.Diagnostics.Debugger.IsAttached)
            {
                await Task.Delay(DEBUGGER_ATTACH_CYCLE_WAIT_TIME, debuggerTimeoutToken);
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Register services for dependency injection.
    /// </summary>
    private static void WithServices(LanguageServerOptions options, ClientContext clientContext)
    {
        options.WithServices(services =>
        {
            services.AddSingleton<ClientContext>(clientContext);
        });
    }
}
