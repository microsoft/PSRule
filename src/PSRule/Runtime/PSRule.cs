// Copyright(c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System.Collections;
using System.Management.Automation;

namespace PSRule.Runtime
{
    /// <summary>
    /// A set of context properties that are exposed at runtime through the $PSRule variable.
    /// </summary>
    public sealed class PSRule
    {
        public Hashtable Field
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.Field;
            }
        }

        public PSObject TargetObject
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetObject;
            }
        }

        public string TargetName
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetName;
            }
        }

        public string TargetType
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetType;
            }
        }
    }
}
