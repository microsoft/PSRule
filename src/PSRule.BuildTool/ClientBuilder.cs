// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using PSRule.BuildTool.Resources;

namespace PSRule.BuildTool;

internal sealed class ClientBuilder
{
    private readonly Option<bool> _Global_Debug;
    private readonly Option<string> _Badge_OutputPath;

    private ClientBuilder(RootCommand cmd)
    {
        Command = cmd;

        // Global options.
        _Global_Debug = new Option<bool>("--debug")
        {
            Description = "Enable debug logging.",
            Recursive = true,
        };

        // Options for the badge command.
        _Badge_OutputPath = new Option<string>("--output-path")
        {
            // Description = CmdStrings.Badge_OutputPath_Description,
        };

        Command.Options.Add(_Global_Debug);
    }

    /// <summary>
    /// Gets the configured root command.
    /// </summary>
    public RootCommand Command { get; }

    public static RootCommand New()
    {
        var cmd = new RootCommand("PSRule Build Tool")
        {

        };
        var builder = new ClientBuilder(cmd);

        builder.AddBadgeResource();

        return builder.Command;
    }

    /// <summary>
    /// Add badge resource.
    /// This API is not supported on operating systems other than Windows 6.1 or later.
    /// </summary>
    public void AddBadgeResource()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;

        var cmd = new Command("badge", CmdStrings.Badge_Description)
        {
            _Badge_OutputPath,
        };

        cmd.SetAction(async (parse, cancellationToken) =>
        {
            var option = new BadgeResourceOption
            {
                OutputPath = parse.GetValue(_Badge_OutputPath),
            };

#pragma warning disable CA1416
            return BadgeResource.Build(option, GetClientContext(parse));
#pragma warning restore CA1416
        });

        Command.Add(cmd);
    }

    private ClientContext GetClientContext(ParseResult parseResult)
    {
        var debug = parseResult.GetValue(_Global_Debug);

        return new ClientContext(debug);
    }
}
