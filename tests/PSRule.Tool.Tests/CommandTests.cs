// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PSRule.Tool;

public sealed class CommandTests
{
    [Fact]
    public async Task Analyze()
    {
        var console = new TestConsole();
        var builder = ClientBuilder.New();
        Assert.NotNull(builder.Subcommands.FirstOrDefault(c => c.Name == "analyze"));

        await builder.InvokeAsync("analyze", console);
        var output = console.Out.ToString();

        Assert.NotNull(output);
        Assert.Contains($"Using PSRule v0.0.1{System.Environment.NewLine}", output);
    }

    [Fact]
    public async Task Restore()
    {
        var console = new TestConsole();
        var builder = ClientBuilder.New();
        Assert.NotNull(builder.Subcommands.FirstOrDefault(c => c.Name == "restore"));

        await builder.InvokeAsync("restore", console);

        var output = console.Out.ToString();
        Assert.NotNull(output);
    }
}
