// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using System;
using System.Management.Automation;

namespace PSRule.Runtime
{
#pragma warning disable CA1822 // Mark members as static

    /// <summary>
    /// A set of rule properties that are exposed at runtime through the $Rule variable.
    /// </summary>
    public sealed class Rule
    {
        private const string VariableName = "Rule";

        public string RuleName
        {
            get
            {
                return RunspaceContext.CurrentThread.RuleRecord.RuleName;
            }
        }

        public string RuleId
        {
            get
            {
                return RunspaceContext.CurrentThread.RuleRecord.RuleId;
            }
        }

        [Obsolete("Use property on $PSRule instead")]
        public PSObject TargetObject
        {
            get
            {
                RunspaceContext.CurrentThread.WarnPropertyObsolete(VariableName, nameof(TargetObject));
                return RunspaceContext.CurrentThread.RuleRecord.TargetObject;
            }
        }

        [Obsolete("Use property on $PSRule instead")]
        public string TargetName
        {
            get
            {
                RunspaceContext.CurrentThread.WarnPropertyObsolete(VariableName, nameof(TargetName));
                return RunspaceContext.CurrentThread.RuleRecord.TargetName;
            }
        }

        [Obsolete("Use property on $PSRule instead")]
        public string TargetType
        {
            get
            {
                RunspaceContext.CurrentThread.WarnPropertyObsolete(VariableName, nameof(TargetType));
                return RunspaceContext.CurrentThread.RuleRecord.TargetType;
            }
        }
    }

#pragma warning restore CA1822 // Mark members as static
}
