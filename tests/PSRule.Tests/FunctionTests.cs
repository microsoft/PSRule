// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using Xunit;

namespace PSRule
{
    /// <summary>
    /// Define tests for expression functions are working correctly.
    /// </summary>
    public sealed class FunctionTests
    {
        [Fact]
        public void Concat()
        {
            var context = GetContext();
            var fn = GetFunction("concat");

            var properties = new LanguageExpression.PropertyBag
            {
                { "concat", new object[] { "1", "2", "3" } }
            };
            Assert.Equal("123", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "concat", new object[] { 1, 2, 3 } }
            };
            Assert.Equal("123", fn(context, properties)(context));
        }

        [Fact]
        public void Configuration()
        {
            var context = GetContext();
            var fn = GetFunction("configuration");

            var properties = new LanguageExpression.PropertyBag
            {
                { "configuration", "config1" }
            };
            Assert.Equal("123", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "configuration", "notconfig" }
            };
            Assert.Null(fn(context, properties)(context));
        }

        [Fact]
        public void Boolean()
        {
            var context = GetContext();
            var fn = GetFunction("boolean");

            var properties = new LanguageExpression.PropertyBag
            {
                { "boolean", true }
            };
            Assert.Equal(true, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "boolean", false }
            };
            Assert.Equal(false, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "boolean", "true" }
            };
            Assert.Equal(true, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "boolean", "false" }
            };
            Assert.Equal(false, fn(context, properties)(context));
        }

        [Fact]
        public void Integer()
        {
            var context = GetContext();
            var fn = GetFunction("integer");

            var properties = new LanguageExpression.PropertyBag
            {
                { "integer", 1 }
            };
            Assert.Equal(1, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "integer", -1 }
            };
            Assert.Equal(-1, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "integer", 0 }
            };
            Assert.Equal(0, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "integer", "1" }
            };
            Assert.Equal(1, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "integer", "-1" }
            };
            Assert.Equal(-1, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "integer", "0" }
            };
            Assert.Equal(0, fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "integer", "not" }
            };
            Assert.Equal(0, fn(context, properties)(context));
        }

        [Fact]
        public void String()
        {
            var context = GetContext();
            var fn = GetFunction("string");

            var properties = new LanguageExpression.PropertyBag
            {
                { "string", 1 }
            };
            Assert.Equal("1", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "string", -1 }
            };
            Assert.Equal("-1", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "string", "1" }
            };
            Assert.Equal("1", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "string", "-1" }
            };
            Assert.Equal("-1", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "string", "0" }
            };
            Assert.Equal("0", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "string", "abc" }
            };
            Assert.Equal("abc", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "string", true }
            };
            Assert.Equal("True", fn(context, properties)(context));
        }

        [Fact]
        public void Substring()
        {
            var context = GetContext();
            var fn = GetFunction("substring");

            var properties = new LanguageExpression.PropertyBag
            {
                { "substring", "TestObject" },
                { "length", 7 }
            };
            Assert.Equal("TestObj", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "substring", "TestObject" },
                { "length", "7" }
            };
            Assert.Equal("TestObj", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "substring", 10000 },
                { "length", 2 }
            };
            Assert.Null(fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "substring", "Test" },
                { "length", 100 }
            };
            Assert.Equal("Test", fn(context, properties)(context));
        }

        [Fact]
        public void Path()
        {
            var context = GetContext();
            var fn = GetFunction("path");

            var properties = new LanguageExpression.PropertyBag
            {
                { "path", "name" }
            };
            Assert.Equal("TestObject1", fn(context, properties)(context));

            properties = new LanguageExpression.PropertyBag
            {
                { "path", "notProperty" }
            };
            Assert.Null(fn(context, properties)(context));
        }

        #region Helper methods

        private static ExpressionBuilderFn GetFunction(string name)
        {
            return Functions.Builtin.Single(f => f.Name == name).Fn;
        }

        private static PSRuleOption GetOption()
        {
            var option = new PSRuleOption();
            option.Configuration["config1"] = "123";
            return option;
        }

        private static Source[] GetSource()
        {
            var builder = new SourcePipelineBuilder(null, null);
            builder.Directory(GetSourcePath("Selectors.Rule.yaml"));
            return builder.Build();
        }

        private static ExpressionContext GetContext()
        {
            var targetObject = new PSObject();
            targetObject.Properties.Add(new PSNoteProperty("name", "TestObject1"));
            var context = new Runtime.RunspaceContext(PipelineContext.New(GetOption(), null, null, PipelineHookActions.BindTargetName, PipelineHookActions.BindTargetType, PipelineHookActions.BindField, new OptionContextBuilder(GetOption(), null, null, null).Build(), null), null);
            var s = GetSource();
            var result = new ExpressionContext(s[0].File[0], Definitions.ResourceKind.Rule, targetObject);
            context.Init(s);
            context.Begin();
            context.PushScope(Runtime.RunspaceScope.Precondition);
            context.EnterTargetObject(new TargetObject(targetObject));
            return result;
        }

        private static string GetSourcePath(string fileName)
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        #endregion Helper methods
    }
}
