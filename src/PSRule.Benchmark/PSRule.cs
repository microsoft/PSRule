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
using PSRule.Configuration;

namespace PSRule.Benchmark
{
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
        private InvokeRulePipeline _InvokeTypePipeline;
        private InvokeRulePipeline _InvokeSummaryPipeline;

        [GlobalSetup]
        public void Prepare()
        {
            PrepareGetPipeline();
            PrepareInvokePipeline();
            PrepareInvokeIfPipeline();
            PrepareInvokeTypePipeline();
            PrepareInvokeSummaryPipeline();
            PrepareTargetObjects();
        }

        private void PrepareGetPipeline()
        {
            var option = new PSRuleOption();
            option.Baseline.RuleName = new string[] { "Benchmark" };
            var builder = PipelineBuilder.Get().Configure(option);
            builder.Source(new RuleSource[] { new RuleSource(path: GetSource(), moduleName: null) });
            _GetPipeline = builder.Build();
        }

        private void PrepareInvokePipeline()
        {
            var option = new PSRuleOption();
            option.Baseline.RuleName = new string[] { "Benchmark" };
            var builder = PipelineBuilder.Invoke().Configure(option);
            builder.Source(new RuleSource[] { new RuleSource(path: GetSource(), moduleName: null) });
            _InvokePipeline = builder.Build();
        }

        private void PrepareInvokeIfPipeline()
        {
            var option = new PSRuleOption();
            option.Baseline.RuleName = new string[] { "BenchmarkIf" };
            var builder = PipelineBuilder.Invoke().Configure(option);
            builder.Source(new RuleSource[] { new RuleSource(path: GetSource(), moduleName: null) });
            _InvokeIfPipeline = builder.Build();
        }

        private void PrepareInvokeTypePipeline()
        {
            var option = new PSRuleOption();
            option.Baseline.RuleName = new string[] { "BenchmarkType" };
            var builder = PipelineBuilder.Invoke().Configure(option);
            builder.Source(new RuleSource[] { new RuleSource(path: GetSource(), moduleName: null) });
            _InvokeTypePipeline = builder.Build();
        }

        private void PrepareInvokeSummaryPipeline()
        {
            var option = new PSRuleOption();
            option.Baseline.RuleName = new string[] { "Benchmark" };
            option.Output.As = ResultFormat.Summary;
            var builder = PipelineBuilder.Invoke().Configure(option);
            builder.Source(new RuleSource[] { new RuleSource(path: GetSource(), moduleName: null) });
            _InvokeSummaryPipeline = builder.Build();
        }

        private string GetSource()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Benchmark.Rule.ps1");
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
        public void Invoke() => _InvokePipeline.Process(_TargetObject);

        [Benchmark]
        public void InvokeIf() => _InvokeIfPipeline.Process(_TargetObject);

        [Benchmark]
        public void InvokeType() => _InvokeTypePipeline.Process(_TargetObject);

        [Benchmark]
        public void InvokeSummary() => _InvokeSummaryPipeline.Process(_TargetObject);

        [Benchmark]
        public void Get() => _GetPipeline.Process().Consume(new Consumer());

        [Benchmark]
        public void DefaultTargetNameBinding()
        {
            foreach (var targetObject in _TargetObject)
            {
                PipelineHookActions.DefaultTargetNameBinding(targetObject);
            }
        }

        [Benchmark]
        public void CustomTargetNameBinding()
        {
            foreach (var targetObject in _TargetObject)
            {
                PipelineHookActions.CustomTargetNameBinding(
                    propertyNames: new string[] { "TargetName", "Name" },
                    caseSensitive: true,
                    targetObject: targetObject,
                    next: (o) => { return null; }
                );
            }
        }

        [Benchmark]
        public void NestedTargetNameBinding()
        {
            foreach (var targetObject in _TargetObject)
            {
                PipelineHookActions.NestedTargetNameBinding(
                    propertyNames: new string[] { "TargetName", "Name" },
                    caseSensitive: true,
                    targetObject: targetObject,
                    next: (o) => { return null; }
                );
            }
        }
    }
}
