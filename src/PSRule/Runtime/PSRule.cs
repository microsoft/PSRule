// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;
using PSRule.Pipeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PSRule.Runtime
{
    /// <summary>
    /// A set of context properties that are exposed at runtime through the $PSRule variable.
    /// </summary>
    public sealed class PSRule
    {
        private readonly RunspaceContext _Context;

        public PSRule() { }

        internal PSRule(RunspaceContext context)
        {
            _Context = context;
        }

        /// <summary>
        /// Custom data set by the rule for this target object.
        /// </summary>
        public Hashtable Data
        {
            get
            {
                return _Context.RuleRecord.GetData();
            }
        }

        /// <summary>
        /// A set of custom fields bound for the target object.
        /// </summary>
        public Hashtable Field
        {
            get
            {
                return _Context.RuleRecord.Field;
            }
        }

        /// <summary>
        /// The current target object.
        /// </summary>
        public PSObject TargetObject
        {
            get
            {
                return _Context.RuleRecord.TargetObject;
            }
        }

        /// <summary>
        /// The bound name of the target object.
        /// </summary>
        public string TargetName
        {
            get
            {
                return _Context.RuleRecord.TargetName;
            }
        }

        /// <summary>
        /// The bound type of the target object.
        /// </summary>
        public string TargetType
        {
            get
            {
                return _Context.RuleRecord.TargetType;
            }
        }

        /// <summary>
        /// Attempts to read content from disk.
        /// </summary>
        public PSObject[] GetContent(PSObject sourceObject)
        {
            if (sourceObject == null)
                return Array.Empty<PSObject>();

            if (!(sourceObject.BaseObject is InputFileInfo || sourceObject.BaseObject is FileInfo || sourceObject.BaseObject is Uri))
                return new PSObject[] { sourceObject };

            var cacheKey = sourceObject.BaseObject.ToString();
            if (_Context.Pipeline.ContentCache.TryGetValue(cacheKey, out PSObject[] result))
                return result;

            result = PipelineReceiverActions.DetectInputFormat(sourceObject, PipelineReceiverActions.PassThru).ToArray();
            _Context.Pipeline.ContentCache.Add(cacheKey, result);
            return result;
        }

        /// <summary>
        /// Attempts to read content from disk and extract a field from each object.
        /// </summary>
        public PSObject[] GetContentField(PSObject sourceObject, string field)
        {
            var content = GetContent(sourceObject);
            if (content == null || content.Length == 0 || string.IsNullOrEmpty(field))
                return Array.Empty<PSObject>();

            var result = new List<PSObject>();
            for (var i = 0; i < content.Length; i++)
            {
                if (ObjectHelper.GetField(content[i], field, false, out object value) && value != null)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
                    {
                        foreach (var item in (value as IEnumerable))
                            result.Add(PSObject.AsPSObject(item));
                    }
                    else
                        result.Add(PSObject.AsPSObject(value));
                }
                    
            }
            return result.ToArray();
        }

        /// <summary>
        /// Attempts to read content from disk and return the first object or null.
        /// </summary>
        public PSObject GetContentFirstOrDefault(PSObject sourceObject)
        {
            var content = GetContent(sourceObject);
            if (content == null || content.Length == 0)
                return null;

            return content[0];
        }
    }
}
