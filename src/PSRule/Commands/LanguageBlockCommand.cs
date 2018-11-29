using PSRule.Annotations;
using PSRule.Host;
using PSRule.Rules;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Commands
{
    public abstract class LanguageBlockCommand : PSCmdlet
    {
        protected BlockMetadata GetMetadata(string path, int lineNumber, int offset)
        {
            return HostHelper.GetCommentMeta(path, lineNumber, offset);
        }

        protected TagSet GetTag(Hashtable hashtable)
        {
            return TagSet.FromHashtable(hashtable);
        }
    }
}
