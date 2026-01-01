// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Reflection;
using PSRule.CommandLine;
using PSRule.CommandLine.Commands;
using PSRule.CommandLine.Models;
using PSRule.Tool.Resources;

namespace PSRule.Tool;

/// <summary>
/// A helper to build the command-line commands and options offered to the caller.
/// </summary>
internal sealed class ClientBuilder
{
    private const string ARG_FORCE = "--force";

    internal static readonly string? Version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    private readonly Option<string> _Global_Option;
    private readonly Option<bool> _Global_Verbose;
    private readonly Option<bool> _Global_Debug;
    private readonly Option<bool> _Global_WaitForDebugger;
    private readonly Option<bool> _Global_InGitHubActions;
    private readonly Option<bool> _Module_Restore_Force;
    private readonly Option<bool> _Module_Init_Force;
    private readonly Option<string> _Module_Add_Version;
    private readonly Option<bool> _Module_Add_Force;
    private readonly Option<bool> _Module_Add_SkipVerification;
    private readonly Option<bool> _Module_Prerelease;
    private readonly Option<string[]> _Global_Path;
    private readonly Option<string> _Run_OutputPath;
    private readonly Option<string> _Run_OutputFormat;
    private readonly Option<string[]> _Run_InputPath;
    private readonly Option<string[]> _Run_Module;
    private readonly Option<string[]> _Run_Name;
    private readonly Option<string> _Run_Baseline;
    private readonly Option<string[]> _Run_Formats;
    private readonly Option<string[]> _Run_Convention;
    private readonly Option<string[]> _Run_Outcome;
    private readonly Option<bool> _Run_NoRestore;
    private readonly Option<string> _Run_JobSummaryPath;

    private ClientBuilder(RootCommand cmd, IConsole? console = null)
    {
        Command = cmd;
        Console = console ?? new CommandLine.Console();

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
            Recursive = true,
        };

        // Arguments that are hidden because they are intercepted early in the process.
        _Global_WaitForDebugger = new Option<bool>("--wait-for-debugger")
        {
            Description = string.Empty,
            Recursive = true,
            Hidden = true,
        };
        _Global_InGitHubActions = new Option<bool>("--in-github-actions")
        {
            Description = string.Empty,
            Recursive = true,
            Hidden = true,
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
        _Run_Name = new Option<string[]>("--name")
        {
            Description = CmdStrings.Run_Name_Description,
        };
        _Run_Baseline = new Option<string>("--baseline")
        {
            Description = CmdStrings.Run_Baseline_Description,
        };
        _Run_Formats = new Option<string[]>("--formats")
        {
            Description = CmdStrings.Run_Formats_Description,
            AllowMultipleArgumentsPerToken = true,
        };
        _Run_Convention = new Option<string[]>("--convention")
        {
            Description = CmdStrings.Run_Convention_Description,
        };
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
        _Run_JobSummaryPath = new Option<string>("--job-summary-path")
        {
            Description = CmdStrings.Run_JobSummaryPath_Description,
        };

        // Options for the module command.
        _Module_Init_Force = new Option<bool>(ARG_FORCE)
        {
            Description = CmdStrings.Module_Init_Force_Description,
        };
        _Module_Add_Version = new Option<string>("--version")
        {
            Description = CmdStrings.Module_Add_Version_Description,
        };
        _Module_Add_Force = new Option<bool>(ARG_FORCE)
        {
            Description = CmdStrings.Module_Add_Force_Description,
        };
        _Module_Add_SkipVerification = new Option<bool>("--skip-verification")
        {
            Description = CmdStrings.Module_Add_SkipVerification_Description,
        };
        _Module_Prerelease = new Option<bool>("--prerelease")
        {
            Description = CmdStrings.Module_Prerelease_Description,
        };
        _Module_Restore_Force = new Option<bool>(ARG_FORCE)
        {
            Description = CmdStrings.Module_Restore_Force_Description,
        };

        cmd.Options.Add(_Global_Option);
        cmd.Options.Add(_Global_Verbose);
        cmd.Options.Add(_Global_Debug);
        cmd.Options.Add(_Global_Path);
        cmd.Options.Add(_Global_WaitForDebugger);
        cmd.Options.Add(_Global_InGitHubActions);
    }

    public RootCommand Command { get; }

    public IConsole Console { get; }

    public static Command New(IConsole? console = null)
    {
        var cmd = new RootCommand(string.Concat(CmdStrings.Cmd_Description, " v", Version))
        {

        };
        var builder = new ClientBuilder(cmd, console);
        builder.AddRun();
        builder.AddModule();
        builder.AddRestore();
        return builder.Command;
    }

    /// <summary>
    /// Add the <c>run</c> command.
    /// </summary>
    private void AddRun()
    {
        var cmd = new Command("run", CmdStrings.Run_Description)
        {
            _Run_OutputPath,
            _Run_OutputFormat,
            _Run_InputPath,
            _Run_Module,
            _Run_Name,
            _Run_Baseline,
            _Run_Formats,
            _Run_Outcome,
            _Run_NoRestore,
            _Run_JobSummaryPath,
        };

        cmd.SetAction(async (parse, cancellationToken) =>
        {
            var option = new RunOptions
            {
                Path = parse.GetValue(_Global_Path),
                InputPath = parse.GetValue(_Run_InputPath),
                Module = parse.GetValue(_Run_Module),
                Name = parse.GetValue(_Run_Name),
                Baseline = parse.GetValue(_Run_Baseline),
                Formats = parse.GetValue(_Run_Formats),
                Convention = parse.GetValue(_Run_Convention),
                Outcome = parse.GetValue(_Run_Outcome).ToRuleOutcome(),
                OutputPath = parse.GetValue(_Run_OutputPath),
                OutputFormat = parse.GetValue(_Run_OutputFormat).ToOutputFormat(),
                NoRestore = parse.GetValue(_Run_NoRestore),
                JobSummaryPath = parse.GetValue(_Run_JobSummaryPath),
            };

            var result = await RunCommand.RunAsync(option, GetClientContext(parse), cancellationToken);
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

        };

        var requiredModuleArg = new Argument<string[]>("module")
        {
            Description = CmdStrings.Module_RequiredModule_Description,
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
                Force = parse.GetValue(_Module_Init_Force),
                SkipVerification = parse.GetValue(_Module_Add_SkipVerification),
            };

            return await ModuleCommand.ModuleInitAsync(option, GetClientContext(parse), cancellationToken);
        });

        // List
        var list = new Command("list", CmdStrings.Module_List_Description);
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
            _Module_Prerelease,
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
                Prerelease = parse.GetValue(_Module_Prerelease),
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
        var optionalModuleArg = new Argument<string[]>("module")
        {
            Description = CmdStrings.Module_OptionalModule_Description,
            Arity = ArgumentArity.ZeroOrMore,
        };

        var upgrade = new Command("upgrade", CmdStrings.Module_Upgrade_Description)
        {
            optionalModuleArg,
            _Module_Prerelease,
        };
        upgrade.SetAction(async (parse, cancellationToken) =>
        {
            var option = new ModuleOptions
            {
                Path = parse.GetValue(_Global_Path),
                Module = parse.GetValue(requiredModuleArg),
                Prerelease = parse.GetValue(_Module_Prerelease),
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

    /// <summary>
    /// Add the <c>restore</c> command.
    /// </summary>
    private void AddRestore()
    {
        var restore = new Command("restore", CmdStrings.Restore_Description)
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

        Command.Add(restore);
    }

    private ClientContext GetClientContext(ParseResult parseResult)
    {
        var option = parseResult.GetValue(_Global_Option).TrimQuotes();
        var verbose = parseResult.GetValue(_Global_Verbose);
        var debug = parseResult.GetValue(_Global_Debug);

        return new ClientContext
        (
            console: Console,
            option: option,
            verbose: verbose,
            debug: debug
        );
    }
}
