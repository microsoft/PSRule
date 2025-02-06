// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Options;

/// <summary>
/// Unit tests for <see cref="FormatOption"/>.
/// </summary>
public sealed class FormatOptionTests
{
    [Fact]
    public void Import_WithDictionary_ShouldImportAllProperties()
    {
        var formatOption = new FormatOption();

        formatOption.Import(new Dictionary<string, object>
        {
            { "Format.json.Type", new string[] { ".json", ".jsonc"} },
            { "Format.json.Enabled", true },
            { "Format.json.Replace", new Hashtable
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            }},
            { "Format.Yaml.Type", ".yaml" },
            { "Format.Yaml.Enabled", false },
            { "Format.Yaml.Replace", new Dictionary<string, string>
            {
                { "Key3", "Value3" },
                { "Key4", "Value4" }
            }}
        });

        var formatType = formatOption["json"];
        Assert.NotNull(formatType);
        Assert.Equal(new string[] { ".json", ".jsonc" }, formatType.Type);
        Assert.True(formatType.Enabled);
        Assert.NotNull(formatType.Replace);
        Assert.Equal("Value1", formatType.Replace["Key1"]);
        Assert.Equal("Value2", formatType.Replace["Key2"]);

        formatType = formatOption["yaml"];
        Assert.NotNull(formatType);
        Assert.Equal(new string[] { ".yaml" }, formatType.Type);
        Assert.False(formatType.Enabled);
        Assert.NotNull(formatType.Replace);
        Assert.Equal("Value3", formatType.Replace["Key3"]);
        Assert.Equal("Value4", formatType.Replace["Key4"]);
    }
}
