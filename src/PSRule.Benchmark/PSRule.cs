using BenchmarkDotNet.Attributes;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Linq;
using BenchmarkDotNet.Engines;
using System.IO;
using System.Reflection;

namespace PSRule.Benchmark
{
    /// <summary>
    /// Define a set of benchmarks for performance testing PSRule internals.
    /// </summary>
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class PSRule
    {
        private PSObject[] _TargetObject;
        private InvokeRulePipeline _Invoke;

        public sealed class TargetObject
        {
            public TargetObject(string name, string message)
            {
                Name = name;
                Message = message;
            }

            public string Name { get; private set; }

            public string Message { get; private set; }
        }

        [GlobalSetup]
        public void Prepare()
        {
            var builder = PipelineBuilder.Invoke();
            
            builder.Source(new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Benchmark.Rule.ps1") });
            _Invoke = builder.Build();

            var r = new Random();
            var randomBuffer = new byte[40];
            var targetObjects = new List<PSObject>();
            while (targetObjects.Count < 1000)
            {
                r.NextBytes(randomBuffer);
                var o = new TargetObject(name: targetObjects.Count.ToString(), message: Convert.ToBase64String(randomBuffer));
                targetObjects.Add(PSObject.AsPSObject(o));
            }

            _TargetObject = targetObjects.ToArray();
        }

        [Benchmark]
        public void Invoke() => _Invoke.Process(_TargetObject).Consume(new Consumer());
    }
}
