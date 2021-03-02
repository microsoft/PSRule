// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// A base class for language blocks.
    /// </summary>
    internal abstract class LanguageBlock : PSCmdlet
    {
        private const string ErrorActionParameter = "ErrorAction";

        protected static CommentMetadata GetCommentMetadata(string path, int lineNumber, int offset)
        {
            return HostHelper.GetCommentMeta(path, lineNumber - 2, offset);
        }

        protected static TagSet GetTag(Hashtable hashtable)
        {
            return TagSet.FromHashtable(hashtable);
        }

        protected static bool IsSourceScope()
        {
            return RunspaceContext.CurrentThread.IsScope(RunspaceScope.Source);
        }

        protected ActionPreference GetErrorActionPreference()
        {
            var preference = GetBoundPreference(ErrorActionParameter) ?? ActionPreference.Stop;
            // Ignore not supported on older PowerShell versions
            return preference == ActionPreference.Ignore ? ActionPreference.SilentlyContinue : preference;
        }

        protected ActionPreference? GetBoundPreference(string name)
        {
            if (MyInvocation.BoundParameters.ContainsKey(name) && Enum.TryParse(MyInvocation.BoundParameters[name].ToString(), out ActionPreference value))
                return value;

            return null;
        }
    }
}
