// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using PSRule.CommandLine;
using PSRule.Runtime;

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Handler for workspace file change notifications to reload PSRule options when they change.
/// </summary>
public sealed class WorkspaceChangeWatcherHandler(ClientContext context, ILogger logger) : IDidChangeWatchedFilesHandler
{
    private readonly ClientContext _Context = context;
    private readonly ILogger _Logger = logger;

    /// <summary>
    /// Get registration options for file watching.
    /// </summary>
    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions(DidChangeWatchedFilesCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DidChangeWatchedFilesRegistrationOptions();
    }

    /// <summary>
    /// Handle file change notifications and reload PSRule options if needed.
    /// </summary>
    public Task<Unit> Handle(DidChangeWatchedFilesParams notification, CancellationToken cancellationToken)
    {
        // Check if any of the changed files are options files
        foreach (var change in notification.Changes)
        {
            var fileName = Path.GetFileName(change.Uri.ToString());
            if (IsOptionsFile(fileName))
            {
                _Logger.LogInformation(EventId.None, "PSRule options file changed: {0}", change.Uri);

                try
                {
                    // Reload the options from the file
                    _Context.ReloadOptions();
                    _Logger.LogInformation(EventId.None, "Successfully reloaded PSRule options.");
                }
                catch (Exception ex)
                {
                    _Logger.LogError(EventId.None, ex, "Failed to reload PSRule options: {0}", ex.Message);
                }

                // Only reload once even if multiple options files changed
                break;
            }
        }

        return Task.FromResult(Unit.Value);
    }

    private bool IsOptionsFile(string fileName)
    {
        // Get the configured options path from context
        var configuredPath = _Context.OptionsPath;

        if (!string.IsNullOrEmpty(configuredPath))
        {
            // Check if the changed file matches the configured options path
            var configuredFileName = Path.GetFileName(configuredPath);
            return fileName.Equals(configuredFileName, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // Default to ps-rule.yaml only
            return fileName.Equals("ps-rule.yaml", StringComparison.OrdinalIgnoreCase);
        }
    }
}
