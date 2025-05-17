// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Globalization;
using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule;

internal static class PSObjectExtensions
{
    private const string PROPERTY_SOURCE = "source";
    private const string PROPERTY_ISSUE = "issue";
    private const string PROPERTY_NAME = "name";
    private const string PROPERTY_TYPE = "type";
    private const string PROPERTY_SCOPE = "scope";
    private const string PROPERTY_PATH = "path";
    private const string PROPERTY_FILE = "file";
    private const string PROPERTY_LINE = "line";
    private const string PROPERTY_POSITION = "position";
    private const string PROPERTY_MESSAGE = "message";

    public static T PropertyValue<T>(this PSObject o, string propertyName)
    {
        return o.BaseObject is Hashtable hashtable
            ? ConvertValue<T>(hashtable[propertyName])
            : ConvertValue<T>(o.Properties[propertyName].Value);
    }

    public static PSObject PropertyValue(this PSObject o, string propertyName)
    {
        return o.BaseObject is Hashtable hashtable
            ? PSObject.AsPSObject(hashtable[propertyName])
            : PSObject.AsPSObject(o.Properties[propertyName].Value);
    }

    public static bool HasProperty(this PSObject o, string propertyName)
    {
        return o.Properties[propertyName] != null;
    }

    /// <summary>
    /// Determines if the PSObject has any note properties.
    /// </summary>
    public static bool HasNoteProperty(this PSObject o)
    {
        foreach (var property in o.Properties)
        {
            if (property.MemberType == PSMemberTypes.NoteProperty)
                return true;
        }
        return false;
    }

    public static bool TryProperty<T>(this PSObject o, string name, out T? value)
    {
        value = default;
        var pValue = ConvertValue<T>(o.Properties[name]?.Value);
        if (pValue is T tValue)
        {
            value = tValue;
            return true;
        }
        return false;
    }

    public static string ToJson(this PSObject o)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.None,
            MaxDepth = 1024,
            Culture = CultureInfo.InvariantCulture
        };
        settings.Converters.Insert(0, new PSObjectJsonConverter());
        return JsonConvert.SerializeObject(o, settings);
    }

    public static bool TryTargetInfo(this PSObject o, out PSRuleTargetInfo targetInfo)
    {
        return TryMember(o, PSRuleTargetInfo.PropertyName, out targetInfo);
    }

    public static void UseTargetInfo(this PSObject o, out PSRuleTargetInfo targetInfo)
    {
        if (TryTargetInfo(o, out targetInfo))
            return;

        o.Members.Add(new PSRuleTargetInfo());
        TryTargetInfo(o, out targetInfo);
    }

    public static void SetTargetInfo(this PSObject o, PSRuleTargetInfo targetInfo)
    {
        if (TryTargetInfo(o, out var originalInfo))
        {
            targetInfo.Combine(originalInfo);
            o.Members[PSRuleTargetInfo.PropertyName].Value = targetInfo;
            return;
        }
        o.Members.Add(targetInfo);
    }

    public static TargetSourceInfo[] GetSourceInfo(this PSObject o)
    {
        return o.TryTargetInfo(out var targetInfo) ? [.. targetInfo.Source] : [];
    }

    public static TargetIssueInfo[] GetIssueInfo(this PSObject o)
    {
        return o.TryTargetInfo(out var targetInfo) ? [.. targetInfo.Issue] : [];
    }

    public static string? GetTargetName(this PSObject o)
    {
        return o != null && o.TryTargetInfo(out var targetInfo) ? targetInfo.TargetName : null;
    }

    public static string? GetTargetType(this PSObject o)
    {
        return o != null && o.TryTargetInfo(out var targetInfo) ? targetInfo.TargetType : null;
    }

    public static string[]? GetScope(this PSObject o)
    {
        return o != null && o.TryTargetInfo(out var targetInfo) ? targetInfo.Scope : null;
    }

    public static string GetTargetPath(this PSObject o)
    {
        if (o == null)
            return string.Empty;

        if (o.TryTargetInfo(out var targetInfo))
            return targetInfo.Path;

        var baseObject = o.BaseObject;
        return baseObject is JToken token ? token.Path : string.Empty;
    }

    public static void ConvertTargetInfoProperty(this PSObject o)
    {
        if (o == null || !TryProperty(o, PSRuleTargetInfo.PropertyName, out PSObject value))
            return;

        UseTargetInfo(o, out var targetInfo);
        if (TryProperty(value, PROPERTY_NAME, out string name) && targetInfo.TargetName == null)
            targetInfo.TargetName = name;

        if (TryProperty(value, PROPERTY_TYPE, out string type) && targetInfo.TargetType == null)
            targetInfo.TargetType = type;

        if (TryProperty(value, PROPERTY_SCOPE, out string[] scope) && targetInfo.Scope == null)
            targetInfo.Scope = scope;

        if (TryProperty(value, PROPERTY_PATH, out string path) && targetInfo.Path == null)
            targetInfo.Path = path;

        if (TryProperty(value, PROPERTY_SOURCE, out Array sources))
        {
            for (var i = 0; i < sources.Length; i++)
            {
                var source = CreateSourceInfo(sources.GetValue(i));
                targetInfo.WithSource(source);
            }
        }
        if (TryProperty(value, PROPERTY_ISSUE, out Array issues))
        {
            for (var i = 0; i < issues.Length; i++)
            {
                var issue = CreateIssueInfo(issues.GetValue(i));
                targetInfo.WithIssue(issue);
            }
        }
    }

    public static void ConvertTargetInfoType(this PSObject o)
    {
        var info = o?.BaseObject;
        if (info is FileInfo fileInfo)
        {
            UseTargetInfo(o, out var targetInfo);
            targetInfo.WithSource(new TargetSourceInfo(fileInfo));
        }
        if (info is InputFileInfo inputFileInfo)
        {
            UseTargetInfo(o, out var targetInfo);
            targetInfo.WithSource(new TargetSourceInfo(inputFileInfo));
        }
    }

    private static TargetIssueInfo? CreateIssueInfo(object o)
    {
        return o is PSObject pso ? CreateIssueInfo(pso) : null;
    }

    private static TargetIssueInfo CreateIssueInfo(PSObject o)
    {
        var result = new TargetIssueInfo();
        if (o.TryProperty(PROPERTY_TYPE, out string type))
            result.Type = type;

        if (o.TryProperty(PROPERTY_NAME, out string name))
            result.Name = name;

        if (o.TryProperty(PROPERTY_PATH, out string path))
            result.Path = path;

        if (o.TryProperty(PROPERTY_MESSAGE, out string message))
            result.Message = message;

        return result;
    }

    private static TargetSourceInfo? CreateSourceInfo(object o)
    {
        return o is PSObject pso ? CreateSourceInfo(pso) : null;
    }

    private static TargetSourceInfo CreateSourceInfo(PSObject o)
    {
        var result = new TargetSourceInfo();
        if (o.TryProperty(PROPERTY_FILE, out string file))
            result.File = file;

        if (o.TryProperty(PROPERTY_LINE, out int line))
            result.Line = line;

        if (o.TryProperty(PROPERTY_POSITION, out int position))
            result.Position = position;

        if (o.TryProperty(PROPERTY_TYPE, out string type))
            result.Type = type;

        return result;
    }

    private static T? ConvertValue<T>(object value)
    {
        if (value == null)
            return default;

        return typeof(T).IsValueType ? (T)Convert.ChangeType(value, typeof(T), Thread.CurrentThread.CurrentCulture) : (T)value;
    }

    private static bool TryMember<T>(PSObject o, string name, out T value)
    {
        value = default;
        if (o.Members[name]?.Value is T tValue)
        {
            value = tValue;
            return true;
        }
        return false;
    }
}
