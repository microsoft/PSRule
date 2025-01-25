// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using PSRule.EditorServices.Handlers;

namespace PSRule.EditorServices;

/// <summary>
/// Tests for <see cref="Hosting.LspServer"/>.
/// </summary>
public sealed class LspServerTests
{
    [Fact]
    public async Task TestServerCommandAsync()
    {
        using var container = await LanguageServerTestContainer.CreateAsync(workingPath: GetRootPath());

        var client = container.Client;

        var response = await client.ExecuteCommandWithResponse<UpgradeDependencyCommandHandlerOutput>(Command.Create
        (
            name: "upgradeDependency",
            args: new UpgradeDependencyCommandHandlerInput
            {
                Module = "*",
                Path = GetSourcePath("ps-rule.lock.json")
            }
        ));

        Assert.NotNull(response);
    }

    private static string GetRootPath()
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../"));
    }

    private static string GetSourcePath(string file)
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../", file));
    }
}
