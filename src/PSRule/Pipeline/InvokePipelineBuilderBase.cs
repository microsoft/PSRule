// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;

namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A pipeline builder for any pipelines that test objects against rules.
/// </summary>
internal abstract class InvokePipelineBuilderBase : PipelineBuilderBase, IInvokePipelineBuilder
{
    protected InputPathBuilder? _InputPath;
    protected string? _ResultVariableName;

    private List<string>? _TrustedPublishers;

    protected InvokePipelineBuilderBase(Source[] source, IHostContext hostContext)
        : base(source, hostContext)
    {
        _InputPath = null;
    }

    /// <inheritdoc/>
    public void InputPath(string[]? path)
    {
        if (path == null || path.Length == 0)
            return;

        var basePath = Environment.GetWorkingPath();
        var filter = GetInputFilter();

        // Wrap with a filter that only allows files that have changed.
        if (TryChangedFiles(out var files) && files != null)
        {
            filter = new ChangedFilesPathFilter(filter, basePath, files);
        }

        var builder = new InputPathBuilder(PrepareWriter(), basePath, "*", filter, null);
        builder.Add(path);
        _InputPath = builder;
    }

    /// <inheritdoc/>
    public void ResultVariable(string variableName)
    {
        _ResultVariableName = variableName;
    }

    /// <inheritdoc/>
    public void UnblockPublisher(string publisher)
    {
        if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        _TrustedPublishers ??= [];
        _TrustedPublishers.Add(publisher);
    }

    /// <inheritdoc/>
    public void Formats(string[]? format)
    {
        EnableFormatsByName(format);
    }

    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        base.Configure(option);

        Option.Output.As = option.Output.As ?? OutputOption.Default.As;
        Option.Output.Culture = GetCulture(option.Output.Culture);
        Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
        Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
        Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;
        Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);

        if (option.Configuration != null)
            Option.Configuration = new(option.Configuration);

        ConfigureBinding(option);
        Option.Requires = [.. option.Requires];
        if (option.Suppression.Count > 0)
            Option.Suppression = new(option.Suppression);

        return this;
    }

    public override IPipeline? Build(IPipelineWriter? writer = null)
    {
        writer ??= PrepareWriter();
        Unblock(writer);
        if (!RequireModules() || !RequireWorkspaceCapabilities() || !RequireSources())
            return null;

        var context = PrepareContext(PipelineHookActions.Default, writer, checkModuleCapabilities: true);
        if (context == null)
            return null;

        return new InvokeRulePipeline
        (
            context: context,
            source: Source,
            outcome: Option.Output.Outcome.Value
        );
    }

    protected void Unblock(IPipelineWriter writer)
    {
        if (Source == null || Source.Length == 0 ||
            _TrustedPublishers == null || _TrustedPublishers.Count == 0 ||
            System.Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        var files = new List<string>();
        for (var i = 0; i < Source.Length; i++)
        {
            for (var j = 0; j < Source[i].File.Length; j++)
            {
                if (Source[i].File[j].Type == SourceType.Script && IsBlocked(Source[i].File[j].Path))
                    files.Add(Source[i].File[j].Path);
            }
        }
        if (files.Count > 0 && Option.Execution.RestrictScriptSource != Options.RestrictScriptSource.DisablePowerShell)
        {
            HostHelper.UnblockFile(writer, _TrustedPublishers.ToArray(), files.ToArray());
        }
    }

    private static bool IsBlocked(string path)
    {
        try
        {
            var zone = File.ReadLines(string.Concat(path, ":Zone.Identifier")).FirstOrDefault(s => s.StartsWith("ZoneId="));
            return zone != null;
        }
        catch
        {
            return false;
        }
    }

    protected override PipelineInputStream PrepareReader()
    {
        return new PipelineInputStream(GetLanguageScopeSet(), _InputPath, GetInputObjectSourceFilter(), Option, _Output);
    }
}

#nullable restore
