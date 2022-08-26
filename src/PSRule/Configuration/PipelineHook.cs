// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace PSRule.Configuration
{
    /// <summary>
    /// Used by custom binding functions.
    /// </summary>
    public delegate string BindTargetName(object targetObject);

    internal delegate string BindTargetMethod(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, out string path);
    internal delegate string BindTargetFunc(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, BindTargetMethod next, out string path);

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
