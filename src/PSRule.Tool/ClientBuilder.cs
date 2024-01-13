// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using PSRule.Rules;
using PSRule.Tool.Commands;
using PSRule.Tool.Models;
using PSRule.Tool.Resources;

namespace PSRule.Tool;

/// <summary>
/// A helper to build the command-line commands and options offered to the caller.
/// </summary>
internal sealed class ClientBuilder
{
    private const string ARG_FORCE = "--force";

    private static readonly string? _Version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    private readonly Option<string> _Option;
    private readonly Option<bool> _Verbose;
    private readonly Option<bool> _Debug;
    private readonly Option<bool> _ModuleRestoreForce;
    private readonly Option<bool> _ModuleInitForce;
    private readonly Option<string> _ModuleAddVersion;
    private readonly Option<bool> _ModuleAddForce;
    private readonly Option<bool> _ModuleAddSkipVerification;
    private readonly Option<string[]> _Path;
    private readonly Option<DirectoryInfo> _OutputPath;
    private readonly Option<string> _OutputFormat;
    private readonly Option<string[]> _InputPath;
    private readonly Option<string[]> _Module;
    private readonly Option<string> _Baseline;
    private readonly Option<string[]> _Outcome;

    private ClientBuilder(RootCommand cmd)
    {
        Command = cmd;
        _Option = new Option<string>(
            new string[] { "--option" },
            getDefaultValue: () => "ps-rule.yaml",
            description: CmdStrings.Options_Option_Description
        );
        _Verbose = new Option<bool>(
            new string[] { "--verbose" },
            CmdStrings.Options_Verbose_Description
        );
        _Debug = new Option<bool>(
            new string[] { "--debug" },
            CmdStrings.Options_Debug_Description
        );
        _Path = new Option<string[]>(
            new string[] { "-p", "--path" },
            CmdStrings.Options_Path_Description
        );
        _OutputPath = new Option<DirectoryInfo>(
            new string[] { "--output-path" }
        );
        _OutputFormat = new Option<string>(
            new string[] { "-o", "--output" }
        );
        _InputPath = new Option<string[]>(
            new string[] { "-f", "--input-path" }
        );
        _Module = new Option<string[]>(
            new string[] { "-m", "--module" },
            CmdStrings.Options_Module_Description
        );
        _Baseline = new Option<string>(
            new string[] { "--baseline" },
            CmdStrings.Run_Baseline_Description
        );
        _Outcome = new Option<string[]>(
            new string[] { "--outcome" },
            description: CmdStrings.Run_Outcome_Description
        ).FromAmong("Pass", "Fail", "Error", "Processed", "Problem");
        _Outcome.Arity = ArgumentArity.ZeroOrMore;

        // Options for the module command.
        _ModuleInitForce = new Option<bool>(
            new string[] { ARG_FORCE },
            CmdStrings.Module_Init_Force_Description
        );
        _ModuleAddVersion = new Option<string>
        (
            new string[] { "--version" },
            CmdStrings.Module_Add_Version_Description
        );
        _ModuleAddForce = new Option<bool>(
            new string[] { ARG_FORCE },
            CmdStrings.Module_Add_Force_Description
        );
        _ModuleAddSkipVerification = new Option<bool>(
            new string[] { "--skip-verification" },
            CmdStrings.Module_Add_SkipVerification_Description
        );
        _ModuleRestoreForce = new Option<bool>(
            new string[] { ARG_FORCE },
            CmdStrings.Module_Restore_Force_Description
        );

        cmd.AddGlobalOption(_Option);
        cmd.AddGlobalOption(_Verbose);
        cmd.AddGlobalOption(_Debug);
    }

    public RootCommand Command { get; }

    public static Command New()
    {
        var cmd = new RootCommand(string.Concat(CmdStrings.Cmd_Description, " v", _Version))
        {
            Name = "ps-rule"
        };
        var builder = new ClientBuilder(cmd);
        builder.AddRun();
        builder.AddModule();
        return builder.Command;
    }

    /// <summary>
    /// Add the <c>run</c> command.
    /// </summary>
    private void AddRun()
    {
        var cmd = new Command("run", CmdStrings.Run_Description);
        cmd.AddOption(_Path);
        cmd.AddOption(_OutputPath);
        cmd.AddOption(_OutputFormat);
        cmd.AddOption(_InputPath);
        cmd.AddOption(_Module);
        cmd.AddOption(_Baseline);
        cmd.AddOption(_Outcome);
        cmd.SetHandler((invocation) =>
        {
            var option = new RunOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                InputPath = invocation.ParseResult.GetValueForOption(_InputPath),
                Module = invocation.ParseResult.GetValueForOption(_Module),
                Baseline = invocation.ParseResult.GetValueForOption(_Baseline),
                Outcome = ParseOutcome(invocation.ParseResult.GetValueForOption(_Outcome)),
            };
            var client = GetClientContext(invocation);
            invocation.ExitCode = RunCommand.Run(option, client);
        });
        Command.AddCommand(cmd);
    }

    /// <summary>
    /// Add the <c>module</c> command.
    /// </summary>
    private void AddModule()
    {
        var cmd = new Command("module", CmdStrings.Module_Description);

        var moduleArg = new Argument<string[]>
        (
            "module",
            CmdStrings.Module_Module_Description
        );
        moduleArg.Arity = ArgumentArity.OneOrMore;

        // Init
        var init = new Command
        (
            "init",
            CmdStrings.Module_Init_Description
        );
        init.AddOption(_ModuleInitForce);
        init.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Version = invocation.ParseResult.GetValueForOption(_ModuleAddVersion),
                Force = invocation.ParseResult.GetValueForOption(_ModuleAddForce),
                SkipVerification = invocation.ParseResult.GetValueForOption(_ModuleAddSkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = ModuleCommand.ModuleInit(option, client);
        });

        // List
        var list = new Command
        (
            "list",
            CmdStrings.Module_List_Description
        );
        list.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Version = invocation.ParseResult.GetValueForOption(_ModuleAddVersion),
                Force = invocation.ParseResult.GetValueForOption(_ModuleAddForce),
                SkipVerification = invocation.ParseResult.GetValueForOption(_ModuleAddSkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = ModuleCommand.ModuleList(option, client);
        });

        // Add
        var add = new Command
        (
            "add",
            CmdStrings.Module_Add_Description
        );
        add.AddArgument(moduleArg);
        add.AddOption(_ModuleAddVersion);
        add.AddOption(_ModuleAddForce);
        add.AddOption(_ModuleAddSkipVerification);
        add.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Module = invocation.ParseResult.GetValueForArgument(moduleArg),
                Version = invocation.ParseResult.GetValueForOption(_ModuleAddVersion),
                Force = invocation.ParseResult.GetValueForOption(_ModuleAddForce),
                SkipVerification = invocation.ParseResult.GetValueForOption(_ModuleAddSkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = ModuleCommand.ModuleAdd(option, client);
        });

        // Remove
        var remove = new Command
        (
            "remove",
            CmdStrings.Module_Remove_Description
        );
        remove.AddArgument(moduleArg);
        remove.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Module = invocation.ParseResult.GetValueForArgument(moduleArg),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = ModuleCommand.ModuleRemove(option, client);
        });

        // Upgrade
        var upgrade = new Command
        (
            "upgrade",
            CmdStrings.Module_Upgrade_Description
        );
        upgrade.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = ModuleCommand.ModuleUpgrade(option, client);
        });

        // Restore
        var restore = new Command("restore", CmdStrings.Module_Restore_Description);
        // restore.AddOption(_Path);
        restore.AddOption(_ModuleRestoreForce);
        restore.SetHandler((invocation) =>
        {
            var option = new RestoreOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Force = invocation.ParseResult.GetValueForOption(_ModuleRestoreForce),
            };
            var client = GetClientContext(invocation);
            invocation.ExitCode = ModuleCommand.ModuleRestore(option, client);
        });

        cmd.AddCommand(init);
        cmd.AddCommand(list);
        cmd.AddCommand(add);
        cmd.AddCommand(remove);
        cmd.AddCommand(upgrade);
        cmd.AddCommand(restore);

        cmd.AddOption(_Path);
        Command.AddCommand(cmd);
    }

    private ClientContext GetClientContext(InvocationContext invocation)
    {
        var option = invocation.ParseResult.GetValueForOption(_Option);
        var verbose = invocation.ParseResult.GetValueForOption(_Verbose);
        var debug = invocation.ParseResult.GetValueForOption(_Debug);

        return new ClientContext
        (
            invocation: invocation,
            option: option,
            verbose: verbose,
            debug: debug
        );
    }

    /// <summary>
    /// Convert string arguments to flags of <see cref="RuleOutcome"/>.
    /// </summary>
    private static RuleOutcome? ParseOutcome(string[]? s)
    {
        var result = RuleOutcome.None;
        for (var i = 0; s != null && i < s.Length; i++)
        {
            if (Enum.TryParse(s[i], ignoreCase: true, result: out RuleOutcome flag))
                result |= flag;
        }
        return result == RuleOutcome.None ? null : result;
    }
}
