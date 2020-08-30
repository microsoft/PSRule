// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Annotations;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Pipeline;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// A base class for language blocks.
    /// </summary>
    internal abstract class LanguageBlock : PSCmdlet
    {
        protected static CommentMetadata GetMetadata(string path, int lineNumber, int offset)
        {
            return HostHelper.GetCommentMeta(path, lineNumber - 2, offset);
        }

        protected static TagSet GetTag(Hashtable hashtable)
        {
            return TagSet.FromHashtable(hashtable);
        }

        protected static bool IsScriptScope()
        {
            return PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Script;
        }
    }
}
