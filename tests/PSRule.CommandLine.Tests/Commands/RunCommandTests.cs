// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.IO;
using PSRule.CommandLine.Models;

namespace PSRule.CommandLine.Commands;

public sealed class RunCommandTests : BaseTests
{
    [Fact]
    public async Task RunAsync_WithDefault_ShouldRunSuccessfully()
    {
        var console = new TestConsole();
        var context = ClientContext
        (
            option: GetSourcePath("../../../../../ps-rule-ci.yaml"),
            console: console,
            workingPath: GetSourcePath("../../../../../")
        );

        var exitCode = await RunCommand.RunAsync(OperationOptions(), context);
        Assert.Equal(0, exitCode);
    }

    #region Helper methods

    private static RunOptions OperationOptions()
    {
        return new RunOptions();
    }

    #endregion Helper methods
}
