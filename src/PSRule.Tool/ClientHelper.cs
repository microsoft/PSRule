// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Invocation;
using PSRule.Configuration;
using PSRule.Pipeline;

namespace PSRule.Tool
{
    internal sealed class ClientHelper
    {
        public static void RunAnalyze(AnalyzerOptions analyzerOptions, ClientContext clientContext, InvocationContext invocation)
        {
            var option = GetOption();
            var host = new ClientHost(invocation, analyzerOptions.Verbose, analyzerOptions.Debug);

            var inputPath = analyzerOptions.InputPath == null || analyzerOptions.InputPath.Length == 0 ?
                new string[] { PSRuleOption.GetWorkingPath() } : analyzerOptions.InputPath;
            var builder = CommandLineBuilder.Assert(new string[] { "PSRule.Rules.Azure" }, option, host);
            builder.InputPath(inputPath);

            using var pipeline = builder.Build();
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
        }

        private static PSRuleOption GetOption()
        {
            var option = PSRuleOption.FromFileOrEmpty();
            option.Input.Format = InputFormat.File;
            option.Output.Style = OutputStyle.Client;
            return option;
        }
    }
}
