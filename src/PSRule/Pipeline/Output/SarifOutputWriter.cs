// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Sarif;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions.Rules;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Output
{
    internal sealed class SarifBuilder
    {
        private const string TOOL_NAME = "PSRule";
        private const string TOOL_ORG = "Microsoft Corporation";
        private const string RECOMMENDATION_MESSAGE_ID = "recommendation";
        private const string LOCATION_KIND_OBJECT = "object";
        private const string LOCATION_ID_REPOROOT = "REPOROOT";

        private readonly Run _Run;
        private readonly Dictionary<string, ReportingDescriptor> _Rules;
        private readonly Dictionary<string, ToolComponent> _Extensions;

        public SarifBuilder(Source[] source)
        {
            _Rules = new Dictionary<string, ReportingDescriptor>();
            _Extensions = new Dictionary<string, ToolComponent>();
            _Run = new Run
            {
                Tool = GetTool(source),
                Results = new List<Result>(),
                Invocations = GetInvocation(),
                AutomationDetails = GetAutomationDetails(),
                OriginalUriBaseIds = GetBaseIds(),
            };
        }

        private IDictionary<string, ArtifactLocation> GetBaseIds()
        {
            var result = new Dictionary<string, ArtifactLocation>(1);
            result.Add(LOCATION_ID_REPOROOT, new ArtifactLocation
            {
                Description = GetMessage(ReportStrings.SARIF_REPOROOT_Description),
            });
            return result;
        }

        public SarifLog Build()
        {
            var log = new SarifLog
            {
                Runs = new List<Run>(1),
            };
            log.Runs.Add(_Run);
            return log;
        }

        public void Add(RuleRecord record)
        {
            if (record == null)
                return;

            var rule = GetRule(record);
            var result = new Result
            {
                RuleId = rule.Id,
                Kind = GetKind(record),
                Level = GetLevel(record),
                Message = new Message { Id = RECOMMENDATION_MESSAGE_ID },
                Locations = GetLocations(record),
            };
            _Run.Results.Add(result);
        }

        private ReportingDescriptor GetRule(RuleRecord record)
        {
            var id = record.Ref ?? record.RuleId;
            if (!_Rules.TryGetValue(id, out var descriptor))
            {
                descriptor = AddRule(record, id);
            }
            return descriptor;
        }

        private ReportingDescriptor AddRule(RuleRecord record, string id)
        {
            var descriptor = new ReportingDescriptor
            {
                Id = id,
                Name = record.RuleName,
                ShortDescription = GetMessageString(record.Info.Synopsis),
                HelpUri = record.Info.GetOnlineHelpUri(),
                FullDescription = GetMessageString(record.Info.Description),
                MessageStrings = GetMessageStrings(record)
            };
            _Rules.Add(id, descriptor);

            // Add to module extensions or PSRule
            if (string.IsNullOrEmpty(record.Info.ModuleName) || !_Extensions.TryGetValue(record.Info.ModuleName, out var toolComponent))
                toolComponent = _Run.Tool.Driver;

            toolComponent.Rules.Add(descriptor);
            return descriptor;
        }

        private static RunAutomationDetails GetAutomationDetails()
        {
            return PipelineContext.CurrentThread == null ? null : new RunAutomationDetails
            {
                Id = PipelineContext.CurrentThread.RunId,
            };
        }

        private static IList<Invocation> GetInvocation()
        {
            var result = new List<Invocation>(1);
            var invocation = new Invocation
            {

            };
            result.Add(invocation);
            return result;
        }

        private static Message GetMessage(string text)
        {
            return new Message
            {
                Text = text
            };
        }

        private static MultiformatMessageString GetMessageString(string text)
        {
            return new MultiformatMessageString
            {
                Text = text
            };
        }

        private static IDictionary<string, MultiformatMessageString> GetMessageStrings(RuleRecord record)
        {
            var strings = new Dictionary<string, MultiformatMessageString>(1);
            strings.Add(RECOMMENDATION_MESSAGE_ID, new MultiformatMessageString
            {
                Text = record.Recommendation
            });
            return strings;
        }

        private static IList<Location> GetLocations(RuleRecord record)
        {
            if (!record.HasSource())
                return null;

            var result = new List<Location>(record.Source.Length);
            for (var i = 0; i < record.Source.Length; i++)
            {
                result.Add(new Location
                {
                    PhysicalLocation = GetPhysicalLocation(record.Source[i]),
                    LogicalLocation = new LogicalLocation
                    {
                        Name = record.TargetName,
                        FullyQualifiedName = string.Concat(record.TargetType, "/", record.TargetName),
                        Kind = LOCATION_KIND_OBJECT,
                    }
                });
            }
            return result;
        }

        private static PhysicalLocation GetPhysicalLocation(TargetSourceInfo info)
        {
            var location = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = new Uri(info.GetPath(useRelativePath: true), UriKind.Relative),
                    UriBaseId = LOCATION_ID_REPOROOT,
                }
            };
            var region = new Region();
            if (info.Line.HasValue)
            {
                region.StartLine = info.Line.Value;
                location.Region = region;
            }
            if (info.Position.HasValue)
            {
                region.StartColumn = info.Position.Value;
                location.Region = region;
            }
            return location;
        }

        private static ResultKind GetKind(RuleRecord record)
        {
            if (record.Outcome == RuleOutcome.Pass)
                return ResultKind.Pass;

            if (record.Outcome == RuleOutcome.Fail)
                return ResultKind.Fail;

            return record.Outcome == RuleOutcome.Error ||
                record.Outcome == RuleOutcome.None && record.OutcomeReason == RuleOutcomeReason.Inconclusive ?
                ResultKind.Open :
                ResultKind.None;
        }

        private static FailureLevel GetLevel(RuleRecord record)
        {
            if (record.Outcome != RuleOutcome.Fail)
                return FailureLevel.None;

            if (record.Level == SeverityLevel.Error)
                return FailureLevel.Error;

            return record.Level == SeverityLevel.Warning ? FailureLevel.Warning : FailureLevel.Note;
        }

        private Tool GetTool(Source[] source)
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            return new Tool
            {
                Driver = new ToolComponent
                {
                    Name = TOOL_NAME,
                    SemanticVersion = version,
                    Organization = TOOL_ORG,
                    Rules = new List<ReportingDescriptor>(),
                    InformationUri = new Uri("https://aka.ms/ps-rule", UriKind.Absolute),
                },
                Extensions = GetExtensions(source),
            };
        }

        private IList<ToolComponent> GetExtensions(Source[] source)
        {
            if (source == null || source.Length == 0)
                return null;

            var result = new List<ToolComponent>();
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i].Module != null && !_Extensions.ContainsKey(source[i].Module.Name))
                {
                    var extension = new ToolComponent
                    {
                        Name = source[i].Module.Name,
                        Version = source[i].Module.Version,
                        Rules = new List<ReportingDescriptor>(),
                    };
                    _Extensions.Add(extension.Name, extension);
                    result.Add(extension);
                }
            }
            return result.Count > 0 ? result : null;
        }
    }

    internal sealed class SarifOutputWriter : SerializationOutputWriter<InvokeResult>
    {
        private readonly SarifBuilder _Builder;
        private readonly Encoding _Encoding;

        internal SarifOutputWriter(Source[] source, PipelineWriter inner, PSRuleOption option)
            : base(inner, option)
        {
            _Builder = new SarifBuilder(source);
            _Encoding = option.Output.GetEncoding();
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!(sendToPipeline is InvokeResult result))
                return;

            Add(result);
        }

        protected override string Serialize(InvokeResult[] o)
        {
            for (var i = 0; o != null && i < o.Length; i++)
            {
                var records = o[i].AsRecord();
                for (var j = 0; j < records.Length; j++)
                    _Builder.Add(records[j]);
            }
            var log = _Builder.Build();
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, _Encoding, bufferSize: 1024, leaveOpen: true);
            log.Save(writer);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
