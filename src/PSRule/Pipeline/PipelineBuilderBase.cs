// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Globalization;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Options;
using PSRule.Pipeline.Output;
using PSRule.Resources;
using PSRule.Runtime;
using PSRule.Runtime.Scripting;

namespace PSRule.Pipeline;

/// <summary>
/// A base instance for a pipeline builder.
/// </summary>
internal abstract class PipelineBuilderBase : IPipelineBuilder
{
    private const string ENGINE_MODULE_NAME = "PSRule";

    protected readonly PSRuleOption Option;
    protected readonly Source[] Source;
    protected readonly IHostContext HostContext;

    private string[]? _Include;
    private Hashtable? _Tag;
    private Configuration.BaselineOption? _Baseline;
    private string[]? _Convention;
    private PathFilter? _InputFilter;
    private PipelineWriter? _Writer;
    private ILanguageScopeSet? _LanguageScopeSet;
    private CapabilitySet? _CapabilitySet;
    private RunspaceContext? _RunspaceContext;

    protected readonly HostPipelineWriter _Output;

    private const int MIN_JSON_INDENT = 0;
    private const int MAX_JSON_INDENT = 4;

    protected PipelineBuilderBase(Source[] source, IHostContext hostContext)
    {
        Option = new PSRuleOption();
        Source = source;
        _Output = new HostPipelineWriter(hostContext, Option, ShouldProcess);
        HostContext = hostContext;
    }

    /// <summary>
    /// Determines if the pipeline is executing in a remote PowerShell session.
    /// </summary>
    public bool InSession => HostContext != null && HostContext.InSession;

    /// <inheritdoc/>
    public void Name(string[]? name)
    {
        if (name == null || name.Length == 0)
            return;

        _Include = name;
        Option.Rule ??= new();
        Option.Rule.Include = ResourceHelper.GetResourceIdReference(_Include);
    }

    /// <inheritdoc/>
    public void Tag(Hashtable tag)
    {
        if (tag == null || tag.Count == 0)
            return;

        _Tag = tag;
        Option.Rule ??= new();
        Option.Rule.Tag = _Tag;
    }

    /// <inheritdoc/>
    public void Convention(string[] convention)
    {
        if (convention == null || convention.Length == 0)
            return;

        _Convention = convention;
    }

    /// <inheritdoc/>
    public virtual IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        Option.Baseline = new Options.BaselineOption(option.Baseline);
        Option.Binding = new BindingOption(option.Binding);
        Option.Capabilities = new CapabilityOption(option.Capabilities);
        Option.Convention = new ConventionOption(option.Convention);
        Option.Execution = GetExecutionOption(option.Execution);
        Option.Format = new FormatOption(option.Format);
        Option.Input = new InputOption(option.Input);
        Option.Input.StringFormat ??= InputOption.Default.StringFormat;
        Option.Output = new OutputOption(option.Output);
        Option.Output.Outcome ??= OutputOption.Default.Outcome;
        Option.Output.Banner ??= OutputOption.Default.Banner;
        Option.Output.Style = GetStyle(option.Output.Style ?? OutputOption.Default.Style!.Value);
        Option.Override = new OverrideOption(option.Override);
        Option.Repository = GetRepository(option.Repository);
        Option.Run = GetRun(option.Run);
        Option.Rule = RuleOption.Combine(Option.Rule, option.Rule);
        return this;
    }

    /// <inheritdoc/>
    public abstract IPipeline? Build(IPipelineWriter? writer = null);

    /// <inheritdoc/>
    public void Baseline(Configuration.BaselineOption? baseline)
    {
        if (baseline == null)
            return;

        _Baseline = baseline;
    }

    /// <summary>
    /// Require correct module versions for pipeline execution.
    /// </summary>
    protected bool RequireModules()
    {
        var result = true;
        if (Option.Requires.TryGetValue(ENGINE_MODULE_NAME, out var requiredVersion))
        {
            var engineVersion = Engine.GetVersion();
            if (GuardModuleVersion(ENGINE_MODULE_NAME, engineVersion, requiredVersion))
                result = false;
        }
        for (var i = 0; Source != null && i < Source.Length; i++)
        {
            if (Source[i].Module != null && Option.Requires.TryGetValue(Source[i].Module!.Name, out requiredVersion))
            {
                if (GuardModuleVersion(Source[i].Module!.Name, Source[i].Module!.FullVersion, requiredVersion))
                    result = false;
            }
        }
        return result;
    }

    /// <summary>
    /// Require sources for pipeline execution.
    /// </summary>
    protected bool RequireSources()
    {
        if (Source == null || Source.Length == 0)
        {
            PrepareWriter().LogNoValidSources(Option.Execution?.NoValidSources ?? ExecutionOption.Default.NoValidSources!.Value);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Require capabilities for pipeline execution
    /// </summary>
    protected bool RequireWorkspaceCapabilities()
    {
        if (Option.Capabilities == null || Option.Capabilities.Items == null || Option.Capabilities.Items.Length == 0)
            return true;

        // Get all capabilities.
        var set = GetCapabilitySet();

        // Check each capability.
        var result = true;
        foreach (var capability in Option.Capabilities.Items)
        {
            if (set.GetCapabilityState(capability) == CapabilityState.Disabled)
            {
                PrepareWriter().ErrorWorkspaceCapabilityDisabled(capability);
                result = false;
            }
            else if (set.GetCapabilityState(capability) == CapabilityState.Unknown)
            {
                PrepareWriter().ErrorWorkspaceCapabilityNotSupported(capability);
                result = false;
            }
        }
        return result;
    }

    /// <summary>
    /// Require module capabilities for pipeline execution.
    /// </summary>
    protected bool RequireModuleCapabilities(ResourceCache resourceCache)
    {
        // Get all capabilities.
        var set = GetCapabilitySet();

        // Check each capability.
        var result = true;
        foreach (var moduleConfig in resourceCache.OfType<IModuleConfig>())
        {

            if (moduleConfig.Spec is IModuleConfigV2Spec spec && spec?.Capabilities?.Items?.Length > 0)
            {
                foreach (var capability in spec.Capabilities.Items)
                {
                    if (set.GetCapabilityState(capability) == CapabilityState.Disabled)
                    {
                        PrepareWriter().ErrorModuleCapabilityDisabled(capability, moduleConfig.Name);
                        result = false;
                    }
                    else if (set.GetCapabilityState(capability) == CapabilityState.Unknown)
                    {
                        PrepareWriter().ErrorModuleCapabilityNotSupported(capability, moduleConfig.Name);
                        result = false;
                    }
                }
            }
        }
        return result;
    }

    protected CapabilitySet GetCapabilitySet()
    {
        if (_CapabilitySet != null)
            return _CapabilitySet;

        // Get all capabilities.
        _CapabilitySet = new CapabilitySet();
        _CapabilitySet.AddOptional("powershell-language", () =>
        {
            return Option.Execution?.RestrictScriptSource.GetValueOrDefault(ExecutionOption.Default.RestrictScriptSource!.Value) != RestrictScriptSource.DisablePowerShell;
        });
        return _CapabilitySet;
    }

    private bool GuardModuleVersion(string moduleName, string moduleVersion, string requiredVersion)
    {
        if (!TryModuleVersion(moduleVersion, requiredVersion))
        {
            var writer = PrepareWriter();
            writer.ErrorRequiredVersionMismatch(moduleName, moduleVersion, requiredVersion);
            writer.End(new DefaultPipelineResult(null, BreakLevel.None) { HadErrors = true });
            return true;
        }
        return false;
    }

    private static bool TryModuleVersion(string moduleVersion, string requiredVersion)
    {
        return SemanticVersion.TryParseVersion(moduleVersion, out var version) &&
            SemanticVersion.TryParseConstraint(requiredVersion, out var constraint) &&
            constraint.Accepts(version);
    }

    /// <summary>
    /// Create a pipeline context.
    /// </summary>
    protected PipelineContext? PrepareContext((BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField) binding, IPipelineWriter? writer = default, bool checkModuleCapabilities = false)
    {
        writer ??= PrepareWriter();
        var unresolved = new List<ResourceRef>();
        if (_Baseline is Configuration.BaselineOption.BaselineRef baselineRef)
            unresolved.Add(new BaselineRef(ResolveBaselineGroup(baselineRef.Name), ScopeType.Explicit));

        var languageScopeSet = GetLanguageScopeSet();
        var runspaceContext = GetRunspaceContext(Option, writer);
        var resourceCache = GetResourceCache(Option, writer, unresolved, languageScopeSet, runspaceContext);
        var options = GetOptionBuilder(resourceCache, binding);

        var baselines = Array.Empty<Baseline>();
        if (_Baseline is Configuration.BaselineOption.BaselineRef baselineRef2 && baselineRef2 != null)
        {
            var list = new List<Baseline>();

            // Only one baseline currently possible.
            var name = ResolveBaselineGroup(baselineRef2.Name);

            var b = resourceCache.OfType<Baseline>().FirstOrDefault(r => ResourceIdEqualityComparer.IdEquals(r.Id, name));
            if (b != null)
            {
                list.Add(b);
            }
            baselines = [.. list];
        }

        foreach (var scope in languageScopeSet.Get())
        {
            scope.Configure(options.Build(scope.Name));
        }

        if (checkModuleCapabilities && !RequireModuleCapabilities(resourceCache))
            return null;

        return PipelineContext.New(
            option: Option,
            hostContext: HostContext,
            reader: PrepareReader,
            writer: writer,
            languageScope: languageScopeSet,
            optionBuilder: options,
            resourceCache: resourceCache,
            runspaceContext: runspaceContext,
            baselines: baselines
        );
    }

    protected ILanguageScopeSet GetLanguageScopeSet()
    {
        if (_LanguageScopeSet != null)
            return _LanguageScopeSet;

        var builder = new LanguageScopeSetBuilder();
        builder.Init(Source);
        return _LanguageScopeSet = builder.Build();
    }

    protected string[]? ResolveBaselineGroup(string[]? name)
    {
        var result = new List<string>();
        for (var i = 0; name != null && i < name.Length; i++)
        {
            var n = ResolveBaselineGroup(name[i]);
            if (n != null)
            {
                result.Add(n);
            }
        }
        return result.Count == 0 ? null : [.. result];
    }

    protected string? ResolveBaselineGroup(string? name)
    {
        if (name == null || name.Length < 2 || !name.StartsWith("@") ||
            Option == null || Option.Baseline == null || Option.Baseline.Group == null ||
            Option.Baseline.Group.Count == 0)
            return name;

        var key = name.Substring(1);
        if (!Option.Baseline.Group.TryGetValue(key, out var baselines) || baselines.Length == 0)
            throw new PipelineConfigurationException("Baseline.Group", string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.PSR0003, key));

        var writer = PrepareWriter();
        writer.LogVerbose(EventId.None, "Using baseline group '{0}': {1}", key, baselines[0]);
        return baselines[0];
    }

    protected virtual PipelineInputStream PrepareReader()
    {
        return new PipelineInputStream(GetLanguageScopeSet(), null, GetInputObjectSourceFilter(), Option, _Output);
    }

    protected virtual PipelineWriter PrepareWriter()
    {
        if (_Writer != null)
            return _Writer;

        var output = GetOutput();
        return _Writer = Option.Output.Format switch
        {
            OutputFormat.Csv => new CsvOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Json => new JsonOutputWriter(output, Option, ShouldProcess),
            OutputFormat.NUnit3 => new NUnit3OutputWriter(output, Option, ShouldProcess),
            OutputFormat.Yaml => new YamlOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Markdown => new MarkdownOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Wide => new WideOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Sarif => new SarifOutputWriter(Source, output, Option, ShouldProcess),
            _ => output,
        };
    }

    protected virtual PipelineWriter GetOutput(bool writeHost = false)
    {
        // Redirect to file instead
        return !string.IsNullOrEmpty(Option.Output.Path) && Option.Output.Path != null
            ? new FileOutputWriter(
                inner: _Output,
                option: Option,
                encoding: Option.Output.GetEncoding(),
                path: Option.Output.Path,
                shouldProcess: HostContext.ShouldProcess,
                writeHost: writeHost
            )
            : _Output;
    }

    protected static string[]? GetCulture(string[]? culture)
    {
        var result = new List<string>();
        var parent = new List<string>();
        var set = new HashSet<string>();
        for (var i = 0; culture != null && i < culture.Length; i++)
        {
            var c = CultureInfo.CreateSpecificCulture(culture[i]);
            if (!set.Contains(c.Name))
            {
                result.Add(c.Name);
                set.Add(c.Name);
            }
            for (var p = c.Parent; !string.IsNullOrEmpty(p.Name); p = p.Parent)
            {
                if (!set.Contains(p.Name))
                {
                    parent.Add(p.Name);
                    set.Add(p.Name);
                }
            }
        }
        if (parent.Count > 0)
            result.AddRange(parent);

        return result.Count == 0 ? null : [.. result];
    }

    protected static RepositoryOption GetRepository(RepositoryOption option)
    {
        var result = new RepositoryOption(option);
        if (string.IsNullOrEmpty(result.Url) && GitHelper.TryRepository(out var url) && url != null)
            result.Url = url;

        if (string.IsNullOrEmpty(result.BaseRef) && GitHelper.TryBaseRef(out var baseRef) && baseRef != null)
            result.BaseRef = baseRef;

        return result;
    }

    protected static RunOption GetRun(RunOption option)
    {
        return RunOption.Combine(option, RunOption.Default);
    }

    /// <summary>
    /// Coalesce execution options with defaults.
    /// </summary>
    protected static ExecutionOption GetExecutionOption(ExecutionOption option)
    {
        var result = ExecutionOption.Combine(option, ExecutionOption.Default);

        // Handle when preference is set to none. The default should be used.
        result.AliasReference = result.AliasReference == ExecutionActionPreference.None ? ExecutionOption.Default.AliasReference!.Value : result.AliasReference;
        result.DuplicateResourceId = result.DuplicateResourceId == ExecutionActionPreference.None ? ExecutionOption.Default.DuplicateResourceId!.Value : result.DuplicateResourceId;
        result.InvariantCulture = result.InvariantCulture == ExecutionActionPreference.None ? ExecutionOption.Default.InvariantCulture!.Value : result.InvariantCulture;
        result.NoValidSources = result.NoValidSources == ExecutionActionPreference.None ? ExecutionOption.Default.NoValidSources!.Value : result.NoValidSources;
        result.RuleExcluded = result.RuleExcluded == ExecutionActionPreference.None ? ExecutionOption.Default.RuleExcluded!.Value : result.RuleExcluded;
        result.RuleInconclusive = result.RuleInconclusive == ExecutionActionPreference.None ? ExecutionOption.Default.RuleInconclusive!.Value : result.RuleInconclusive;
        result.RuleSuppressed = result.RuleSuppressed == ExecutionActionPreference.None ? ExecutionOption.Default.RuleSuppressed!.Value : result.RuleSuppressed;
        result.SuppressionGroupExpired = result.SuppressionGroupExpired == ExecutionActionPreference.None ? ExecutionOption.Default.SuppressionGroupExpired!.Value : result.SuppressionGroupExpired;
        result.UnprocessedObject = result.UnprocessedObject == ExecutionActionPreference.None ? ExecutionOption.Default.UnprocessedObject!.Value : result.UnprocessedObject;
        return result;
    }

    protected IPathFilter? GetInputObjectSourceFilter()
    {
        return Option.Input.IgnoreObjectSource.GetValueOrDefault(InputOption.Default.IgnoreObjectSource!.Value) ? GetInputFilter() : null;
    }

    protected IPathFilter GetInputFilter()
    {
        if (_InputFilter == null)
        {
            var basePath = Environment.GetWorkingPath();
            var ignoreGitPath = Option.Input.IgnoreGitPath ?? InputOption.Default.IgnoreGitPath!.Value;
            var ignoreRepositoryCommon = Option.Input.IgnoreRepositoryCommon ?? InputOption.Default.IgnoreRepositoryCommon!.Value;
            var builder = PathFilterBuilder.Create(basePath, Option.Input.PathIgnore, ignoreGitPath, ignoreRepositoryCommon);
            builder.UseGitIgnore();

            _InputFilter = builder.Build();
        }
        return _InputFilter;
    }

    /// <summary>
    /// Load sources into a resource cache.
    /// </summary>
    private ResourceCache GetResourceCache(PSRuleOption option, IPipelineWriter writer, List<ResourceRef> unresolved, ILanguageScopeSet languageScopeSet, RunspaceContext runspaceContext)
    {
        return new ResourceCacheBuilder(option, writer, runspaceContext, languageScopeSet).Import(Source).Build(unresolved);
    }

    private RunspaceContext GetRunspaceContext(PSRuleOption option, IPipelineWriter writer)
    {
        return _RunspaceContext ??= new RunspaceContext(option, writer);
    }

    protected void EnableFormatsByName(string[]? format)
    {
        if (format == null || format.Length == 0)
            return;

        Option.Format ??= [];
        foreach (var f in format)
        {
            if (string.IsNullOrWhiteSpace(f))
                continue;

            Option.Format[f] ??= new FormatType();
            Option.Format[f]!.Enabled = true;
        }
    }

    protected void ConfigureBinding(PSRuleOption option)
    {
    }

    private OptionContextBuilder GetOptionBuilder(ResourceCache resourceCache, (BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField) binding)
    {
        var builder = new OptionContextBuilder(Option, _Include, _Tag, _Convention, binding.bindTargetName, binding.bindTargetType, binding.bindField);

        foreach (var moduleConfig in resourceCache.OfType<IModuleConfig>())
        {
            builder.ModuleConfig(moduleConfig.Source.Module, moduleConfig.Name, moduleConfig.Spec);
        }

        foreach (var kv in resourceCache.OfType<Baseline>())
        {
            //builder.Baseline();
            // builder.Baseline(kv.Value.baselineRef.Type, kv.Value.baseline.BaselineId, kv.Value.baseline.Source.Module, kv.Value.baseline.Spec, kv.Value.baseline.Obsolete);
        }

        return builder;
    }

    /// <summary>
    /// Normalizes JSON indent range between minimum 0 and maximum 4.
    /// </summary>
    /// <param name="jsonIndent"></param>
    /// <returns>The number of characters to indent.</returns>
    protected static int NormalizeJsonIndentRange(int? jsonIndent)
    {
        if (jsonIndent.HasValue)
        {
            if (jsonIndent < MIN_JSON_INDENT)
                return MIN_JSON_INDENT;

            else if (jsonIndent > MAX_JSON_INDENT)
                return MAX_JSON_INDENT;

            return jsonIndent.Value;
        }
        return MIN_JSON_INDENT;
    }

    protected bool TryChangedFiles(out string[]? files)
    {
        files = null;
        if (!Option.Input.IgnoreUnchangedPath.GetValueOrDefault(InputOption.Default.IgnoreUnchangedPath!.Value) ||
            !GitHelper.TryGetChangedFiles(Option.Repository.BaseRef, "d", null, out files))
            return false;

        for (var i = 0; i < files.Length; i++)
        {
            HostContext.LogVerbose(EventId.None, PSRuleResources.UsingChangedFile, files[i]);
        }
        return true;
    }

    protected bool ShouldProcess(string target, string action)
    {
        return HostContext == null || HostContext.ShouldProcess(target, action);
    }

    protected static OutputStyle GetStyle(OutputStyle style)
    {
        if (style != OutputStyle.Detect)
            return style;

        if (Environment.IsAzurePipelines())
            return OutputStyle.AzurePipelines;

        if (Environment.IsGitHubActions())
            return OutputStyle.GitHubActions;

        return Environment.IsVisualStudioCode() ?
            OutputStyle.VisualStudioCode :
            OutputStyle.Client;
    }
}
