// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PSRule.Pipeline;

namespace PSRule;

public sealed class ObjectPathTests
{
    [Fact]
    public void UseObjectPath()
    {
        var actual = PipelineReceiverActions.ConvertFromYaml(GetYamlContent(),
            (sourceObject) => PipelineReceiverActions.ReadObjectPath(sourceObject, PipelineReceiverActions.PassThru, "items", true)
        ).ToArray();

        Assert.Equal(2, actual.Length);
        Assert.Equal("TestObject1", actual[0].Value.PropertyValue<string>("targetName"));
        Assert.Equal("Test", actual[0].Value.PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<string>("kind"));
        Assert.Equal(2, actual[1].Value.PropertyValue<PSObject>("spec").PropertyValue<PSObject>("properties").PropertyValue<int>("value2"));
    }

    private static TargetObject GetYamlContent()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ObjectFromNestedFile.yaml");
        return new TargetObject(new PSObject(File.ReadAllText(path)));
    }
}
