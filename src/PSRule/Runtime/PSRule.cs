// Copyright (c) Microsoft Corporation.
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
        /// <summary>
        /// Custom data set by the rule for this target object.
        /// </summary>
        public Hashtable Data
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.Data;
            }
        }

        /// <summary>
        /// A set of custom fields bound for the target object.
        /// </summary>
        public Hashtable Field
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.Field;
            }
        }

        /// <summary>
        /// The current target object.
        /// </summary>
        public PSObject TargetObject
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetObject;
            }
        }

        /// <summary>
        /// The bound name of the target object.
        /// </summary>
        public string TargetName
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetName;
            }
        }

        /// <summary>
        /// The bound type of the target object.
        /// </summary>
        public string TargetType
        {
            get
            {
                return PipelineContext.CurrentThread.RuleRecord.TargetType;
            }
        }
    }
}
