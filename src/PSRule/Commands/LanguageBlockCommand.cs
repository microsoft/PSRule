using PSRule.Annotations;
using PSRule.Host;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    public abstract class LanguageBlockCommand : PSCmdlet
    {
        protected BlockMetadata GetMetadata(ScriptBlock body)
        {
            return HostHelper.GetCommentMeta(body.File, body.Ast.Parent.Parent.Extent.StartOffset);
        }

        protected TagSet GetTag(Hashtable hashtable)
        {
            return TagSet.FromHashtable(hashtable);
        }
    }
}
