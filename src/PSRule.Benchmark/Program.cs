// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if BENCHMARK

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

#endif

using System.Diagnostics;
using Microsoft.Extensions.CommandLineUtils;

namespace PSRule.Benchmark
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "PSRule Benchmark",
                Description = "A runner for testing PSRule performance"
            };

#if !BENCHMARK
            // Do profiling
            DebugProfile(app);
            app.Execute(args);
#endif

#if BENCHMARK
            RunProfile(app);
            app.Execute(args);
#endif
        }

#if BENCHMARK

        private static void RunProfile(CommandLineApplication app)
        {
            var config = ManualConfig.CreateEmpty()
                .AddLogger(ConsoleLogger.Default)
                .AddColumnProvider(DefaultColumnProviders.Instance)
                .AddAnalyser(EnvironmentAnalyser.Default)
                .AddAnalyser(OutliersAnalyser.Default)
                .AddAnalyser(MinIterationTimeAnalyser.Default)
                .AddAnalyser(MultimodalDistributionAnalyzer.Default)
                .AddAnalyser(RuntimeErrorAnalyser.Default)
                .AddAnalyser(ZeroMeasurementAnalyser.Default);

            app.Command("benchmark", cmd =>
            {
                var output = cmd.Option("-o | --output", "The path to store report output.", CommandOptionType.SingleValue);
                cmd.OnExecute(() =>
                {
                    if (output.HasValue())
                    {
                        config.WithArtifactsPath(output.Value());
                    }

                    // Do benchmarks
                    BenchmarkRunner.Run<PSRule>(config);
                    return 0;
                });
                cmd.HelpOption("-? | -h | --help");
            });
            app.HelpOption("-? | -h | --help");
        }

#endif

        private const int DebugIterations = 100;

        private static void DebugProfile(CommandLineApplication app)
        {
            app.Command("benchmark", cmd =>
            {
                cmd.OnExecute(() =>
                {
                    RunDebug();
                    return 0;
                });
            });
        }

        private static void RunDebug()
        {
            var profile = new PSRule();
            profile.Prepare();

            Console.WriteLine("Press ENTER to start.");
            Console.ReadLine();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.Invoke();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.InvokeIf();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.InvokeType();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.InvokeSummary();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.Assert();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.Get();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.DefaultTargetNameBinding();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.CustomTargetNameBinding();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.NestedTargetNameBinding();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.AssertHasFieldValue();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.PathTokenize();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.PathExpressionBuild();

            ProfileBlock();
            for (var i = 0; i < DebugIterations; i++)
                profile.PathExpressionGet();
        }

        [DebuggerStepThrough]
        private static void ProfileBlock()
        {
            // Do nothing
        }
    }
}
