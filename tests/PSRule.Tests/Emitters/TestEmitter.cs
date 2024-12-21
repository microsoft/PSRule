// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Emitters;

#nullable enable

/// <summary>
/// An emitter for testing.
/// </summary>
public sealed class TestEmitter : FileEmitter
{
    private readonly ImmutableHashSet<string>? _Types;
    private readonly Func<IEmitterContext, IFileStream, bool>? _VisitFile;

    public TestEmitter(ILogger<TestEmitter> logger, IEmitterConfiguration? emitterConfiguration)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Configuration = emitterConfiguration;
    }

    internal TestEmitter(string[] types, Func<IEmitterContext, IFileStream, bool> visitFile)
    {
        _Types = new HashSet<string>(types, StringComparer.OrdinalIgnoreCase).ToImmutableHashSet();
        _VisitFile = visitFile;
    }

    public ILogger<TestEmitter>? Logger { get; }

    public IEmitterConfiguration? Configuration { get; }

    protected override bool AcceptsFilePath(IEmitterContext context, IFileInfo info)
    {
        return info != null && _Types.Contains(info.Extension);
    }

    protected override bool VisitFile(IEmitterContext context, IFileStream stream)
    {
        return _VisitFile != null && _VisitFile(context, stream);
    }
}

#nullable restore
