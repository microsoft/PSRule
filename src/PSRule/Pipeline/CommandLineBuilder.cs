// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Pipeline.Dependencies;

#nullable enable

namespace PSRule.Pipeline;

/// <summary>
/// A helper to create a PSRule pipeline that can be used via the .NET SDK.
/// </summary>
public static class CommandLineBuilder
{
    /// <summary>
    /// Create a builder for an Invoke pipeline.
    /// </summary>
    /// <remarks>
    /// Invoke pipelines process objects and produce records indicating the outcome of each rule.
    /// </remarks>
    /// <param name="module">The name of modules containing rules to process.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <param name="file">An optional lock file.</param>
    /// <param name="resolvedModuleVersions">A mapping of module versions to use when the specific version is not specified in options.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Invoke(string[] module, PSRuleOption option, IHostContext hostContext, LockFile? file = null, IDictionary<string, string>? resolvedModuleVersions = null)
    {
        var sourcePipeline = new SourcePipelineBuilder(hostContext, option, hostContext.CachePath ?? GetLocalPath());
        LoadModules(sourcePipeline, module, option, file, resolvedModuleVersions);

        var source = sourcePipeline.Build();
        var pipeline = new InvokeRulePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for an Assert pipeline.
    /// </summary>
    /// <remarks>
    /// Assert pipelines process objects with rules and produce text-based output suitable for output to a CI pipeline.
    /// </remarks>
    /// <param name="module">The name of modules containing rules to process.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <param name="file">An optional lock file.</param>
    /// <param name="resolvedModuleVersions">A mapping of module versions to use when the specific version is not specified in options.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Assert(string[] module, PSRuleOption option, IHostContext hostContext, LockFile? file = null, IDictionary<string, string>? resolvedModuleVersions = null)
    {
        var sourcePipeline = new SourcePipelineBuilder(hostContext, option, GetLocalPath());
        LoadModules(sourcePipeline, module, option, file, resolvedModuleVersions);

        var source = sourcePipeline.Build();
        var pipeline = new AssertPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    private static void LoadModules(SourcePipelineBuilder builder, string[] module, PSRuleOption option, LockFile? file, IDictionary<string, string>? resolvedModuleVersions)
    {
        for (var i = 0; module != null && i < module.Length; i++)
        {
            var version = file != null && file.Modules.TryGetValue(module[i], out var entry) ? entry.Version.ToString() : null;
            if (version == null && resolvedModuleVersions != null && resolvedModuleVersions.TryGetValue(module[i], out var resolvedVersion))
                version = resolvedVersion;

            builder.ModuleByName(module[i], version);
        }

        for (var i = 0; option?.Include?.Module != null && i < option.Include.Module.Length; i++)
        {
            var version = file != null && file.Modules.TryGetValue(option.Include.Module[i], out var entry) ? entry.Version.ToString() : null;
            if (version == null && resolvedModuleVersions != null && resolvedModuleVersions.TryGetValue(option.Include.Module[i], out var resolvedVersion))
                version = resolvedVersion;

            builder.ModuleByName(option.Include.Module[i], version);
        }
    }

    private static string? GetLocalPath()
    {
        return string.IsNullOrEmpty(AppContext.BaseDirectory) ? null : Environment.GetRootedBasePath(AppContext.BaseDirectory);
    }
}

#nullable restore
