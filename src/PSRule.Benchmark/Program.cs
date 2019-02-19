using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.CommandLineUtils;
using PSRule.Pipeline;
using System;

namespace PSRule.Benchmark
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "PSRule Benchmark";
            app.Description = "A runner for testing PSRule performance";

#if !BENCHMARK
            // Do profiling
            DebugProfile();
#endif

#if BENCHMARK
            RunProfile(app);
            app.Execute(args);
#endif
        }

        private static void RunProfile(CommandLineApplication app)
        {
            var config = ManualConfig.CreateEmpty()
                .With(ConsoleLogger.Default)
                .With(DefaultColumnProviders.Instance)
                .With(EnvironmentAnalyser.Default)
                .With(OutliersAnalyser.Default)
                .With(MinIterationTimeAnalyser.Default)
                .With(MultimodalDistributionAnalyzer.Default)
                .With(RuntimeErrorAnalyser.Default)
                .With(ZeroMeasurementAnalyser.Default);

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
                    var summary = BenchmarkRunner.Run<PSRule>(config);

                    return 0;
                });

                cmd.HelpOption("-? | -h | --help");
            });

            app.HelpOption("-? | -h | --help");
        }

        private static void DebugProfile()
        {
            var profile = new PSRule();
            profile.Prepare();

            for (var i = 0; i < 10; i++)
            {
                profile.Invoke();
            }

            for (var i = 0; i < 10; i++)
            {
               profile.InvokeType();
            }

            for (var i = 0; i < 10; i++)
            {
               profile.DefaultTargetNameBinding();
            }

            for (var i = 0; i < 10; i++)
            {
               profile.CustomTargetNameBinding();
            }

            for (var i = 0; i < 10; i++)
            {
               profile.NestedTargetNameBinding();
            }
        }
    }
}
