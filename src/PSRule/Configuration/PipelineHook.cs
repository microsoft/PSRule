// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Configuration
{
    /// <summary>
    /// Used by custom binding functions.
    /// </summary>
    public delegate string BindTargetName(PSObject targetObject);

    internal delegate string BindTargetMethod(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, PSObject targetObject);
    internal delegate string BindTargetFunc(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, PSObject targetObject, BindTargetMethod next);

    /// <summary>
    /// Hooks that provide customize pipeline execution.
    /// </summary>
    public sealed class PipelineHook
    {
        public PipelineHook()
        {
            BindTargetName = new List<BindTargetName>();
            BindTargetType = new List<BindTargetName>();
        }

        public PipelineHook(PipelineHook option)
        {
            BindTargetName = option?.BindTargetName ?? new List<BindTargetName>();
            BindTargetType = option?.BindTargetType ?? new List<BindTargetName>();
        }

        /// <summary>
        /// One or more custom functions to use to bind TargetName of a pipeline object.
        /// </summary>
        public List<BindTargetName> BindTargetName { get; set; }

        /// <summary>
        /// One or more custom functions to use to bind TargetType of a pipeline object.
        /// </summary>
        public List<BindTargetName> BindTargetType { get; set; }
    }
}
