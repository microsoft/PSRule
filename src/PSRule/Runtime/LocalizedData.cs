// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Dynamic;
using System.Management.Automation.Language;

namespace PSRule.Runtime;

/// <summary>
/// A PSRule built-in variable that is used to reference localized strings from rules.
/// </summary>
public sealed class LocalizedData : DynamicObject
{
    private const string DATA_FILENAME = "PSRule-rules.psd1";

    private static readonly Hashtable Empty = [];

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var hashtable = TryGetLocalized();
        if (hashtable.Count > 0 && binder != null && !string.IsNullOrEmpty(binder.Name) && hashtable.ContainsKey(binder.Name))
        {
            result = hashtable[binder.Name];
            return true;
        }
        result = null;
        return false;
    }

    private static Hashtable TryGetLocalized()
    {
        var path = GetFilePath();
        if (path == null)
            return Empty;

        if (LegacyRunspaceContext.CurrentThread.Pipeline.LocalizedDataCache.TryGetValue(path, out var value))
            return value;

        var ast = Parser.ParseFile(path, out var tokens, out var errors);
        var data = ast.Find(a => a is HashtableAst, false);
        if (data != null)
        {
            var result = (Hashtable)data.SafeGetValue();
            LegacyRunspaceContext.CurrentThread.Pipeline.LocalizedDataCache[path] = result;
            return result;
        }
        return Empty;
    }

    private static string GetFilePath()
    {
        return LegacyRunspaceContext.CurrentThread.GetLocalizedPath(DATA_FILENAME, out _);
    }
}
