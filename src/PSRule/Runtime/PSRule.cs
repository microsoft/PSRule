﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Badges;
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
    public sealed class PSRule : ScopedItem
    {
        private PSRuleSource _Source;
        private IBadgeBuilder _BadgeBuilder;

        public PSRule() { }

        internal PSRule(RunspaceContext context)
            : base(context) { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Exposed as helper for PowerShell.")]
        private sealed class PSRuleSource : ScopedItem, ITargetSourceCollection
        {
            internal PSRuleSource(RunspaceContext context)
                : base(context) { }

            public TargetSourceInfo this[string type]
            {
                get
                {
                    return GetContext().TargetObject?.Source[type];
                }
            }
        }

        /// <summary>
        /// Exposes the badge API for used within conventions.
        /// </summary>
        public IBadgeBuilder Badges
        {
            get
            {
                RequireScope(RunspaceScope.ConventionEnd);
                if (_BadgeBuilder == null)
                    _BadgeBuilder = new BadgeBuilder();

                return _BadgeBuilder;
            }
        }

        /// <summary>
        /// Custom data set by the rule for this target object.
        /// </summary>
        public Hashtable Data
        {
            get
            {
                RequireScope(RunspaceScope.Rule | RunspaceScope.Precondition | RunspaceScope.ConventionBegin | RunspaceScope.ConventionProcess);
                return GetContext().Data;
            }
        }

        /// <summary>
        /// A set of custom fields bound for the target object.
        /// </summary>
        public Hashtable Field
        {
            get
            {
                RequireScope(RunspaceScope.Rule | RunspaceScope.Precondition);
                return GetContext().RuleRecord.Field;
            }
        }

        public IEnumerable<InvokeResult> Output
        {
            get
            {
                RequireScope(RunspaceScope.ConventionEnd);
                return GetContext().Output;
            }
        }

        public ITargetSourceCollection Source => GetSource();

        /// <summary>
        /// The current target object.
        /// </summary>
        public PSObject TargetObject => GetContext().RuleRecord.TargetObject;

        /// <summary>
        /// The bound name of the target object.
        /// </summary>
        public string TargetName => GetContext().RuleRecord.TargetName;

        /// <summary>
        /// The bound type of the target object.
        /// </summary>
        public string TargetType => GetContext().RuleRecord.TargetType;

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
            if (GetContext().Pipeline.ContentCache.TryGetValue(cacheKey, out PSObject[] result))
                return result;

            var items = PipelineReceiverActions.DetectInputFormat(new TargetObject(sourceObject), PipelineReceiverActions.PassThru).ToArray();
            result = new PSObject[items.Length];
            for (var i = 0; i < items.Length; i++)
                result[i] = items[i].Value;

            GetContext().Pipeline.ContentCache.Add(cacheKey, result);
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
                    if (value is IEnumerable evalue)
                    {
                        foreach (var item in evalue)
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

        /// <summary>
        /// Imports source objects into the pipeline for processing.
        /// </summary>
        public void Import(PSObject[] sourceObject)
        {
            if (sourceObject == null || sourceObject.Length == 0)
                return;

            RequireScope(RunspaceScope.ConventionBegin);
            for (var i = 0; i < sourceObject.Length; i++)
            {
                if (sourceObject[i] == null)
                    continue;

                GetContext().Pipeline.Reader.Enqueue(sourceObject[i], skipExpansion: true);
            }
        }

        #region Helper methods

        private ITargetSourceCollection GetSource()
        {
            RequireScope(RunspaceScope.Runtime);
            if (_Source == null)
                _Source = new PSRuleSource(GetContext());

            return _Source;
        }

        #endregion Helper methods
    }
}
