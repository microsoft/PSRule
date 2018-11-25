using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PSRule.Commands
{
    [Cmdlet(VerbsCommon.New, "RuleDefinition")]
    internal sealed class NewRuleDefinitionCommand : LanguageBlockCommand
    {
        /// <summary>
        /// The name of the deployment.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// The definition of the deployment.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        public ScriptBlock Body { get; set; }

        [Parameter(Mandatory = false)]
        public string[] Tag { get; set; }

        /// <summary>
        /// The environments that the deployment applies to.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] Environment { get; set; }

        /// <summary>
        /// Deployments that this deployment depends on.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string[] DependsOn { get; set; }

        protected override void ProcessRecord()
        {
            var metadata = GetMetadata(Body);

            foreach (var environmentName in GetEnvironmentNames())
            {
                WriteVerbose($"[PSRule][R][{Name}]::BEGIN");

                var block = new RuleBlock(environmentName, Name)
                {
                    Body = Body,
                    Description = metadata.Description,
                    DependsOn = DependsOn
                };

                WriteObject(block);

                WriteVerbose($"[PSRule][R][{Name}]::END");
            }
        }

        private string[] GetEnvironmentNames()
        {
            if (Environment == null || Environment.Length == 0)
            {
                return new string[] { "default" };
            }
            else
            {
                return Environment;
            }
        }
    }
}
