using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Configuration
{
    public delegate string BindTargetName(PSObject targetObject);

    public delegate string BindTargetNameAction(PSObject targetObject, BindTargetName next);

    /// <summary>
    /// Hooks that provide customize pipeline execution.
    /// </summary>
    public sealed class PipelineHook
    {
        public PipelineHook()
        {
            BindTargetName = new List<BindTargetName>();
        }

        /// <summary>
        /// One or more custom functions to use to bind TargetName of a pipeline object.
        /// </summary>
        public List<BindTargetName> BindTargetName { get; set; }
    }
}
