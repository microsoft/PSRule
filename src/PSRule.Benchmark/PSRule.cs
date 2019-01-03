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
        private GetRulePipeline _GetPipeline;
        private InvokeRulePipeline _InvokePipeline;
        private InvokeRulePipeline _InvokeIfPipeline;
        private InvokeRulePipeline _InvokeSummaryPipeline;

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
            PrepareGetPipeline();
            PrepareInvokePipeline();
            PrepareInvokeIfPipeline();
            PrepareInvokeSummaryPipeline();
            PrepareTargetObjects();
        }

        private void PrepareGetPipeline()
        {
            var builder = PipelineBuilder.Get();
            builder.Source(new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Benchmark.Rule.ps1") });
            builder.FilterBy(new string[] { "Benchmark" }, null);
            _GetPipeline = builder.Build();
        }

        private void PrepareInvokePipeline()
        {
            var builder = PipelineBuilder.Invoke();
            builder.Source(new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Benchmark.Rule.ps1") });
            builder.FilterBy(new string[] { "Benchmark" }, null);
            _InvokePipeline = builder.Build();
        }

        private void PrepareInvokeIfPipeline()
        {
            var builder = PipelineBuilder.Invoke();
            builder.Source(new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Benchmark.Rule.ps1") });
            builder.FilterBy(new string[] { "BenchmarkIf" }, null);
            _InvokeIfPipeline = builder.Build();
        }

        private void PrepareInvokeSummaryPipeline()
        {
            var builder = PipelineBuilder.Invoke();
            builder.Source(new string[] { Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Benchmark.Rule.ps1") });
            builder.FilterBy(new string[] { "Benchmark" }, null);
            builder.As(Configuration.ResultFormat.Summary);
            _InvokeSummaryPipeline = builder.Build();
        }

        private void PrepareTargetObjects()
        {
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
        public void Invoke() => _InvokePipeline.Process(_TargetObject).Consume(new Consumer());

        [Benchmark]
        public void InvokeIf() => _InvokeIfPipeline.Process(_TargetObject).Consume(new Consumer());

        [Benchmark]
        public void InvokeSummary() => _InvokeSummaryPipeline.Process(_TargetObject);

        [Benchmark]
        public void Get() => _GetPipeline.Process().Consume(new Consumer());
    }
}
