// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline.Output;

#nullable enable

/// <summary>
/// A helper to build a SARIF log.
/// </summary>
internal sealed class SarifBuilder
{
    private const string TOOL_NAME = "PSRule";
    private const string TOOL_ORG = "Microsoft Corporation";
    private const string TOOL_GUID = "0130215d-58eb-4887-b6fa-31ed02500569";
    private const string RECOMMENDATION_MESSAGE_ID = "recommendation";
    private const string LOCATION_KIND_OBJECT = "object";
    private const string LOCATION_ID_REPOROOT = "REPO_ROOT";

    private readonly Run _Run;
    private readonly System.Security.Cryptography.HashAlgorithm _ConfiguredHashAlgorithm;
    private readonly string _ConfiguredHashAlgorithmName;
    private readonly System.Security.Cryptography.HashAlgorithm? _SHA265;
    private readonly PSRuleOption _Option;
    private readonly Dictionary<string, ReportingDescriptorReference> _Rules;
    private readonly Dictionary<string, ToolComponent> _Extensions;
    private readonly Dictionary<string, Artifact> _Artifacts;

    public SarifBuilder(Source[] source, PSRuleOption option)
    {
        _Option = option;
        _Rules = [];
        _Extensions = [];
        _Artifacts = [];
        _Run = new Run
        {
            Tool = GetTool(source),
            Results = [],
            Invocations = GetInvocation(),
            AutomationDetails = GetAutomationDetails(),
            OriginalUriBaseIds = GetBaseIds(),
            VersionControlProvenance = GetVersionControl(option.Repository),
        };
        var algorithm = option.Execution.HashAlgorithm.GetValueOrDefault(ExecutionOption.Default.HashAlgorithm!.Value);
        _ConfiguredHashAlgorithm = algorithm.GetHashAlgorithm();
        _ConfiguredHashAlgorithmName = algorithm.GetHashAlgorithmName();

        // Always include SHA-256 to allow comparison with other tools and formats such as SPDX.
        _SHA265 = algorithm != HashAlgorithm.SHA256 ? HashAlgorithm.SHA256.GetHashAlgorithm() : null;
    }

    /// <summary>
    /// Get information from version control system.
    /// </summary>
    /// <remarks>
    /// https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317497
    /// </remarks>
    private static List<VersionControlDetails> GetVersionControl(RepositoryOption option)
    {
        var repository = option.Url;
        return new List<VersionControlDetails>()
        {
            new() {
                RepositoryUri = !string.IsNullOrEmpty(repository) ? new Uri(repository) : null,
                RevisionId = !string.IsNullOrEmpty(repository) && GitHelper.TryRevision(out var revision) ? revision : null,
                Branch = !string.IsNullOrEmpty(repository) && GitHelper.TryHeadBranch(out var branch) ? branch : null,
                MappedTo = new ArtifactLocation
                {
                    UriBaseId = LOCATION_ID_REPOROOT
                },
            }
        };
    }

    private static Dictionary<string, ArtifactLocation> GetBaseIds()
    {
        return new Dictionary<string, ArtifactLocation>(1)
        {
            {
                LOCATION_ID_REPOROOT,
                new ArtifactLocation
                {
                    Description = GetMessage(ReportStrings.SARIF_REPOROOT_Description),
                }
            }
        };
    }

    public SarifLog Build()
    {
        AddArtifacts();
        AddOptions();

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
            Rule = rule,
            Kind = GetKind(record),
            Level = GetLevel(record),
            Message = new Message { Text = record.Recommendation },
            Locations = GetLocations(record),
        };

        AddFields(result, record);
        AddAnnotations(result, record);
        AddArtifacts(record);

        // SARIF2004: Use the RuleId property instead of Rule for standalone rules.
        if (rule.ToolComponent.Guid == TOOL_GUID)
        {
            result.RuleId = rule.Id;
            result.Rule = null;
        }
        _Run.Results.Add(result);
    }

    /// <summary>
    /// Add non-null fields from the record to the result.
    /// </summary>
    private static void AddFields(Result result, RuleRecord record)
    {
        if (result == null || record?.Field == null || record.Field.Count == 0) return;

        // Filter out null values.
        var fields = new Hashtable();
        foreach (DictionaryEntry kv in record.Field)
        {
            if (kv.Value != null)
                fields[kv.Key] = kv.Value;
        }

        if (fields.Count > 0)
            result.SetProperty("fields", fields);
    }

    /// <summary>
    /// Add non-null annotations from the record to the result.
    /// </summary>
    private static void AddAnnotations(Result result, RuleRecord record)
    {
        if (result == null || record?.Info?.Annotations == null || record.Info.Annotations.Count == 0) return;

        // Filter out null values.
        var annotations = new Hashtable();
        foreach (DictionaryEntry kv in record.Info.Annotations)
        {
            if (kv.Value != null && !string.Equals("online version", kv.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                annotations[kv.Key] = kv.Value;
        }

        if (annotations.Count > 0)
            result.SetProperty("annotations", annotations);
    }

    /// <summary>
    /// Get collected artifacts.
    /// </summary>
    private void AddArtifacts()
    {
        _Run.Artifacts = _Artifacts.Values.OrderBy(item => item.Location.Index).ToList();
    }

    /// <summary>
    /// Add options to the run.
    /// </summary>
    private void AddOptions()
    {
        var s = new JsonSerializer();
        s.Converters.Add(new PSObjectJsonConverter());
        s.NullValueHandling = NullValueHandling.Ignore;

        var localScope = JObject.FromObject(_Option, s);
        var options = new JObject
        {
            ["workspace"] = localScope
        };

        _Run.SetProperty("options", options);
    }

    private void AddArtifacts(RuleRecord record)
    {
        if (record.Source == null || record.Source.Length == 0) return;

        foreach (var source in record.Source)
            AddArtifact(source);
    }

    private void AddArtifact(TargetSourceInfo source)
    {
        if (source == null || string.IsNullOrEmpty(source.File)) return;

        var relativePath = source.GetPath(useRelativePath: true);
        var fullPath = source.GetPath(useRelativePath: false);
        if (relativePath == null || fullPath == null || _Artifacts.ContainsKey(relativePath)) return;

        var location = new ArtifactLocation
        (
            uri: new Uri(relativePath, uriKind: UriKind.Relative),
            uriBaseId: LOCATION_ID_REPOROOT,
            index: _Artifacts.Count,
            description: null,
            properties: null
        );
        var artifact = new Artifact
        {
            Location = location,
            Hashes = GetArtifactHash(fullPath)
        };

        _Artifacts.Add(relativePath, artifact);
    }

    private Dictionary<string, string>? GetArtifactHash(string path)
    {
        if (!File.Exists(path)) return null;

        var hash = _ConfiguredHashAlgorithm.GetFileDigest(path);
        var result = new Dictionary<string, string>
        {
            [_ConfiguredHashAlgorithmName] = hash
        };
        if (_SHA265 != null)
        {
            result["sha-256"] = _SHA265.GetFileDigest(path);
        }
        return result;
    }

    private ReportingDescriptorReference GetRule(RuleRecord record)
    {
        var id = record.Ref ?? record.RuleId;
        if (!_Rules.TryGetValue(id, out var descriptorReference))
            descriptorReference = AddRule(record, id);

        return descriptorReference;
    }

    private ReportingDescriptorReference AddRule(RuleRecord record, string id)
    {
        if (string.IsNullOrEmpty(record.Info.ModuleName) || !_Extensions.TryGetValue(record.Info.ModuleName, out var toolComponent))
            toolComponent = _Run.Tool.Driver;

        // Add the rule to the component.
        var descriptor = new ReportingDescriptor
        {
            Id = id,
            Name = record.RuleName,
            ShortDescription = GetMessageString(record.Info.Synopsis),
            HelpUri = record.Info.GetOnlineHelpUri(),
            FullDescription = GetMessageString(record.Info.Description),
            MessageStrings = GetMessageStrings(record),
            DefaultConfiguration = new ReportingConfiguration
            {
                Enabled = true,
                Level = GetLevel(record.Default.Level),
            },
        };

        toolComponent.Rules.Add(descriptor);

        // Create a reference to the rule.
        var descriptorReference = new ReportingDescriptorReference
        {
            Id = descriptor.Id,
            ToolComponent = new ToolComponentReference
            {
                Guid = toolComponent.Guid,
                Name = toolComponent.Name,
                Index = _Run.Tool.Extensions == null ? -1 : _Run.Tool.Extensions.IndexOf(toolComponent),
            },
        };

        _Rules.Add(id, descriptorReference);

        // Create a configuration override if applicable.
        if (record.Override != null && record.Override.Level.HasValue && record.Override.Level.Value != SeverityLevel.None && record.Override.Level != record.Default.Level)
        {
            if (_Run.Invocations[0].RuleConfigurationOverrides == null)
                _Run.Invocations[0].RuleConfigurationOverrides = [];

            _Run.Invocations[0].RuleConfigurationOverrides.Add(new ConfigurationOverride
            {
                Descriptor = descriptorReference,
                Configuration = new ReportingConfiguration
                {
                    Level = GetLevel(record.Override.Level.Value),
                }
            });
        }

        return descriptorReference;
    }

    private static RunAutomationDetails? GetAutomationDetails()
    {
        return PipelineContext.CurrentThread == null ? null : new RunAutomationDetails
        {
            Id = PipelineContext.CurrentThread.RunId,
        };
    }

    private static List<Invocation> GetInvocation()
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

    private static MultiformatMessageString GetMessageString(InfoString text)
    {
        return new MultiformatMessageString
        {
            Text = text.Text
        };
    }

    private static MultiformatMessageString GetMessageString(string text)
    {
        return new MultiformatMessageString
        {
            Text = text
        };
    }

    private static Dictionary<string, MultiformatMessageString> GetMessageStrings(RuleRecord record)
    {
        return new Dictionary<string, MultiformatMessageString>(1)
        {
            {
                RECOMMENDATION_MESSAGE_ID,
                new MultiformatMessageString
                {
                    Text = record.Recommendation
                }
            }
        };
    }

    private static List<Location>? GetLocations(RuleRecord record)
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
        var region = new Region
        {
            StartLine = info.Line ?? 1,
            StartColumn = info.Position ?? 0,
        };
        var location = new PhysicalLocation
        {
            ArtifactLocation = new ArtifactLocation
            {
                Uri = new Uri(info.GetPath(useRelativePath: true), UriKind.Relative),
                UriBaseId = LOCATION_ID_REPOROOT,
            },
            Region = region,
        };
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

        return record.Level switch
        {
            SeverityLevel.Error => FailureLevel.Error,
            SeverityLevel.Warning => FailureLevel.Warning,
            SeverityLevel.Information => FailureLevel.Note,
            _ => FailureLevel.None,
        };
    }

    private static FailureLevel GetLevel(SeverityLevel level)
    {
        return level switch
        {
            SeverityLevel.Error => FailureLevel.Error,
            SeverityLevel.Warning => FailureLevel.Warning,
            SeverityLevel.Information => FailureLevel.Note,
            _ => FailureLevel.None,
        };
    }

    private Tool GetTool(Source[] source)
    {
        var version = Engine.GetVersion();
        return new Tool
        {
            Driver = new ToolComponent
            {
                Name = TOOL_NAME,
                SemanticVersion = version,
                Organization = TOOL_ORG,
                Guid = TOOL_GUID,
                Rules = new List<ReportingDescriptor>(),
                InformationUri = new Uri("https://aka.ms/ps-rule", UriKind.Absolute),
            },
            Extensions = GetExtensions(source),
        };
    }

    private List<ToolComponent>? GetExtensions(Source[] source)
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
                    Guid = source[i].Module.Guid,
                    AssociatedComponent = new ToolComponentReference
                    {
                        Name = TOOL_NAME,
                    },
                    InformationUri = new Uri(source[i].Module.ProjectUri, UriKind.Absolute),
                    Organization = source[i].Module.CompanyName,
                    Rules = new List<ReportingDescriptor>(),
                };
                _Extensions.Add(extension.Name, extension);
                result.Add(extension);
            }
        }
        return result.Count > 0 ? result : null;
    }
}

#nullable restore
