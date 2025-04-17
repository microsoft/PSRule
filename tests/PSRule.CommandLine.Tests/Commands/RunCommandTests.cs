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

        var output = await RunCommand.RunAsync(OperationOptions(), context);
        Assert.Equal(0, output.ExitCode);
    }

    [Fact]
    public async Task RunAsync_WithNoSources_ShouldError()
    {
        var console = new TestConsole();
        var context = ClientContext
        (
            option: GetSourcePath("../../../../../ps-rule.yaml"),
            console: console,
            workingPath: GetSourcePath("../../../../../")
        );

        var output = await RunCommand.RunAsync(OperationOptions(), context);
        Assert.Equal(15, output.ExitCode);
    }

    [Fact]
    public async Task RunAsync_WithNoMatchRules_ShouldError()
    {
        var console = new TestConsole();
        var context = ClientContext
        (
            option: GetSourcePath("../../../../../ps-rule-ci.yaml"),
            console: console,
            workingPath: GetSourcePath("../../../../../")
        );

        var operation = OperationOptions();
        operation.Name = ["NotARule"];

        var output = await RunCommand.RunAsync(operation, context);
        Assert.Equal(16, output.ExitCode);
    }

    [Fact]
    public async Task RunAsync_WithNoInput_ShouldError()
    {
        var console = new TestConsole();
        var context = ClientContext
        (
            option: GetSourcePath("../../../../../ps-rule-ci.yaml"),
            console: console,
            workingPath: GetSourcePath("../../../../../")
        );

        var operation = OperationOptions();
        operation.InputPath = ["./out/"];

        var output = await RunCommand.RunAsync(operation, context);
        Assert.Equal(17, output.ExitCode);
    }

    #region Helper methods

    private static RunOptions OperationOptions()
    {
        return new RunOptions();
    }

    #endregion Helper methods
}
