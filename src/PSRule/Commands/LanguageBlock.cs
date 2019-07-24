using PSRule.Annotations;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    /// <summary>
    /// A base class for language blocks.
    /// </summary>
    internal abstract class LanguageBlock : PSCmdlet
    {
        protected BlockMetadata GetMetadata(string path, int lineNumber, int offset)
        {
            return HostHelper.GetCommentMeta(path, lineNumber, offset);
        }

        protected TagSet GetTag(Hashtable hashtable)
        {
            return TagSet.FromHashtable(hashtable);
        }

        protected bool IsScriptScope()
        {
            return PipelineContext.CurrentThread.ExecutionScope == ExecutionScope.Script;
        }
    }
}
