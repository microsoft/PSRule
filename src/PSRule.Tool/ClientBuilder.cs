// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.IO;
using PSRule.Tool.Resources;

namespace PSRule.Tool
{
    internal sealed class ClientBuilder
    {
        private readonly Option<string> _Option;
        private readonly Option<bool> _Verbose;
        private readonly Option<bool> _Debug;
        private readonly Option<string[]> _Path;
        private readonly Option<DirectoryInfo> _OutputPath;
        private readonly Option<string> _OutputFormat;
        private readonly Option<string[]> _InputPath;
        private readonly Option<string[]> _Module;
        private readonly Option<string> _Baseline;

        private ClientBuilder(RootCommand cmd)
        {
            Command = cmd;
            _Option = new Option<string>(
                new string[] { "--option" },
                CmdStrings.Options_Option_Description
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
                new string[] { "-p", "--path" }
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
                new string[] { "-m", "--module" }
            );
            _Baseline = new Option<string>(
                new string[] { "--baseline" }
            );

            cmd.AddGlobalOption(_Option);
            cmd.AddGlobalOption(_Verbose);
            cmd.AddGlobalOption(_Debug);
        }

        public RootCommand Command { get; }

        public static Command New()
        {
            var cmd = new RootCommand(CmdStrings.Cmd_Description)
            {
                Name = "ps-rule"
            };
            var builder = new ClientBuilder(cmd);
            builder.AddAnalyze();
            builder.AddRestore();
            return builder.Command;
        }

        private void AddAnalyze()
        {
            var cmd = new Command("analyze", CmdStrings.Analyze_Description);
            cmd.AddOption(_Path);
            cmd.AddOption(_OutputPath);
            cmd.AddOption(_OutputFormat);
            cmd.AddOption(_InputPath);
            cmd.AddOption(_Module);
            cmd.AddOption(_Baseline);
            cmd.SetHandler((invocation) =>
            {
                var option = new AnalyzerOptions
                {
                    Path = invocation.ParseResult.GetValueForOption(_Path),
                    InputPath = invocation.ParseResult.GetValueForOption(_InputPath),
                    Module = invocation.ParseResult.GetValueForOption(_Module),
                    Option = invocation.ParseResult.GetValueForOption(_Option),
                    Baseline = invocation.ParseResult.GetValueForOption(_Baseline),
                    Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                    Debug = invocation.ParseResult.GetValueForOption(_Debug),
                };
                var client = new ClientContext();
                ClientHelper.RunAnalyze(option, client, invocation);
            });
            Command.AddCommand(cmd);
        }

        private void AddRestore()
        {
            var cmd = new Command("restore", CmdStrings.Restore_Description);
            cmd.AddOption(_Path);
            cmd.SetHandler((invocation) =>
            {
                var option = new RestoreOptions
                {
                    Path = invocation.ParseResult.GetValueForOption(_Path),
                    Option = invocation.ParseResult.GetValueForOption(_Option),
                    Verbose = invocation.ParseResult.GetValueForOption(_Verbose),
                    Debug = invocation.ParseResult.GetValueForOption(_Debug),
                };
                var client = new ClientContext();
                ClientHelper.RunRestore(option, client, invocation);
            });
            Command.AddCommand(cmd);
        }
    }
}
