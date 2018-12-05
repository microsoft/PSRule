using BenchmarkDotNet.Configs;
#if !DEBUG
using BenchmarkDotNet.Running;
using Microsoft.Extensions.CommandLineUtils;
using PSRule.Pipeline;
#endif

namespace PSRule.Benchmark
{
    internal sealed class Program
    {
        private class BenchmarkConfig : ManualConfig
        {
            public BenchmarkConfig(string artifactsPath)
            {
                ArtifactsPath = artifactsPath;
                
            }
        }

        static void Main(string[] args)
        {

#if DEBUG
            // Do profiling
            RunProfile();
#endif

#if !DEBUG
            var config = DefaultConfig.Instance;

            if (args != null && args.Length == 1)
            {
                config = new BenchmarkConfig(args[0]);
            }

            var app = new CommandLineApplication();

            app.Command("benchmark", cmd =>
            {
                var output = cmd.Option("-o | --output", "The path to store report output.", CommandOptionType.SingleValue);

                cmd.OnExecute(() =>
                {
                    // Do benchmarks
                    var summary = BenchmarkRunner.Run<PSRule>(config);
                    
                    return 0;
                });

                cmd.HelpOption("-? | -h | --help");
            });

            app.HelpOption("-? | -h | --help");
            app.Execute(args);
#endif
        }

        public static void RunProfile()
        {
            var profile = new PSRule();
            profile.Prepare();

            for (var i = 0; i < 1000; i++)
            {
                profile.Invoke();
            }
        }
    }
}
