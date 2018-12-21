using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Configuration
{
    public delegate string BindTargetName(PSObject targetObject);

    public delegate string BindTargetNameAction(PSObject targetObject, BindTargetName next);

    public sealed class PipelineHook
    {
        public static BindTargetName EmptyBindTargetNameDelegate = (targetObject) => { return null; };

        public PipelineHook()
        {
            BindTargetName = new List<BindTargetName>();
        }

        public List<BindTargetName> BindTargetName { get; set; }
    }
}
