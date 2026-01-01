// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Reflection;
using PSRule.CommandLine;
using PSRule.CommandLine.Commands;
using PSRule.CommandLine.Models;
using PSRule.EditorServices.Commands;
using PSRule.EditorServices.Models;
using PSRule.EditorServices.Resources;
using PSRule.Pipeline;

namespace PSRule.EditorServices;

/// <summary>
/// A helper to build the command-line commands and options offered to the caller.
/// </summary>
internal sealed class ClientBuilder
{
    private const string ARG_FORCE = "--force";

    private static readonly string? _Version = typeof(PipelineBuilder).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    private readonly Option<string> _Global_Option;
    private readonly Option<bool> _Global_Verbose;
    private readonly Option<bool> _Global_Debug;
    private readonly Option<bool> _Module_Restore_Force;
    private readonly Option<bool> _Module_Init_Force;
    private readonly Option<string> _Module_Add_Version;
    private readonly Option<bool> _Module_Add_Force;
    private readonly Option<bool> _Module_Add_SkipVerification;
    private readonly Option<string[]> _Global_Path;
    private readonly Option<string> _Run_OutputPath;
    private readonly Option<string> _Run_OutputFormat;
    private readonly Option<string[]> _Run_InputPath;
    private readonly Option<string[]> _Run_Module;
    private readonly Option<string> _Run_Baseline;
    private readonly Option<string[]> _Run_Formats;
    private readonly Option<string[]> _Run_Outcome;
    private readonly Option<bool> _Run_NoRestore;
    private readonly Option<bool> _Listen_Stdio;
    private readonly Option<string> _Listen_Pipe;

    private ClientBuilder(RootCommand cmd)
    {
        Command = cmd;

        // Global options.
        _Global_Option = new Option<string>("--option")
        {
            Description = CmdStrings.Global_Option_Description,
            DefaultValueFactory = _ => "ps-rule.yaml",
            Recursive = true,
        };
        _Global_Verbose = new Option<bool>("--verbose")
        {
            Description = CmdStrings.Global_Verbose_Description,
            Recursive = true,
        };
        _Global_Debug = new Option<bool>("--debug")
        {
            Description = CmdStrings.Global_Debug_Description,
            Recursive = true,
        };
        _Global_Path = new Option<string[]>("--path", "-p")
        {
            Description = CmdStrings.Global_Path_Description,
        };

        // Options for the run command.
        _Run_OutputPath = new Option<string>("--output-path")
        {
            Description = CmdStrings.Run_OutputPath_Description,
        };
        _Run_OutputFormat = new Option<string>("--output", "-o")
        {
            Description = CmdStrings.Run_OutputFormat_Description,
        };
        _Run_OutputFormat.AcceptOnlyFromAmong("Yaml", "Json", "Markdown", "NUnit3", "Csv", "Sarif");
        _Run_InputPath = new Option<string[]>("--input-path", "-f")
        {
            Description = CmdStrings.Run_InputPath_Description,
        };
        _Run_Module = new Option<string[]>("--module", "-m")
        {
            Description = CmdStrings.Run_Module_Description,
        };
        _Run_Baseline = new Option<string>("--baseline")
        {
            Description = CmdStrings.Run_Baseline_Description,
        };
        _Run_Formats = new Option<string[]>("--formats")
        {
            Description = CmdStrings.Run_Formats_Description,
        };
        _Run_Formats.AllowMultipleArgumentsPerToken = true;
        _Run_Outcome = new Option<string[]>("--outcome")
        {
            Description = CmdStrings.Run_Outcome_Description,
            Arity = ArgumentArity.ZeroOrMore,
        };
        _Run_Outcome.AcceptOnlyFromAmong("Pass", "Fail", "Error", "Processed", "Problem");
        _Run_NoRestore = new Option<bool>("--no-restore")
        {
            Description = CmdStrings.Run_NoRestore_Description,
        };

        // Options for the module command.
        _Module_Init_Force = new Option<bool>(ARG_FORCE)
        {
            Description = CmdStrings.Module_Init_Force_Description
        };
        _Module_Add_Version = new Option<string>("--version")
        {
            Description = CmdStrings.Module_Add_Version_Description
        };
        _Module_Add_Force = new Option<bool>(ARG_FORCE)
        {
            Description = CmdStrings.Module_Add_Force_Description
        };
        _Module_Add_SkipVerification = new Option<bool>("--skip-verification")
        {
            Description = CmdStrings.Module_Add_SkipVerification_Description
        };
        _Module_Restore_Force = new Option<bool>(ARG_FORCE)
        {
            Description = CmdStrings.Module_Restore_Force_Description
        };

        // Options for the listen command.
        _Listen_Stdio = new Option<bool>("--stdio")
        {
            Description = CmdStrings.Listen_Stdio_Description
        };

        _Listen_Pipe = new Option<string>("--pipe")
        {
            Description = CmdStrings.Listen_Pipe_Description
        };

        Command.Options.Add(_Global_Option);
        Command.Options.Add(_Global_Verbose);
        Command.Options.Add(_Global_Debug);
    }

    public RootCommand Command { get; }

    public static Command New()
    {
        var cmd = new RootCommand(string.Concat(CmdStrings.Cmd_Description, " v", _Version))
        {

        };

        var builder = new ClientBuilder(cmd);
        builder.AddRun();
        builder.AddModule();
        builder.AddListen();
        return builder.Command;
    }

    /// <summary>
    /// Add the <c>run</c> command.
    /// </summary>
    private void AddRun()
    {
        var cmd = new Command("run", CmdStrings.Run_Description)
        {
            _Global_Path,
            _Run_OutputPath,
            _Run_OutputFormat,
            _Run_InputPath,
            _Run_Module,
            _Run_Baseline,
            _Run_Formats,
            _Run_Outcome,
            _Run_NoRestore,
        };
        cmd.SetAction(async (parse, cancellationToken) =>
        {
            var option = new RunOptions
            {
                Path = parse.GetValue(_Global_Path),
                InputPath = parse.GetValue(_Run_InputPath),
                Module = parse.GetValue(_Run_Module),
                Baseline = parse.GetValue(_Run_Baseline),
                Formats = parse.GetValue(_Run_Formats),
                Outcome = parse.GetValue(_Run_Outcome).ToRuleOutcome(),
                OutputPath = parse.GetValue(_Run_OutputPath),
                OutputFormat = parse.GetValue(_Run_OutputFormat).ToOutputFormat(),
                NoRestore = parse.GetValue(_Run_NoRestore),
            };
            var client = GetClientContext(parse);

            Environment.TryPathEnvironmentVariable("PSModulePath", out var searchPaths);
            var sp = searchPaths == null ? string.Empty : string.Join(',', searchPaths);

            if (client.Verbose)
            {
                client.Console.Out.WriteLine($"VERBOSE: Using workspace: {Environment.GetWorkingPath()}");
                client.Console.Out.WriteLine($"VERBOSE: Using module search path: {sp}");
                client.Console.Out.WriteLine($"VERBOSE: Using language server: {client.Path}");
                client.Console.Out.WriteLine($"VERBOSE: Using cache path: {client.CachePath}");
            }

            var result = await RunCommand.RunAsync(option, client, cancellationToken);
            return result.ExitCode;
        });
        Command.Add(cmd);
    }

    /// <summary>
    /// Add the <c>module</c> command.
    /// </summary>
    private void AddModule()
    {
        var cmd = new Command("module", CmdStrings.Module_Description)
        {
            _Global_Path,
        };

        var requiredModuleArg = new Argument<string[]>("module")
        {
            Description = CmdStrings.Module_Module_Description,
            Arity = ArgumentArity.OneOrMore,
        };

        // Init
        var init = new Command("init", CmdStrings.Module_Init_Description)
        {
            _Module_Init_Force,
        };
        init.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ModuleOptions
            {
                Path = parse.GetValue(_Global_Path),
                Version = parse.GetValue(_Module_Add_Version),
                Force = parse.GetValue(_Module_Init_Force),
                SkipVerification = parse.GetValue(_Module_Add_SkipVerification),
            };

            return await ModuleCommand.ModuleInitAsync(option, GetClientContext(parse), cancellationToken);
        });

        // List
        var list = new Command
        (
            "list",
            CmdStrings.Module_List_Description
        );
        list.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ModuleOptions
            {
                Path = parse.GetValue(_Global_Path),
                Version = parse.GetValue(_Module_Add_Version),
                Force = parse.GetValue(_Module_Add_Force),
                SkipVerification = parse.GetValue(_Module_Add_SkipVerification),
            };

            return await ModuleCommand.ModuleListAsync(option, GetClientContext(parse), cancellationToken);
        });

        // Add
        var add = new Command("add", CmdStrings.Module_Add_Description)
        {
            requiredModuleArg,
            _Module_Add_Version,
            _Module_Add_Force,
            _Module_Add_SkipVerification,
        };
        add.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ModuleOptions
            {
                Path = parse.GetValue(_Global_Path),
                Module = parse.GetValue(requiredModuleArg),
                Version = parse.GetValue(_Module_Add_Version),
                Force = parse.GetValue(_Module_Add_Force),
                SkipVerification = parse.GetValue(_Module_Add_SkipVerification),
            };

            return await ModuleCommand.ModuleAddAsync(option, GetClientContext(parse), cancellationToken);
        });

        // Remove
        var remove = new Command("remove", CmdStrings.Module_Remove_Description)
        {
            requiredModuleArg,
        };
        remove.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ModuleOptions
            {
                Path = parse.GetValue(_Global_Path),
                Module = parse.GetValue(requiredModuleArg),
            };

            return await ModuleCommand.ModuleRemoveAsync(option, GetClientContext(parse), cancellationToken);
        });

        // Upgrade
        var upgrade = new Command("upgrade", CmdStrings.Module_Upgrade_Description);
        upgrade.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ModuleOptions
            {
                Path = parse.GetValue(_Global_Path),
            };

            return await ModuleCommand.ModuleUpgradeAsync(option, GetClientContext(parse), cancellationToken);
        });

        // Restore
        var restore = new Command("restore", CmdStrings.Module_Restore_Description)
        {
            _Module_Restore_Force,
        };

        restore.SetAction(async (parse, cancellationToken) =>
        {
            var option = new RestoreOptions
            {
                Path = parse.GetValue(_Global_Path),
                Force = parse.GetValue(_Module_Restore_Force),
            };

            return await ModuleCommand.ModuleRestoreAsync(option, GetClientContext(parse), cancellationToken);
        });

        cmd.Add(init);
        cmd.Add(list);
        cmd.Add(add);
        cmd.Add(remove);
        cmd.Add(upgrade);
        cmd.Add(restore);

        Command.Add(cmd);
    }

    private void AddListen()
    {
        var cmd = new Command("listen", CmdStrings.Listen_Description)
        {
            _Global_Path,
            _Listen_Stdio,
            _Listen_Pipe,
        };
        cmd.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ListenOptions
            {
                Path = parse.GetValue(_Global_Path),
                Pipe = parse.GetValue(_Listen_Pipe),
                Stdio = parse.GetValue(_Listen_Stdio),
            };

            return await ListenCommand.ListenAsync(option, GetClientContext(parse), cancellationToken);
        });
        Command.Add(cmd);
    }

    private ClientContext GetClientContext(ParseResult parseResult)
    {
        var option = parseResult.GetValue(_Global_Option).TrimQuotes();
        var verbose = parseResult.GetValue(_Global_Verbose);
        var debug = parseResult.GetValue(_Global_Debug);

        if (!string.IsNullOrEmpty(option))
        {
            option = Environment.GetRootedPath(option);
        }

        option ??= Path.Combine(Environment.GetWorkingPath(), "ps-rule.yaml");

        return new ClientContext
        (
            console: new CommandLine.Console(),
            option: option,
            verbose: verbose,
            debug: debug
        );
    }
}
