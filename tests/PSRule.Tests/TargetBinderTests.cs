// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Pipeline;

namespace PSRule;

public sealed class TargetBinderTests
{
    [Fact]
    public void BindTargetObject()
    {
        var binder = GetBinder();
        var targetObject = GetTargetObject();
        binder.Bind(targetObject);

        var m1 = binder.Result("Module1");
        Assert.Equal("Name1", m1.TargetName);
        Assert.Equal("Type1", m1.TargetType);

        var m2 = binder.Result("Module2");
        Assert.Equal("Name2", m2.TargetName);
        Assert.Equal("Type1", m2.TargetType);

        var m0 = binder.Result(".");
        Assert.Equal("Name1", m0.TargetName);
        Assert.Equal("System.Management.Automation.PSCustomObject", m0.TargetType);

        // With specified type
        targetObject = GetTargetObject(targetType: "ManualType");
        binder.Bind(targetObject);

        m1 = binder.Result("Module1");
        Assert.Equal("Name1", m1.TargetName);
        Assert.Equal("Type1", m1.TargetType);

        var m3 = binder.Result("Module3");
        Assert.Equal("Name1", m3.TargetName);
        Assert.Equal("ManualType", m3.TargetType);
    }

    [Fact]
    public void BindJObject()
    {
        var binder = GetBinder();
        var targetObject = new TargetObject(PSObject.AsPSObject(JToken.Parse("{ \"name\": \"Name1\", \"type\": \"Type1\", \"AlternativeName\": \"Name2\", \"AlternativeType\": \"Type2\" }")));
        binder.Bind(targetObject);

        var m1 = binder.Result("Module1");
        Assert.Equal("Name1", m1.TargetName);
        Assert.Equal("Type1", m1.TargetType);

        var m2 = binder.Result("Module2");
        Assert.Equal("Name2", m2.TargetName);
        Assert.Equal("Type1", m2.TargetType);

        var m0 = binder.Result(".");
        Assert.Equal("Name1", m0.TargetName);
        Assert.Equal("System.Management.Automation.PSCustomObject", m0.TargetType);
    }

    #region Helper methods

    private static TargetObject GetTargetObject(string targetType = null)
    {
        var pso = new PSObject();
        pso.Properties.Add(new PSNoteProperty("name", "Name1"));
        pso.Properties.Add(new PSNoteProperty("type", "Type1"));
        pso.Properties.Add(new PSNoteProperty("AlternativeName", "Name2"));
        pso.Properties.Add(new PSNoteProperty("AlternativeType", "Type2"));
        return new TargetObject(pso, targetType: targetType);
    }

    private static ITargetBinder GetBinder()
    {
        var builder = new TargetBinderBuilder(PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, null);
        var option = new OptionContextBuilder();

        option.ModuleConfig("Module1", new ModuleConfigV1Spec
        {
            Binding = new BindingOption
            {
                TargetName = new[] { "name" },
                TargetType = new[] { "type" }
            }
        });

        option.ModuleConfig("Module2", new ModuleConfigV1Spec
        {
            Binding = new BindingOption
            {
                TargetName = new[] { "AlternativeName" },
                TargetType = new[] { "type" }
            }
        });

        option.ModuleConfig("Module3", new ModuleConfigV1Spec
        {
            Binding = new BindingOption
            {
                TargetName = new[] { "name" },
                TargetType = new[] { "type" },
                PreferTargetInfo = true
            }
        });

        var scopes = new Runtime.LanguageScopeSet(null);

        scopes.Import("Module1", out var module1);
        module1.Configure(option.Build(module1.Name));
        builder.With(module1);

        scopes.Import("Module2", out var module2);
        module2.Configure(option.Build(module2.Name));
        builder.With(module2);

        scopes.Import("Module3", out var module3);
        module3.Configure(option.Build(module3.Name));
        builder.With(module3);

        scopes.Import(".", out var local);
        local.Configure(option.Build(local.Name));
        builder.With(local);
        return builder.Build();
    }

    private static IBaselineV1Spec GetOption(string[] targetName, string[] targetType, bool preferTargetInfo = false)
    {
        var result = new BaselineOption.BaselineInline();
        result.Binding.TargetName = targetName;
        result.Binding.TargetType = targetType;
        result.Binding.PreferTargetInfo = preferTargetInfo;
        return result;
    }

    #endregion Helper methods
}
