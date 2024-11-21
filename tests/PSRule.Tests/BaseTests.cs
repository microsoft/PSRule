// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Pipeline.Emitters;

namespace PSRule;

#nullable enable

/// <summary>
/// A base class for all tests.
/// </summary>
public abstract class BaseTests
{
    #region Helper methods

    protected virtual PSRuleOption GetOption()
    {
        return new PSRuleOption();
    }

    protected virtual PSRuleOption GetOption(string path)
    {
        return PSRuleOption.FromFile(path);
    }

    internal TestWriter GetTestWriter(PSRuleOption? option = default)
    {
        return new TestWriter(option ?? GetOption());
    }

    protected static string GetSourcePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
    }

    internal static InternalFileInfo GetFileInfo(string fileName)
    {
        var file = new FileInfo(GetSourcePath(fileName));
        return new InternalFileInfo(file.FullName, file.Extension);
    }
    protected static string ReadFileAsString(string fileName)
    {
        var file = GetFileInfo(fileName);
        using var stream = file.GetFileStream();
        using var reader = stream.AsTextReader();
        return reader.ReadToEnd();
    }

    protected static Source[] GetSource(string path)
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath(path));
        return builder.Build();
    }

    protected static PSObject GetObject(params (string name, object value)[] properties)
    {
        var result = new PSObject();
        for (var i = 0; properties != null && i < properties.Length; i++)
            result.Properties.Add(new PSNoteProperty(properties[i].name, properties[i].value));

        return result;
    }

    #endregion Helper methods
}

#nullable restore
