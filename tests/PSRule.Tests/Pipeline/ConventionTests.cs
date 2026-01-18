// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Conventions;

namespace PSRule.Pipeline;

/// <summary>
/// Unit tests for verifying conventions in a pipeline.
/// </summary>
public sealed class ConventionTests : BaseTests
{
    [Fact]
    public void Invoke_WithConventions_ShouldCallConventions()
    {
        var testObject1 = new TestObject { Name = "TestObject1" };
        var option = GetOption();
        option.Rule.Include = ResourceHelper.GetResourceIdReference(["ConventionTest"]);
        option.Convention.Include = ["Convention1"];

        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var pipeline = builder.Build() as InvokeRulePipeline;

        Assert.NotNull(pipeline);

        // Check conventions have been imported.
        var conventions = pipeline.Context.Pipeline.ResourceCache.OfType<IConventionV1>();
        Assert.NotEmpty(conventions);

        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(testObject1));
        pipeline.End();
    }

    /// <summary>
    /// Test that a convention running the <c>Initialize</c> block has access to global configuration options.
    /// </summary>
    [Fact]
    public void Invoke_WithInitialize_ShouldHaveAccessToGlobalConfigurationOptions()
    {
        var testObject1 = new TestObject { Name = "TestObject1" };
        var option = GetOption();
        option.Configuration.Add("InitializeConfiguration", "InitializeValue");
        option.Rule.Include = ResourceHelper.GetResourceIdReference(["ConventionTest"]);
        option.Convention.Include = ["Convention1"];

        var writer = new TestWriter(option);
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var pipeline = builder.Build(writer);

        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(testObject1));
        pipeline.End();

        var actual1 = writer.Output[0] as InvokeResult;
        var actual2 = actual1.AsRecord()[0].Data["InitializeConfiguration"];
        Assert.Equal("InitializeValue", actual2);
    }

    /// <summary>
    /// Test that a convention running the <c>Begin</c> block has access to global configuration options.
    /// </summary>
    [Fact]
    public void Invoke_WithBegin_ShouldHaveAccessToGlobalConfigurationOptions()
    {
        var testObject1 = new TestObject { Name = "TestObject1" };
        var option = GetOption();
        option.Configuration.Add("BeginConfiguration", "BeginValue");
        option.Rule.Include = ResourceHelper.GetResourceIdReference(["ConventionTest"]);
        option.Convention.Include = ["Convention1"];

        var writer = new TestWriter(option);
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var pipeline = builder.Build(writer);

        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(testObject1));
        pipeline.End();

        var actual1 = writer.Output[0] as InvokeResult;
        var actual2 = actual1.AsRecord()[0].Data["BeginConfiguration"];
        Assert.Equal("BeginValue", actual2);
    }

    // TODO: Check that configuration that is in a run is also accessible within a begin block.

    [Fact]
    public void Invoke_WithConventions_ShouldCallConventionsInOrder()
    {
        var testObject1 = new TestObject { Name = "TestObject1" };
        var option = GetOption();
        option.Rule.Include = ResourceHelper.GetResourceIdReference(["ConventionTest"]);

        // Order 1
        option.Convention.Include = ["Convention1", "Convention2"];
        var writer = new TestWriter(option);
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var pipeline = builder.Build(writer);
        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(testObject1));
        pipeline.End();
        var actual1 = writer.Output[0] as InvokeResult;
        var actual2 = actual1.AsRecord()[0].Data["count"];
        Assert.Equal(110, actual2);

        // Order 2
        option.Convention.Include = ["Convention2", "Convention1"];
        writer = new TestWriter(option);
        builder = PipelineBuilder.Invoke(GetSource(), option, null);
        pipeline = builder.Build(writer);
        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(testObject1));
        pipeline.End();
        actual1 = writer.Output[0] as InvokeResult;
        actual2 = actual1.AsRecord()[0].Data["count"];
        Assert.Equal(11, actual2);
    }

    /// <summary>
    /// Test that localized data is accessible to conventions at each block.
    /// </summary>
    [Fact]
    public void WithLocalizedData()
    {
        var testObject1 = new TestObject { Name = "TestObject1" };
        var option = GetOption();
        option.Rule.Include = ResourceHelper.GetResourceIdReference(["WithLocalizedDataPrecondition"]);
        option.Convention.Include = ["Convention.WithLocalizedData"];
        var writer = new TestWriter(option);
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var pipeline = builder.Build(writer);

        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(testObject1));
        pipeline.End();
        Assert.NotEmpty(writer.Information);
        Assert.Equal("LocalizedMessage for en. Format=Initialize.", writer.Information[0] as string);
        Assert.Equal("LocalizedMessage for en. Format=Begin.", writer.Information[1] as string);
        Assert.Equal("LocalizedMessage for en. Format=Precondition.", writer.Information[2] as string);
        Assert.Equal("LocalizedMessage for en. Format=Process.", writer.Information[3] as string);
        Assert.Equal("LocalizedMessage for en. Format=End.", writer.Information[4] as string);
    }

    #region Helper methods

    private static Source[] GetSource()
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath("FromFileConventions.Rule.ps1"));
        return builder.Build();
    }

    protected override PSRuleOption GetOption()
    {
        var option = new PSRuleOption();
        option.Output.Culture = ["en-US"];
        return option;
    }

    #endregion Helper methods
}
