// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using PSRule.CommandLine;
using PSRule.CommandLine.Commands;
using PSRule.CommandLine.Models;
using PSRule.Runtime;

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Handler the upgradeDependency command.
/// This command upgrades a module dependency to the latest version that meeds any specified constraints.
/// </summary>
public sealed class UpgradeDependencyCommandHandler(ClientContext context, ISerializer serializer, ILogger logger)
    : ExecuteTypedResponseCommandHandlerBase<UpgradeDependencyCommandHandlerInput, UpgradeDependencyCommandHandlerOutput>(COMMAND_NAME, serializer)
{
    private const string COMMAND_NAME = "upgradeDependency";
    private const string ALL_MODULES_PLACEHOLDER = "*";

    private readonly ClientContext _Context = context;
    private readonly ILogger _Logger = logger;

    /// <summary>
    /// Handle the <c>upgradeDependency</c> command.
    /// </summary>
    public override async Task<UpgradeDependencyCommandHandlerOutput> Handle(UpgradeDependencyCommandHandlerInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Path, nameof(input.Path));
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Module, nameof(input.Module));

        var all = input.Module == ALL_MODULES_PLACEHOLDER;

        var options = new ModuleOptions
        {
            Module = all ? null : [input.Module],
        };

        if (all)
        {
            _Logger.LogInformation(EventId.None, "Checking for upgrades of all modules in: {0}", input.Path);
        }
        else
        {
            _Logger.LogInformation(EventId.None, "Checking for upgrades of module {0} in: {1}", input.Module, input.Path);
        }

        try
        {
            var exitCode = await ModuleCommand.ModuleUpgradeAsync(options, _Context, cancellationToken);
            if (exitCode != 0)
            {
                throw new InvalidOperationException("Module upgrade failed");
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(EventId.None, ex, ex.Message);
        }

        return new UpgradeDependencyCommandHandlerOutput();
    }
}
