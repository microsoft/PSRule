// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.IO;

namespace PSRule.Tool;

public sealed class CommandTests
{
    [Fact]
    public async Task Run()
    {
        var console = new TestConsole();
        var builder = ClientBuilder.New();
        Assert.NotNull(builder.Subcommands.FirstOrDefault(c => c.Name == "run"));

        await builder.InvokeAsync("run", console);
        var output = console.Out.ToString();

        Assert.NotNull(output);
        Assert.Contains($"Using PSRule v0.0.1{System.Environment.NewLine}", output);
    }

    [Fact]
    public async Task ModuleInit()
    {
        var console = new TestConsole();
        var builder = ClientBuilder.New();
        var module = builder.Subcommands.FirstOrDefault(c => c.Name == "module");

        Assert.NotNull(module);
        Assert.NotNull(module.Subcommands.FirstOrDefault(c => c.Name == "init"));

        await builder.InvokeAsync("module init", console);

        var output = console.Out.ToString();
        Assert.NotNull(output);
    }

    [Fact]
    public async Task ModuleRestore()
    {
        var console = new TestConsole();
        var builder = ClientBuilder.New();
        var module = builder.Subcommands.FirstOrDefault(c => c.Name == "module");

        Assert.NotNull(module);
        Assert.NotNull(module.Subcommands.FirstOrDefault(c => c.Name == "restore"));

        await builder.InvokeAsync("module restore", console);

        var output = console.Out.ToString();
        Assert.NotNull(output);
    }
}
