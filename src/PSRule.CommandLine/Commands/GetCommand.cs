// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using PSRule.CommandLine.Models;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Pipeline.Dependencies;

namespace PSRule.CommandLine.Commands;

/// <summary>
/// Execute features of the <c>get</c> command through the CLI.
/// </summary>
public sealed class GetCommand
{
    /// <summary>
    /// A generic error.
    /// </summary>
    private const int ERROR_GENERIC = 1;

    /// <summary>
    /// Call <c>get rule</c>.
    /// </summary>
    public static async Task<int> GetRuleAsync(GetRuleOptions operationOptions, ClientContext clientContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var exitCode = 0;
            var workingPath = operationOptions.WorkspacePath ?? Environment.GetWorkingPath();
            var file = LockFile.Read(null);

            if (operationOptions.Path != null)
            {
                clientContext.Option.Include.Path = operationOptions.Path;
            }

            if (operationOptions.Name != null && operationOptions.Name.Length > 0)
            {
                clientContext.Option.Rule.Include = operationOptions.Name;
            }

            if (operationOptions.Baseline != null)
            {
                clientContext.Option.Baseline.Group = [operationOptions.Baseline];
            }

            if (operationOptions.Module != null && operationOptions.Module.Length > 0)
            {
                clientContext.Option.Requires.Module = operationOptions.Module;
            }

            if (!operationOptions.NoRestore)
            {
                var restoreBuilder = new ModuleRestoreBuilder(workingPath, clientContext.Option);
                var restoreResult = await restoreBuilder.RestoreAsync(file, restore: true, cancellationToken);
            }

            var builder = new GetRulePipelineBuilder([workingPath], clientContext);
            builder.Configure(clientContext.Option);

            if (operationOptions.IncludeDependencies)
                builder.IncludeDependencies();

            // Use a custom writer to capture the output
            var capturedObjects = new List<object>();
            var writer = new CapturingPipelineWriter(capturedObjects);

            using var pipeline = builder.Build(writer);
            if (pipeline != null)
            {
                pipeline.Begin();
                pipeline.End();

                // Convert captured objects to JSON and output
                var json = JsonSerializer.Serialize(capturedObjects, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                clientContext.Output.WriteHost(json);
            }

            return exitCode;
        }
        catch (Exception ex)
        {
            clientContext.Output.WriteError(ex.Message);
            return ERROR_GENERIC;
        }
    }

    /// <summary>
    /// A pipeline writer that captures objects for JSON serialization.
    /// </summary>
    private sealed class CapturingPipelineWriter : IPipelineWriter
    {
        private readonly List<object> _capturedObjects;

        public CapturingPipelineWriter(List<object> capturedObjects)
        {
            _capturedObjects = capturedObjects;
        }

        public int ExitCode { get; private set; }
        public bool HadErrors { get; private set; }
        public bool HadFailures { get; private set; }

        public void WriteObject(object o, bool enumerateCollection)
        {
            if (o != null)
            {
                if (enumerateCollection && o is System.Collections.IEnumerable enumerable && !(o is string))
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                            _capturedObjects.Add(item);
                    }
                }
                else
                {
                    _capturedObjects.Add(o);
                }
            }
        }

        public void WriteHost(System.Management.Automation.HostInformationMessage info) { }
        public void WriteResult(InvokeResult result) { }
        public void Begin() { }
        public void End(IPipelineResult result) { }
        public void SetExitCode(int exitCode) => ExitCode = exitCode;

        // ILogger implementation
        public void WriteError(System.Management.Automation.ErrorRecord errorRecord) => HadErrors = true;
        public void WriteWarning(string message) { }
        public void WriteVerbose(string message) { }
        public void WriteDebug(string message) { }
        public void WriteInformation(System.Management.Automation.InformationRecord informationRecord) { }
        
        // ILogger implementation
        public bool IsEnabled(LogLevel logLevel) => false;
        
        public void Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception? exception, 
            Func<TState, Exception?, string> formatter) 
        { 
            if (logLevel == LogLevel.Error || 
                logLevel == LogLevel.Critical)
                HadErrors = true;
        }

        public void Dispose() { }
    }
}