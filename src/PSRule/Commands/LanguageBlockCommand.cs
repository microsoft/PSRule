using PSRule.Annotations;
using PSRule.Host;
using System.Management.Automation;

namespace PSRule.Commands
{
    public abstract class LanguageBlockCommand : PSCmdlet
    {
        protected BlockMetadata GetMetadata(ScriptBlock body)
        {
            return HostHelper.GetCommentMeta(body.File, body.Ast.Parent.Parent.Extent.StartOffset);
        }
    }
}
