// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Reflection;
using PSRule.Rules;
using PSRule.Tool.Resources;

namespace PSRule.Tool;

internal sealed class ClientBuilder
{
    private static readonly string? _Version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    private readonly Option<string> _Option;
    private readonly Option<bool> _Verbose;
    private readonly Option<bool> _Debug;
    private readonly Option<bool> _RestoreForce;
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

        _RestoreForce = new Option<bool>(
            new string[] { "--force" },
            CmdStrings.Restore_Force_Description
        );
        _ModuleAddVersion = new Option<string>
        (
            new string[] { "--version" },
            CmdStrings.Module_Add_Version_Description
        );
        _ModuleAddForce = new Option<bool>(
            new string[] { "--force" },
            CmdStrings.Module_Add_Force_Description
        );
        _ModuleAddSkipVerification = new Option<bool>(
            new string[] { "--skip-verification" },
            CmdStrings.Module_Add_SkipVerification_Description
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
        builder.AddRestore();
        return builder.Command;
    }

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
                Option = invocation.ParseResult.GetValueForOption(_Option),
                Baseline = invocation.ParseResult.GetValueForOption(_Baseline),
                Outcome = ParseOutcome(invocation.ParseResult.GetValueForOption(_Outcome)),
                Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                Debug = invocation.ParseResult.GetValueForOption(_Debug),
            };
            var client = new ClientContext();
            invocation.ExitCode = ClientHelper.RunAnalyze(option, client, invocation);
        });
        Command.AddCommand(cmd);
    }

    private void AddRestore()
    {
        var cmd = new Command("restore", CmdStrings.Restore_Description);
        cmd.AddOption(_Path);
        cmd.AddOption(_RestoreForce);
        cmd.SetHandler((invocation) =>
        {
            var option = new RestoreOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Option = invocation.ParseResult.GetValueForOption(_Option),
                Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                Debug = invocation.ParseResult.GetValueForOption(_Debug),
                Force = invocation.ParseResult.GetValueForOption(_RestoreForce),
            };
            var client = new ClientContext();
            invocation.ExitCode = ClientHelper.RunRestore(option, client, invocation);
        });
        Command.AddCommand(cmd);
    }

    private void AddModule()
    {
        var cmd = new Command("module", CmdStrings.Module_Description);

        var moduleArg = new Argument<string[]>
        (
            "module",
            CmdStrings.Module_Module_Description
        );
        moduleArg.Arity = ArgumentArity.OneOrMore;

        // Add
        var add = new Command(
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
                Option = invocation.ParseResult.GetValueForOption(_Option),
                Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                Debug = invocation.ParseResult.GetValueForOption(_Debug),
                Module = invocation.ParseResult.GetValueForArgument(moduleArg),
                Version = invocation.ParseResult.GetValueForOption(_ModuleAddVersion),
                Force = invocation.ParseResult.GetValueForOption(_ModuleAddForce),
                SkipVerification = invocation.ParseResult.GetValueForOption(_ModuleAddSkipVerification),
            };

            var client = new ClientContext();
            invocation.ExitCode = ClientHelper.AddModule(option, client, invocation);
        });

        // Remove
        var remove = new Command(
            "remove",
            CmdStrings.Module_Remove_Description
        );
        remove.AddArgument(moduleArg);
        remove.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Option = invocation.ParseResult.GetValueForOption(_Option),
                Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                Debug = invocation.ParseResult.GetValueForOption(_Debug),
                Module = invocation.ParseResult.GetValueForArgument(moduleArg)
            };

            var client = new ClientContext();
            invocation.ExitCode = ClientHelper.RemoveModule(option, client, invocation);
        });

        // Upgrade
        var upgrade = new Command(
            "upgrade",
            CmdStrings.Module_Upgrade_Description
        );
        upgrade.SetHandler((invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Path),
                Option = invocation.ParseResult.GetValueForOption(_Option),
                Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                Debug = invocation.ParseResult.GetValueForOption(_Debug)
            };

            var client = new ClientContext();
            invocation.ExitCode = ClientHelper.UpgradeModule(option, client, invocation);
        });

        cmd.AddCommand(add);
        cmd.AddCommand(remove);
        cmd.AddCommand(upgrade);

        cmd.AddOption(_Path);
        Command.AddCommand(cmd);
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
