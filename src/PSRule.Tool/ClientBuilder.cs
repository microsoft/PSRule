// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
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
    private readonly Option<string[]> _Get_Module;
    private readonly Option<string[]> _Get_Name;
    private readonly Option<string> _Get_Baseline;
    private readonly Option<bool> _Get_IncludeDependencies;
    private readonly Option<bool> _Get_NoRestore;

    private ClientBuilder(RootCommand cmd)
    {
        Command = cmd;

        // Global options.
        _Global_Option = new Option<string>(
            ["--option"],
            getDefaultValue: () => "ps-rule.yaml",
            description: CmdStrings.Global_Option_Description
        );
        _Global_Verbose = new Option<bool>(
            ["--verbose"],
            description: CmdStrings.Global_Verbose_Description
        );
        _Global_Debug = new Option<bool>(
            ["--debug"],
            description: CmdStrings.Global_Debug_Description
        );
        _Global_Path = new Option<string[]>(
            ["-p", "--path"],
            description: CmdStrings.Global_Path_Description
        );

        // Arguments that are hidden because they are intercepted early in the process.
        _Global_WaitForDebugger = new Option<bool>(
            ["--wait-for-debugger"],
            description: string.Empty
        );
        _Global_WaitForDebugger.IsHidden = true;
        _Global_InGitHubActions = new Option<bool>(
            ["--in-github-actions"],
            description: string.Empty
        );
        _Global_InGitHubActions.IsHidden = true;

        // Options for the run command.
        _Run_OutputPath = new Option<string>(
            ["--output-path"],
            description: CmdStrings.Run_OutputPath_Description
        );
        _Run_OutputFormat = new Option<string>(
            ["-o", "--output"],
            description: CmdStrings.Run_OutputFormat_Description
        ).FromAmong("Yaml", "Json", "Markdown", "NUnit3", "Csv", "Sarif");
        _Run_InputPath = new Option<string[]>(
            ["-f", "--input-path"],
            description: CmdStrings.Run_InputPath_Description
        );
        _Run_Module = new Option<string[]>(
            ["-m", "--module"],
            description: CmdStrings.Run_Module_Description
        );
        _Run_Name = new Option<string[]>(
            ["--name"],
            description: CmdStrings.Run_Name_Description
        );
        _Run_Baseline = new Option<string>(
            ["--baseline"],
            description: CmdStrings.Run_Baseline_Description
        );
        _Run_Formats = new Option<string[]>(
            ["--formats"],
            description: CmdStrings.Run_Formats_Description
        );
        _Run_Formats.AllowMultipleArgumentsPerToken = true;
        _Run_Convention = new Option<string[]>(
            ["--convention"],
            description: CmdStrings.Run_Convention_Description
        );
        _Run_Outcome = new Option<string[]>(
            ["--outcome"],
            description: CmdStrings.Run_Outcome_Description
        ).FromAmong("Pass", "Fail", "Error", "Processed", "Problem");
        _Run_Outcome.Arity = ArgumentArity.ZeroOrMore;
        _Run_NoRestore = new Option<bool>(
            "--no-restore",
            description: CmdStrings.Run_NoRestore_Description
        );
        _Run_JobSummaryPath = new Option<string>(
            ["--job-summary-path"],
            description: CmdStrings.Run_JobSummaryPath_Description
        );

        // Options for the module command.
        _Module_Init_Force = new Option<bool>(
            [ARG_FORCE],
            description: CmdStrings.Module_Init_Force_Description
        );
        _Module_Add_Version = new Option<string>
        (
            ["--version"],
            description: CmdStrings.Module_Add_Version_Description
        );
        _Module_Add_Force = new Option<bool>(
            [ARG_FORCE],
            description: CmdStrings.Module_Add_Force_Description
        );
        _Module_Add_SkipVerification = new Option<bool>(
            ["--skip-verification"],
            description: CmdStrings.Module_Add_SkipVerification_Description
        );
        _Module_Prerelease = new Option<bool>(
            ["--prerelease"],
            description: CmdStrings.Module_Prerelease_Description
        );
        _Module_Restore_Force = new Option<bool>(
            [ARG_FORCE],
            description: CmdStrings.Module_Restore_Force_Description
        );

        // Options for the get command.
        _Get_Module = new Option<string[]>(
            ["-m", "--module"],
            description: CmdStrings.Run_Module_Description
        );
        _Get_Name = new Option<string[]>(
            ["--name"],
            description: CmdStrings.Run_Name_Description
        );
        _Get_Baseline = new Option<string>(
            ["--baseline"],
            description: CmdStrings.Run_Baseline_Description
        );
        _Get_IncludeDependencies = new Option<bool>(
            ["--include-dependencies"],
            description: "Include rule dependencies in the output."
        );
        _Get_NoRestore = new Option<bool>(
            "--no-restore",
            description: "Do not restore modules before getting rules."
        );

        cmd.AddGlobalOption(_Global_Option);
        cmd.AddGlobalOption(_Global_Verbose);
        cmd.AddGlobalOption(_Global_Debug);
        cmd.AddGlobalOption(_Global_WaitForDebugger);
        cmd.AddGlobalOption(_Global_InGitHubActions);
    }

    public RootCommand Command { get; }

    public static Command New()
    {
        var cmd = new RootCommand(string.Concat(CmdStrings.Cmd_Description, " v", Version))
        {
            Name = "ps-rule"
        };
        var builder = new ClientBuilder(cmd);
        builder.AddRun();
        builder.AddGet();
        builder.AddModule();
        builder.AddRestore();
        return builder.Command;
    }

    /// <summary>
    /// Add the <c>run</c> command.
    /// </summary>
    private void AddRun()
    {
        var cmd = new Command("run", CmdStrings.Run_Description);
        cmd.AddOption(_Global_Path);
        cmd.AddOption(_Run_OutputPath);
        cmd.AddOption(_Run_OutputFormat);
        cmd.AddOption(_Run_InputPath);
        cmd.AddOption(_Run_Module);
        cmd.AddOption(_Run_Name);
        cmd.AddOption(_Run_Baseline);
        cmd.AddOption(_Run_Formats);
        cmd.AddOption(_Run_Outcome);
        cmd.AddOption(_Run_NoRestore);
        cmd.AddOption(_Run_JobSummaryPath);
        cmd.SetHandler(async (invocation) =>
        {
            var option = new RunOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                InputPath = invocation.ParseResult.GetValueForOption(_Run_InputPath),
                Module = invocation.ParseResult.GetValueForOption(_Run_Module),
                Name = invocation.ParseResult.GetValueForOption(_Run_Name),
                Baseline = invocation.ParseResult.GetValueForOption(_Run_Baseline),
                Formats = invocation.ParseResult.GetValueForOption(_Run_Formats),
                Convention = invocation.ParseResult.GetValueForOption(_Run_Convention),
                Outcome = invocation.ParseResult.GetValueForOption(_Run_Outcome).ToRuleOutcome(),
                OutputPath = invocation.ParseResult.GetValueForOption(_Run_OutputPath),
                OutputFormat = invocation.ParseResult.GetValueForOption(_Run_OutputFormat).ToOutputFormat(),
                NoRestore = invocation.ParseResult.GetValueForOption(_Run_NoRestore),
                JobSummaryPath = invocation.ParseResult.GetValueForOption(_Run_JobSummaryPath),
            };

            var client = GetClientContext(invocation);
            var output = await RunCommand.RunAsync(option, client);
            invocation.ExitCode = output.ExitCode;
        });
        Command.AddCommand(cmd);
    }

    /// <summary>
    /// Add the <c>get</c> command.
    /// </summary>
    private void AddGet()
    {
        var cmd = new Command("get", "Get information about rules and other PSRule resources.");

        // Add the rule subcommand
        var ruleCmd = new Command("rule", "Get rule information including metadata such as tags, labels, and annotations.");
        ruleCmd.AddOption(_Global_Path);
        ruleCmd.AddOption(_Get_Module);
        ruleCmd.AddOption(_Get_Name);
        ruleCmd.AddOption(_Get_Baseline);
        ruleCmd.AddOption(_Get_IncludeDependencies);
        ruleCmd.AddOption(_Get_NoRestore);
        
        ruleCmd.SetHandler(async (invocation) =>
        {
            var option = new GetRuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Module = invocation.ParseResult.GetValueForOption(_Get_Module),
                Name = invocation.ParseResult.GetValueForOption(_Get_Name),
                Baseline = invocation.ParseResult.GetValueForOption(_Get_Baseline),
                IncludeDependencies = invocation.ParseResult.GetValueForOption(_Get_IncludeDependencies),
                NoRestore = invocation.ParseResult.GetValueForOption(_Get_NoRestore),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await GetCommand.GetRuleAsync(option, client);
        });

        cmd.AddCommand(ruleCmd);
        Command.AddCommand(cmd);
    }

    /// <summary>
    /// Add the <c>module</c> command.
    /// </summary>
    private void AddModule()
    {
        var cmd = new Command("module", CmdStrings.Module_Description);

        var requiredModuleArg = new Argument<string[]>
        (
            "module",
            CmdStrings.Module_RequiredModule_Description
        );
        requiredModuleArg.Arity = ArgumentArity.OneOrMore;

        // Init
        var init = new Command
        (
            "init",
            CmdStrings.Module_Init_Description
        );
        init.AddOption(_Module_Init_Force);
        init.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Force = invocation.ParseResult.GetValueForOption(_Module_Init_Force),
                SkipVerification = invocation.ParseResult.GetValueForOption(_Module_Add_SkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleInitAsync(option, client);
        });

        // List
        var list = new Command
        (
            "list",
            CmdStrings.Module_List_Description
        );
        list.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Version = invocation.ParseResult.GetValueForOption(_Module_Add_Version),
                Force = invocation.ParseResult.GetValueForOption(_Module_Add_Force),
                SkipVerification = invocation.ParseResult.GetValueForOption(_Module_Add_SkipVerification),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleListAsync(option, client);
        });

        // Add
        var add = new Command
        (
            "add",
            CmdStrings.Module_Add_Description
        );
        add.AddArgument(requiredModuleArg);
        add.AddOption(_Module_Add_Version);
        add.AddOption(_Module_Add_Force);
        add.AddOption(_Module_Add_SkipVerification);
        add.AddOption(_Module_Prerelease);
        add.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Module = invocation.ParseResult.GetValueForArgument(requiredModuleArg),
                Version = invocation.ParseResult.GetValueForOption(_Module_Add_Version),
                Force = invocation.ParseResult.GetValueForOption(_Module_Add_Force),
                SkipVerification = invocation.ParseResult.GetValueForOption(_Module_Add_SkipVerification),
                Prerelease = invocation.ParseResult.GetValueForOption(_Module_Prerelease),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleAddAsync(option, client);
        });

        // Remove
        var remove = new Command
        (
            "remove",
            CmdStrings.Module_Remove_Description
        );
        remove.AddArgument(requiredModuleArg);
        remove.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Module = invocation.ParseResult.GetValueForArgument(requiredModuleArg),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleRemoveAsync(option, client);
        });

        // Upgrade
        var upgrade = new Command
        (
            "upgrade",
            CmdStrings.Module_Upgrade_Description
        );
        var optionalModuleArg = new Argument<string[]>
        (
            "module",
            CmdStrings.Module_OptionalModule_Description
        );
        optionalModuleArg.Arity = ArgumentArity.ZeroOrMore;
        upgrade.AddArgument(optionalModuleArg);
        upgrade.AddOption(_Module_Prerelease);
        upgrade.SetHandler(async (invocation) =>
        {
            var option = new ModuleOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Module = invocation.ParseResult.GetValueForArgument(requiredModuleArg),
                Prerelease = invocation.ParseResult.GetValueForOption(_Module_Prerelease),
            };

            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleUpgradeAsync(option, client);
        });

        // Restore
        var restore = new Command("restore", CmdStrings.Module_Restore_Description);
        restore.AddOption(_Module_Restore_Force);
        restore.SetHandler(async (invocation) =>
        {
            var option = new RestoreOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Force = invocation.ParseResult.GetValueForOption(_Module_Restore_Force),
            };
            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleRestoreAsync(option, client);
        });

        cmd.AddCommand(init);
        cmd.AddCommand(list);
        cmd.AddCommand(add);
        cmd.AddCommand(remove);
        cmd.AddCommand(upgrade);
        cmd.AddCommand(restore);

        cmd.AddOption(_Global_Path);
        Command.AddCommand(cmd);
    }

    /// <summary>
    /// Add the <c>restore</c> command.
    /// </summary>
    private void AddRestore()
    {
        var restore = new Command("restore", CmdStrings.Restore_Description);
        restore.AddOption(_Module_Restore_Force);
        restore.SetHandler(async (invocation) =>
        {
            var option = new RestoreOptions
            {
                Path = invocation.ParseResult.GetValueForOption(_Global_Path),
                Force = invocation.ParseResult.GetValueForOption(_Module_Restore_Force),
            };
            var client = GetClientContext(invocation);
            invocation.ExitCode = await ModuleCommand.ModuleRestoreAsync(option, client);
        });

        Command.AddCommand(restore);
    }

    private ClientContext GetClientContext(InvocationContext invocation)
    {
        var option = invocation.ParseResult.GetValueForOption(_Global_Option);
        var verbose = invocation.ParseResult.GetValueForOption(_Global_Verbose);
        var debug = invocation.ParseResult.GetValueForOption(_Global_Debug);

        return new ClientContext
        (
            invocation: invocation,
            option: option,
            verbose: verbose,
            debug: debug
        );
    }
}
