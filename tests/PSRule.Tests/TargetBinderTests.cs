// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Definitions.Baselines;
using PSRule.Pipeline;
using Xunit;
using static PSRule.Pipeline.TargetBinder;

namespace PSRule
{
    public sealed class TargetBinderTests
    {
        [Fact]
        public void BindTargetObject()
        {
            var binder = GetBinder();
            var targetObject = GetTargetObject();
            binder.Bind(targetObject);

            var m1 = binder.Using("Module1");
            Assert.Equal("Name1", m1.TargetName);
            Assert.Equal("Type1", m1.TargetType);

            var m2 = binder.Using("Module2");
            Assert.Equal("Name2", m2.TargetName);
            Assert.Equal("Type1", m2.TargetType);

            var m0 = binder.Using(".");
            Assert.Equal("Name1", m0.TargetName);
            Assert.Equal("System.Management.Automation.PSCustomObject", m0.TargetType);
        }

        [Fact]
        public void BindJObject()
        {
            var binder = GetBinder();
            var targetObject = new TargetObject(PSObject.AsPSObject(JToken.Parse("{ \"name\": \"Name1\", \"type\": \"Type1\", \"AlternativeName\": \"Name2\", \"AlternativeType\": \"Type2\" }")));
            binder.Bind(targetObject);

            var m1 = binder.Using("Module1");
            Assert.Equal("Name1", m1.TargetName);
            Assert.Equal("Type1", m1.TargetType);

            var m2 = binder.Using("Module2");
            Assert.Equal("Name2", m2.TargetName);
            Assert.Equal("Type1", m2.TargetType);

            var m0 = binder.Using(".");
            Assert.Equal("Name1", m0.TargetName);
            Assert.Equal("System.Management.Automation.PSCustomObject", m0.TargetType);
        }

        #region Helper methods

        private TargetObject GetTargetObject()
        {
            var pso = new PSObject();
            pso.Properties.Add(new PSNoteProperty("name", "Name1"));
            pso.Properties.Add(new PSNoteProperty("type", "Type1"));
            pso.Properties.Add(new PSNoteProperty("AlternativeName", "Name2"));
            pso.Properties.Add(new PSNoteProperty("AlternativeType", "Type2"));
            return new TargetObject(pso);
        }

        private ITargetBinder GetBinder()
        {
            var builder = new TargetBinderBuilder(PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, null);
            var option = new OptionContext();

            option.Add(new OptionContext.BaselineScope(
                type: OptionContext.ScopeType.Module,
                baselineId: null,
                moduleName: "Module1",
                option: GetOption(
                    targetName: new string[] { "name" },
                    targetType: new string[] { "type" }
                ),
                obsolete: false
            ));

            option.Add(new OptionContext.BaselineScope(
                type: OptionContext.ScopeType.Module,
                baselineId: null,
                moduleName: "Module2",
                option: GetOption(
                    targetName: new string[] { "AlternativeName" },
                    targetType: new string[] { "type" }
                ),
                obsolete: false
            ));

            option.UseScope("Module1");
            builder.With(new TargetBindingContext("Module1", option.GetTargetBinding()));

            option.UseScope("Module2");
            builder.With(new TargetBindingContext("Module2", option.GetTargetBinding()));

            option.UseScope(null);
            builder.With(new TargetBindingContext(".", option.GetTargetBinding()));
            return builder.Build();
        }

        private static IBaselineSpec GetOption(string[] targetName, string[] targetType)
        {
            var result = new BaselineOption.BaselineInline();
            result.Binding.TargetName = targetName;
            result.Binding.TargetType = targetType;
            return result;
        }

        #endregion Helper methods
    }
}
