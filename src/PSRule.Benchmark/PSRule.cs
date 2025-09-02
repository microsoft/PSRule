// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime.ObjectPath;

namespace PSRule.Benchmark;

/// <summary>
/// Define a set of benchmarks for performance testing PSRule internals.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class PSRule
{
    private PSObject[] _TargetObject;
    private BenchmarkHostContext _HostContext;
    private IPipeline _AssertPipeline;
    private IPipeline _AssertHasFieldValuePipeline;
    private IPipeline _GetPipeline;
    private IPipeline _GetHelpPipeline;
    private IPipeline _InvokePipeline;
    private IPipeline _InvokeIfPipeline;
    private IPipeline _InvokeTypePipeline;
    private IPipeline _InvokeSummaryPipeline;
    private IPipeline _InvokeWithinPipeline;
    private IPipeline _InvokeWithinBulkPipeline;
    private IPipeline _InvokeWithinLikePipeline;
    private PathExpressionBuilder _PathExpressionBuilder;
    private IPathToken[] _PathExpressionTokens;
    private PathExpression _PathExpression;

    [GlobalSetup]
    public void Prepare()
    {
        _HostContext = new BenchmarkHostContext();
        PrepareGetPipeline();
        PrepareGetHelpPipeline();
        PrepareInvokePipeline();
        PrepareInvokeIfPipeline();
        PrepareInvokeTypePipeline();
        PrepareInvokeSummaryPipeline();
        PrepareAssertPipeline();
        PrepareInvokeWithinPipeline();
        PrepareInvokeWithinBulkPipeline();
        PrepareInvokeWithinLikePipeline();
        PrepareTargetObjects();
        PrepareAssertHasFieldValuePipeline();
        PreparePathExpressionBuild();
        PreparePathExpressionSelect();
    }

    private void PrepareGetPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "Benchmark" };
        var builder = PipelineBuilder.Get(GetSource(), option, _HostContext);
        _GetPipeline = builder.Build();
    }

    private void PrepareGetHelpPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "BenchmarkHelp" };
        option.Output.Culture = new string[] { "en-ZZ" };
        var builder = PipelineBuilder.GetHelp(GetSource(), option, _HostContext);
        _GetHelpPipeline = builder.Build();
    }

    private void PrepareInvokePipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "Benchmark" };
        var builder = PipelineBuilder.Invoke(GetSource(), option, _HostContext);
        _InvokePipeline = builder.Build();
    }

    private void PrepareInvokeIfPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "BenchmarkIf" };
        var builder = PipelineBuilder.Invoke(GetSource(), option, _HostContext);
        _InvokeIfPipeline = builder.Build();
    }

    private void PrepareInvokeTypePipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "BenchmarkType" };
        var builder = PipelineBuilder.Invoke(GetSource(), option, _HostContext);
        _InvokeTypePipeline = builder.Build();
    }

    private void PrepareInvokeSummaryPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "Benchmark" };
        option.Output.As = ResultFormat.Summary;
        var builder = PipelineBuilder.Invoke(GetSource(), option, _HostContext);
        _InvokeSummaryPipeline = builder.Build();
    }

    private void PrepareAssertPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "Benchmark" };
        var builder = PipelineBuilder.Assert(GetSource(), option, _HostContext);
        _AssertPipeline = builder.Build();
    }

    private void PrepareInvokeWithinPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "BenchmarkWithin" };
        var builder = PipelineBuilder.Invoke(GetWithinSource(), option, _HostContext);
        _InvokeWithinPipeline = builder.Build();
    }

    private void PrepareInvokeWithinBulkPipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "BenchmarkWithinBulk" };
        var builder = PipelineBuilder.Invoke(GetWithinSource(), option, _HostContext);
        _InvokeWithinBulkPipeline = builder.Build();
    }

    private void PrepareInvokeWithinLikePipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "BenchmarkWithinLike" };
        var builder = PipelineBuilder.Invoke(GetWithinSource(), option, _HostContext);
        _InvokeWithinLikePipeline = builder.Build();
    }

    private void PrepareAssertHasFieldValuePipeline()
    {
        var option = new PSRuleOption();
        option.Rule.Include = new string[] { "Assert.HasFieldValue" };
        var builder = PipelineBuilder.Invoke(GetSource(), option, _HostContext);
        _AssertHasFieldValuePipeline = builder.Build();
    }

    private void PreparePathExpressionBuild()
    {
        _PathExpressionBuilder = new PathExpressionBuilder();
        _PathExpressionTokens = PathTokenizer.Get("$.Properties.logs[?@.enabled && @.enabled==true].category");
    }

    private void PreparePathExpressionSelect()
    {
        _PathExpression = PathExpression.Create("$.Properties.logs[?@.enabled && @.enabled==true].category");
    }

    private Source[] GetSource()
    {
        var builder = new SourcePipelineBuilder(_HostContext, null);
        builder.Directory(GetSourcePath("Benchmark.Rule.ps1"));
        return builder.Build();
    }

    private Source[] GetWithinSource()
    {
        var builder = new SourcePipelineBuilder(_HostContext, null);
        builder.Directory(GetSourcePath("Benchmark.Within.Rule.ps1"));
        return builder.Build();
    }

    private static string GetSourcePath(string fileName)
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), fileName);
    }

    private void PrepareTargetObjects()
    {
        var r = new Random();
        var randomBuffer = new byte[40];
        var targetObjects = new List<PSObject>();
        while (targetObjects.Count < 1000)
        {
            r.NextBytes(randomBuffer);
            var value = (targetObjects.Count & 1) == 1 ? "Microsoft.Compute/virtualMachines" : "Microsoft.Sql/servers/databases";
            var o = new TargetObject(name: targetObjects.Count.ToString(), message: Convert.ToBase64String(randomBuffer), value: value);
            targetObjects.Add(PSObject.AsPSObject(o));
        }
        _TargetObject = targetObjects.ToArray();
    }

    [Benchmark]
    public void Invoke()
    {
        RunPipelineTargets(_InvokePipeline);
    }

    [Benchmark]
    public void InvokeIf()
    {
        RunPipelineTargets(_InvokeIfPipeline);
    }

    [Benchmark]
    public void InvokeType()
    {
        RunPipelineTargets(_InvokeTypePipeline);
    }

    [Benchmark]
    public void InvokeSummary()
    {
        RunPipelineTargets(_InvokeSummaryPipeline);
    }

    [Benchmark]
    public void Assert()
    {
        RunPipelineTargets(_AssertPipeline);
    }

    [Benchmark]
    public void Get()
    {
        RunPipelineNull(_GetPipeline);
    }

    [Benchmark]
    public void GetHelp()
    {
        RunPipelineNull(_GetHelpPipeline);
    }

    [Benchmark]
    public void Within()
    {
        RunPipelineTargets(_InvokeWithinPipeline);
    }

    [Benchmark]
    public void WithinBulk()
    {
        RunPipelineTargets(_InvokeWithinBulkPipeline);
    }

    [Benchmark]
    public void WithinLike()
    {
        RunPipelineTargets(_InvokeWithinLikePipeline);
    }

    [Benchmark]
    public void DefaultTargetNameBinding()
    {
        for (var i = 0; i < _TargetObject.Length; i++)
            PipelineHookActions.BindTargetName(null, false, _TargetObject[i], out _);
    }

    [Benchmark]
    public void CustomTargetNameBinding()
    {
        for (var i = 0; i < _TargetObject.Length; i++)
            PipelineHookActions.BindTargetName(
                propertyNames: ["TargetName", "Name"],
                caseSensitive: true,
                targetObject: _TargetObject[i],
                path: out _
            );
    }

    [Benchmark]
    public void NestedTargetNameBinding()
    {
        for (var i = 0; i < _TargetObject.Length; i++)
            PipelineHookActions.BindTargetName(
                propertyNames: ["TargetName", "Name"],
                caseSensitive: true,
                targetObject: _TargetObject[i],
                path: out _
            );
    }

    [Benchmark]
    public void AssertHasFieldValue()
    {
        RunPipelineTargets(_AssertHasFieldValuePipeline);
    }

    [Benchmark]
    public void PathTokenize()
    {
        PathTokenizer.Get("$.Properties.logs[?@.enabled && @.enabled==true].category");
    }

    [Benchmark]
    public void PathExpressionBuild()
    {
        _PathExpressionBuilder.Build(_PathExpressionTokens);
    }

    [Benchmark]
    public void PathExpressionGet()
    {
        for (var i = 0; i < _TargetObject.Length; i++)
            _PathExpression.TryGet(_TargetObject[i], false, out object _);
    }

    private void RunPipelineNull(IPipeline pipeline)
    {
        pipeline.Begin();
        pipeline.Process(null);
        pipeline.End();
    }

    private void RunPipelineTargets(IPipeline pipeline)
    {
        pipeline.Begin();

        for (var i = 0; i < _TargetObject.Length; i++)
            pipeline.Process(_TargetObject[i]);

        pipeline.End();
    }
}
