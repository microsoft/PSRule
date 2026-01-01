// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Emitters;
using PSRule.Pipeline;

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

    protected static Source[] GetSourceAsModule(string cachePath, string name, string version)
    {
        var builder = new SourcePipelineBuilder(null, null, GetSourcePath(cachePath));
        builder.ModuleByName(name, version);
        return builder.Build();
    }

    protected static SourceFile GetSourceFile(string path)
    {
        switch (Path.GetExtension(path))
        {
            case ".json":
            case ".jsonc":
                return new SourceFile(GetSourcePath(path), null, SourceType.Json, null);

            case ".yaml":
            case ".yml":
                return new SourceFile(GetSourcePath(path), null, SourceType.Yaml, null);

            default:
                return new SourceFile(GetSourcePath(path), null, SourceType.Script, null);
        }
    }

    protected static PSObject GetObject(params (string name, object value)[] properties)
    {
        var result = new PSObject();
        for (var i = 0; properties != null && i < properties.Length; i++)
            result.Properties.Add(new PSNoteProperty(properties[i].name, properties[i].value));

        return result;
    }

    protected static TargetObject GetTargetObject(params (string name, object value)[] properties)
    {
        return new TargetObject(GetObject(properties));
    }

    #endregion Helper methods
}

#nullable restore
