// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using System.Management.Automation;
using PSRule.Runtime;

namespace PSRule.Definitions.Conventions;

[DebuggerDisplay("Kind = {Kind}, ApiVersion = {ApiVersion}, Id = {Id}")]
internal sealed class ScriptBlockConvention : BaseConvention, IConventionV1, IDisposable
{
    private readonly LanguageScriptBlock? _Initialize;
    private readonly LanguageScriptBlock? _Begin;
    private readonly LanguageScriptBlock? _Process;
    private readonly LanguageScriptBlock? _End;

    private bool _Disposed;

    internal ScriptBlockConvention(
        ISourceFile source,
        ResourceMetadata metadata,
        ResourceHelpInfo info,
        LanguageScriptBlock? begin,
        LanguageScriptBlock? initialize,
        LanguageScriptBlock? process,
        LanguageScriptBlock? end,
        ActionPreference errorPreference,
        ResourceFlags flags,
        ISourceExtent extent)
        : base(source, metadata.Name)
    {
        Info = info;
        _Initialize = initialize;
        _Begin = begin;
        _Process = process;
        _End = end;
        Flags = flags;
        Extent = extent;
    }

    public IResourceHelpInfo Info { get; }

    public ResourceFlags Flags { get; }

    public ISourceExtent Extent { get; }

    public ResourceKind Kind => ResourceKind.Convention;

    public string ApiVersion => Specs.V1;

    // Not supported with conventions.
    ResourceId? IResource.Ref => null;

    // Not supported with conventions.
    ResourceId[]? IResource.Alias => null;

    // Not supported with conventions.
    IResourceTags? IResource.Tags => null;

    // Not supported with conventions.
    IResourceLabels? IResource.Labels => null;

    /// <inheritdoc/>
    public override void Initialize(IConventionContext context, IEnumerable input)
    {
        if (_Initialize == null)
            return;

        context.Logger?.LogDebug(EventId.None, $"Initializing convention '{Id}'.");

        InvokeConventionBlock(context, Source, _Initialize, input);
    }

    /// <inheritdoc/>
    public override void Begin(IConventionContext context, IEnumerable input)
    {
        if (_Begin == null)
            return;

        InvokeConventionBlock(context, Source, _Begin, input);
    }

    /// <inheritdoc/>
    public override void Process(IConventionContext context, IEnumerable input)
    {
        if (_Process == null)
            return;

        InvokeConventionBlock(context, Source, _Process, input);
    }

    /// <inheritdoc/>
    public override void End(IConventionContext context, IEnumerable input)
    {
        if (_End == null)
            return;

        InvokeConventionBlock(context, Source, _End, input);
    }

    private static void InvokeConventionBlock(IConventionContext context, ISourceFile source, LanguageScriptBlock block, IEnumerable input)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (block == null) throw new ArgumentNullException(nameof(block));

        try
        {
            context.EnterLanguageScope(source);
            block.Invoke();
        }
        finally
        {
            context.ExitLanguageScope(source);
        }
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Begin?.Dispose();

                _Process?.Dispose();

                _End?.Dispose();
            }
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
