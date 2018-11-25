using PSRule.Host;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PSRule.Rules
{
    /// <summary>
    /// Define an instance of a deployment block. Each deployment block has a unique name.
    /// </summary>
    public sealed class RuleBlock : ILanguageBlock
    {
        public RuleBlock(string environment, string name)
        {
            Name = name;
        }

        public string SourcePath { get; set; }

        /// <summary>
        /// The name of the deployment.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A human readable block of text, used to identify the purpose of the deployment.
        /// </summary>
        public string Description { get; set; }

        public ScriptBlock Body { get; set; }

        /// <summary>
        /// Other deployments that must completed successfully before calling this deployment.
        /// </summary>
        public string[] DependsOn { get; set; }
    }
}
