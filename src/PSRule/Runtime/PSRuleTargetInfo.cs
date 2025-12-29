// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Resources;

namespace PSRule.Runtime;

[DebuggerDisplay("Path = {Path}, File = {File}")]
internal sealed class PSRuleTargetInfo : PSMemberInfo
{
    internal const string PropertyName = "_PSRule";

    public PSRuleTargetInfo()
    {
        SetMemberName(PropertyName);
        Source ??= new List<TargetSourceInfo>();
        Issue ??= new List<TargetIssueInfo>();
    }

    private PSRuleTargetInfo(PSRuleTargetInfo targetInfo)
        : this()
    {
        if (targetInfo == null)
            return;

        TargetName = targetInfo.TargetName;
        TargetType = targetInfo.TargetType;
        Scope = targetInfo.Scope;
        Path = targetInfo.Path;
        Source = targetInfo.Source;
        Issue = targetInfo.Issue;
    }

    public string? File
    {
        get
        {
            return Source.Count == 0 ? null : Source[0].File;
        }
    }

    [JsonProperty(PropertyName = "name")]
    public string TargetName { get; set; }

    [JsonProperty(PropertyName = "type")]
    public string TargetType { get; set; }

    [JsonProperty(PropertyName = "scope")]
    public string[] Scope { get; set; }

    [JsonProperty(PropertyName = "path")]
    public string Path { get; set; }

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

        TargetName = targetInfo.TargetName;
        TargetType = targetInfo.TargetType;
        Scope = targetInfo.Scope;
        Path = targetInfo.Path;
        Source.AddUnique(targetInfo?.Source);
        Issue.AddUnique(targetInfo?.Issue);
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

    internal void SetSource(string file, int? lineNumber, int? linePosition)
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
