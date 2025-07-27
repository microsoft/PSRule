// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Runtime.Binding;
using static Xunit.Assert;

namespace PSRule.Binding;

public sealed class TargetBinderTests
{
    private static readonly BindingOption Module1_Binding = new()
    {
        TargetName = ["name"],
        TargetType = ["type"]
    };

    private static readonly BindingOption Module2_Binding = new()
    {
        TargetName = ["AlternativeName"],
        TargetType = ["type"]
    };

    private static readonly BindingOption Module3_Binding = new()
    {
        TargetName = ["name"],
        TargetType = ["type"],
        // PreferTargetInfo = true
    };


    [Fact]
    public void BindTargetObject()
    {
        var targetObject = GetTargetObject();

        var m0 = GetBinder(null).Bind(targetObject);
        Equal("Name1", m0.TargetName);
        Equal("System.Management.Automation.PSCustomObject", m0.TargetType);

        var m1 = GetBinder(Module1_Binding).Bind(targetObject);
        Equal("Name1", m1.TargetName);
        Equal("Type1", m1.TargetType);

        var m2 = GetBinder(Module2_Binding).Bind(targetObject);
        Equal("Name2", m2.TargetName);
        Equal("Type1", m2.TargetType);

        // With specified type
        targetObject = GetTargetObject(targetType: "ManualType");

        m1 = GetBinder(Module1_Binding).Bind(targetObject);
        Equal("Name1", m1.TargetName);
        Equal("ManualType", m1.TargetType);

        var m3 = GetBinder(Module3_Binding).Bind(targetObject);
        Equal("Name1", m3.TargetName);
        Equal("ManualType", m3.TargetType);
    }

    [Fact]
    public void BindJObject()
    {
        var targetObject = new TargetObject(PSObject.AsPSObject(JToken.Parse("{ \"name\": \"Name1\", \"type\": \"Type1\", \"AlternativeName\": \"Name2\", \"AlternativeType\": \"Type2\" }")));

        var m0 = GetBinder(null).Bind(targetObject);
        Equal("Name1", m0.TargetName);
        Equal("System.Management.Automation.PSCustomObject", m0.TargetType);

        var m1 = GetBinder(Module1_Binding).Bind(targetObject);
        Equal("Name1", m1.TargetName);
        Equal("Type1", m1.TargetType);

        var m2 = GetBinder(Module2_Binding).Bind(targetObject);
        Equal("Name2", m2.TargetName);
        Equal("Type1", m2.TargetType);
    }

    #region Helper methods

    private static TargetObject GetTargetObject(string? targetType = null)
    {
        var pso = new PSObject();
        pso.Properties.Add(new PSNoteProperty("name", "Name1"));
        pso.Properties.Add(new PSNoteProperty("type", "Type1"));
        pso.Properties.Add(new PSNoteProperty("AlternativeName", "Name2"));
        pso.Properties.Add(new PSNoteProperty("AlternativeType", "Type2"));
        return new TargetObject(pso, type: targetType);
    }

    private static ITargetBinder GetBinder(BindingOption? bindingOption)
    {
        var builder = new TargetBinderBuilder(PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, null);
        return builder.Build(bindingOption);
    }

    #endregion Helper methods
}
