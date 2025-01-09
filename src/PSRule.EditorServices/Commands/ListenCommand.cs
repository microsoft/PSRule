// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Pipes;
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

        var server = await GetServerAsync(operationOptions, cancellationToken);
        if (server == null)
            return ERROR_INVALID_CONFIGURATION;

        try
        {
            await server.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ERROR_SERVER_EXCEPTION;
        }

        return ERROR_SUCCESS;
    }

    private static async Task<LspServer?> GetServerAsync(ListenOptions operationOptions, CancellationToken cancellationToken)
    {
        // Create a server with a named pipe client stream.
        if (operationOptions.Pipe is { } pipeName)
        {
            var clientPipe = await ConnectNamedPipeClientStreamAsync(pipeName, cancellationToken);

            return new LspServer(options =>
            {
                options.WithInput(clientPipe)
                    .WithOutput(clientPipe)
                    .RegisterForDisposal(clientPipe);
            });
        }
        else if (operationOptions.Stdio)
        {
            return new LspServer(options =>
            {
                options.WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput());
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
                new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token
            ).Token;

            while (!System.Diagnostics.Debugger.IsAttached)
            {
                await Task.Delay(500, debuggerTimeoutToken);
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}
