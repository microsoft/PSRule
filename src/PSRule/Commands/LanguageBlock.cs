// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Runtime;

namespace PSRule.Commands;

/// <summary>
/// A base class for language blocks.
/// </summary>
internal abstract class LanguageBlock : PSCmdlet
{
    private const string ErrorActionParameter = "ErrorAction";

    protected static CommentMetadata GetCommentMetadata(ISourceFile file, int lineNumber, int offset)
    {
        return HostHelper.GetCommentMeta(file, lineNumber - 2, offset);
    }

    protected static ResourceTags GetTag(Hashtable hashtable)
    {
        return ResourceTags.FromHashtable(hashtable);
    }

    protected static bool IsSourceScope()
    {
        return LegacyRunspaceContext.CurrentThread.IsScope(RunspaceScope.Source);
    }

    protected ActionPreference GetErrorActionPreference()
    {
        var preference = GetBoundPreference(ErrorActionParameter) ?? ActionPreference.Stop;
        // Ignore not supported on older PowerShell versions
        return preference == ActionPreference.Ignore ? ActionPreference.SilentlyContinue : preference;
    }

    protected ActionPreference? GetBoundPreference(string name)
    {
        return MyInvocation.BoundParameters.TryGetValue(name, out var o) &&
            Enum.TryParse(o.ToString(), out ActionPreference value) ? value : null;
    }
}
