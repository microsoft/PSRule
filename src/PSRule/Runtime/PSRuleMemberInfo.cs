// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Resources;

namespace PSRule.Runtime
{
    internal sealed class PSRuleTargetInfo : PSMemberInfo
    {
        internal const string PropertyName = "_PSRule";

        public PSRuleTargetInfo()
        {
            SetMemberName(PropertyName);
            if (Source == null)
                Source = new List<TargetSourceInfo>();

            if (Issue == null)
                Issue = new List<TargetIssueInfo>();
        }

        private PSRuleTargetInfo(PSRuleTargetInfo targetInfo)
            : this()
        {
            if (targetInfo == null)
                return;

            Source = targetInfo.Source;
            Issue = targetInfo.Issue;
        }

        public string Path
        {
            get
            {
                return Source.Count == 0 ? null : Source[0].File;
            }
        }

        [JsonProperty(PropertyName = "source")]
        public List<TargetSourceInfo> Source { get; internal set; }

        [JsonProperty(PropertyName = "issue")]
        public List<TargetIssueInfo> Issue { get; internal set; }

        [JsonIgnore]
        public override PSMemberTypes MemberType => PSMemberTypes.PropertySet;

        [JsonIgnore]
        public override string TypeNameOfValue => typeof(PSRuleTargetInfo).FullName;

        [JsonIgnore]
        public override object Value
        {
            get => this;
            set { }
        }

        public override PSMemberInfo Copy()
        {
            return new PSRuleTargetInfo(this);
        }

        internal void Combine(PSRuleTargetInfo targetInfo)
        {
            if (targetInfo == null)
                return;

            Source.AddUnique(targetInfo?.Source);
        }

        internal void WithSource(TargetSourceInfo source)
        {
            if (source == null)
                return;

            Source.Add(source);
        }

        internal void WithIssue(TargetIssueInfo issue)
        {
            if (issue == null)
                return;

            Issue.Add(issue);
        }

        internal void UpdateSource(TargetSourceInfo source)
        {
            if (source == null)
                return;

            if (Source.Count == 0)
            {
                Source.Add(source);
                return;
            }

            if (Source[0].File == null)
                Source[0].File = source.File;
        }

        internal void SetSource(string file, int lineNumber, int linePosition)
        {
            if (Source.Count > 0)
                return;

            var s = new TargetSourceInfo
            {
                File = file,
                Type = PSRuleResources.FileSourceType,
                Line = lineNumber,
                Position = linePosition
            };
            Source.Add(s);
        }
    }
}