// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Pipeline.Runs;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// Define tests for expression functions are working correctly.
/// </summary>
public sealed class FunctionTests : ContextBaseTests
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

    [Fact]
    public void Replace()
    {
        var context = GetContext();
        var fn = GetFunction("replace");

        var properties = new LanguageExpression.PropertyBag
        {
            { "oldString", "12" },
            { "newString", "" },
            { "replace", "Test123" }
        };
        Assert.Equal("Test3", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "oldString", "456" },
            { "newString", "" },
            { "replace", "Test123" }
        };
        Assert.Equal("Test123", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "oldString", "456" },
            { "newString", "" },
            { "replace", "" }
        };
        Assert.Equal("", fn(context, properties)(context));
    }

    [Fact]
    public void Trim()
    {
        var context = GetContext();
        var fn = GetFunction("trim");

        var properties = new LanguageExpression.PropertyBag
        {
            { "trim", " test " }
        };
        Assert.Equal("test", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "trim", "test" }
        };
        Assert.Equal("test", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "trim", "\r\ntest\r\n" }
        };
        Assert.Equal("test", fn(context, properties)(context));
    }

    [Fact]
    public void First()
    {
        var context = GetContext();
        var fn = GetFunction("first");

        // String
        var properties = new LanguageExpression.PropertyBag
        {
            { "first", "Test123" }
        };
        Assert.Equal("T", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "first", "" }
        };
        Assert.Null(fn(context, properties)(context));

        // Array
        properties = new LanguageExpression.PropertyBag
        {
            { "first", new string[] { "one", "two", "three" } }
        };
        Assert.Equal("one", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "first", new int[] { 1, 2, 3 } }
        };
        Assert.Equal(1, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "first", Array.Empty<object>() }
        };
        Assert.Null(fn(context, properties)(context));
    }

    [Fact]
    public void Last()
    {
        var context = GetContext();
        var fn = GetFunction("last");

        // String
        var properties = new LanguageExpression.PropertyBag
        {
            { "last", "Test123" }
        };
        Assert.Equal("3", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "last", "" }
        };
        Assert.Null(fn(context, properties)(context));

        // Array
        properties = new LanguageExpression.PropertyBag
        {
            { "last", new string[] { "one", "two", "three" } }
        };
        Assert.Equal("three", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "last", new int[] { 1, 2, 3 } }
        };
        Assert.Equal(3, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "last", Array.Empty<object>() }
        };
        Assert.Null(fn(context, properties)(context));
    }

    [Fact]
    public void Split()
    {
        var context = GetContext();
        var fn = GetFunction("split");

        var properties = new LanguageExpression.PropertyBag
        {
            { "split", "One Two Three" },
            { "delimiter", " " }
        };
        Assert.Equal(new string[] { "One", "Two", "Three" }, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "split", "One Two Three" },
            { "delimiter", " Two " }
        };
        Assert.Equal(new string[] { "One", "Three" }, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "split", "One Two Three" },
            { "delimiter", "/" }
        };
        Assert.Equal(new string[] { "One Two Three" }, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "split", "" },
            { "delimiter", "/" }
        };
        Assert.Equal(new string[] { "" }, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "split", "One Two Three" },
            { "delimiter", new string[] { " Two " } }
        };
        Assert.Equal(new string[] { "One", "Three" }, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "split", "One Two Three" },
            { "delimiter", new string[] { " ", "Two" } }
        };
        Assert.Equal(new string[] { "One", "", "", "Three" }, fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "split", null },
            { "delimiter", new string[] { " ", "Two" } }
        };
        Assert.Null(fn(context, properties)(context));
    }

    [Fact]
    public void PadLeft()
    {
        var context = GetContext();
        var fn = GetFunction("padLeft");

        var properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 5 }
        };
        Assert.Equal("  One", fn(context, properties)(context));


        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 3 }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 5 },
            { "paddingCharacter", '_' }
        };
        Assert.Equal("__One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 5 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("__One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 5 },
            { "paddingCharacter", "__" }
        };
        Assert.Equal("  One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 3 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", 1 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", -1 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", null },
            { "totalLength", 5 }
        };
        Assert.Null(fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padLeft", "One" },
            { "totalLength", null }
        };
        Assert.Equal("One", fn(context, properties)(context));
    }

    [Fact]
    public void PadRight()
    {
        var context = GetContext();
        var fn = GetFunction("padRight");

        var properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 5 }
        };
        Assert.Equal("One  ", fn(context, properties)(context));


        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 3 }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 5 },
            { "paddingCharacter", '_' }
        };
        Assert.Equal("One__", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 5 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One__", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 5 },
            { "paddingCharacter", "__" }
        };
        Assert.Equal("One  ", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 3 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", 1 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", -1 },
            { "paddingCharacter", "_" }
        };
        Assert.Equal("One", fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", null },
            { "totalLength", 5 }
        };
        Assert.Null(fn(context, properties)(context));

        properties = new LanguageExpression.PropertyBag
        {
            { "padRight", "One" },
            { "totalLength", null }
        };
        Assert.Equal("One", fn(context, properties)(context));
    }

    #region Helper methods

    private static ExpressionBuilderFn GetFunction(string name)
    {
        return Functions.Builtin.Single(f => f.Name == name).Fn;
    }

    protected sealed override PSRuleOption GetOption()
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

    private IRun GetRun(PSRuleOption? option)
    {
        var o = option ?? GetOption();
        var runConfig = new RunConfiguration(o.Configuration.ToDictionary());
        return new Run(NullLogger.Instance, ".", "run-001", new InfoString("Test run", null), Guid.Empty.ToString(), new EmptyRuleGraph(), runConfig);
    }

    private ExpressionContext GetContext()
    {
        var option = GetOption();
        var targetObject = new PSObject();
        targetObject.Properties.Add(new PSNoteProperty("name", "TestObject1"));
        var sources = GetSource();
        var context = new Runtime.LegacyRunspaceContext(GetPipelineContext(option: GetOption(), sources: sources, optionBuilder: new OptionContextBuilder(GetOption(), null, null, null), resourceCache: GetResourceCache()));
        var result = new ExpressionContext(context, sources[0].File[0], Definitions.ResourceKind.Rule, new TargetObject(targetObject));
        context.Initialize(sources);
        context.Begin();
        context.PushScope(Runtime.RunspaceScope.Precondition);
        context.EnterRun(GetRun(option));
        context.EnterLanguageScope(sources[0].File[0]);
        context.EnterTargetObject(new TargetObject(targetObject));
        return result;
    }

    #endregion Helper methods
}
