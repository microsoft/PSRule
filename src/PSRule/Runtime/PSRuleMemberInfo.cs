// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Data;
using System.Collections.Generic;
using System.Management.Automation;

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
        }

        private PSRuleTargetInfo(PSRuleTargetInfo targetInfo)
            : this()
        {
            if (targetInfo == null)
                return;

            Source = targetInfo.Source;
        }

        public string Path
        {
            get
            {
                if (Source.Count == 0)
                    return null;

                return Source[0].File;
            }
        }

        [JsonProperty(PropertyName = "source")]
        public List<TargetSourceInfo> Source { get; internal set; }

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

        internal void SetSource(int lineNumber, int linePosition)
        {
            if (Source.Count > 0)
                return;

            var s = new TargetSourceInfo
            {
                Line = lineNumber,
                Position = linePosition
            };
            Source.Add(s);
        }
    }
}
