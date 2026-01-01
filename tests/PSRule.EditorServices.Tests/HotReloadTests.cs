// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.CommandLine;

namespace PSRule.EditorServices;

/// <summary>
/// Tests for hot reload functionality.
/// </summary>
public class HotReloadTests
{
    [Fact]
    public void ClientContext_ReloadOptions_UpdatesOptionsSuccessfully()
    {
        // Create a test directory with options file
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var optionsPath = Path.Combine(testDir, "ps-rule.yaml");

            // Create initial options file
            File.WriteAllText(optionsPath, @"
execution:
  aliasReference: Error
");

            // Create client context
            var context = CreateClientContext(optionsPath, testDir);

            // Verify initial options
            Assert.Equal(Options.ExecutionActionPreference.Error, context.Option.Execution.AliasReference);

            // Update options file
            File.WriteAllText(optionsPath, @"
execution:
  aliasReference: Warn
");

            // Reload options
            context.ReloadOptions();

            // Verify options were reloaded
            Assert.Equal(Options.ExecutionActionPreference.Warn, context.Option.Execution.AliasReference);
        }
        finally
        {
            // Cleanup
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void ClientContext_UpdateOptionsPath_ChangesPath()
    {
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var optionsPath1 = Path.Combine(testDir, "ps-rule1.yaml");
            var optionsPath2 = Path.Combine(testDir, "ps-rule2.yaml");

            // Create first options file
            File.WriteAllText(optionsPath1, @"
execution:
  aliasReference: Error
");

            // Create second options file
            File.WriteAllText(optionsPath2, @"
execution:
  aliasReference: Warn
");

            // Create client context with first options
            var context = CreateClientContext(optionsPath1, testDir);

            // Verify initial options
            Assert.Equal(Options.ExecutionActionPreference.Error, context.Option.Execution.AliasReference);

            // Update options path and reload
            context.UpdateOptionsPath(optionsPath2);
            context.ReloadOptions();

            // Verify new options were loaded
            Assert.Equal(Options.ExecutionActionPreference.Warn, context.Option.Execution.AliasReference);
        }
        finally
        {
            // Cleanup
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void ClientContext_OptionsPath_ReturnsConfiguredPath()
    {
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var optionsPath = Path.Combine(testDir, "custom-options.yaml");

            // Create options file
            File.WriteAllText(optionsPath, @"
execution:
  aliasReference: Error
");

            // Create client context with custom options path
            var context = CreateClientContext(optionsPath, testDir);

            // Verify OptionsPath property returns the configured path
            Assert.Equal(optionsPath, context.OptionsPath);

            // Test with null path
            var contextWithNull = CreateClientContext(null!, testDir);
            Assert.Null(contextWithNull.OptionsPath);
        }
        finally
        {
            // Cleanup
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void ConfigurationChangeHandler_Handle_UpdatesOptionsPath()
    {
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var optionsPath1 = Path.Combine(testDir, "ps-rule1.yaml");
            var optionsPath2 = Path.Combine(testDir, "ps-rule2.yaml");

            // Create options files
            File.WriteAllText(optionsPath1, @"
execution:
  aliasReference: Error
");

            File.WriteAllText(optionsPath2, @"
execution:
  aliasReference: Warn
");

            // Create client context with first options
            var context = CreateClientContext(optionsPath1, testDir);
            var logger = new TestLogger();
            var handler = new Handlers.ConfigurationChangeHandler(context, logger);

            // Verify initial options
            Assert.Equal(Options.ExecutionActionPreference.Error, context.Option.Execution.AliasReference);

            // Create configuration change request with raw JSON string
            var configJson = $@"{{
    ""PSRule"": {{
        ""options"": {{
            ""path"": ""{optionsPath2.Replace("\\", "\\\\").Replace("\"", "\\\"")}""
        }}
    }}
}}";

            var request = new OmniSharp.Extensions.LanguageServer.Protocol.Models.DidChangeConfigurationParams
            {
                Settings = configJson  // Pass the JSON string directly
            };

            // Handle configuration change
            var result = handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsCompletedSuccessfully);

            // Verify options path was updated and options reloaded
            Assert.Equal(optionsPath2, context.OptionsPath);
            Assert.Equal(Options.ExecutionActionPreference.Warn, context.Option.Execution.AliasReference);
        }
        finally
        {
            // Cleanup
            Directory.Delete(testDir, true);
        }
    }

    private static ClientContext CreateClientContext(string optionPath, string workingPath, IConsole? console = null)
    {
        return new ClientContext(
            console: console ?? new TestConsole(),
            option: optionPath,
            verbose: false,
            debug: false,
            workingPath: workingPath
        );
    }

    /// <summary>
    /// A simple test logger for testing purposes.
    /// </summary>
    private class TestLogger : Runtime.ILogger
    {
        public bool IsEnabled(Runtime.LogLevel logLevel) => true;

        public void Log<TState>(Runtime.LogLevel logLevel, Runtime.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Do nothing for tests
        }

        public void LogInformation(Runtime.EventId eventId, string message, params object[] args)
        {
            // Do nothing for tests
        }

        public void LogError(Runtime.EventId eventId, Exception exception, string message, params object[] args)
        {
            // Do nothing for tests
        }

        public void LogWarning(Runtime.EventId eventId, string message, params object[] args)
        {
            // Do nothing for tests
        }

        public void LogVerbose(Runtime.EventId eventId, string message, params object[] args)
        {
            // Do nothing for tests
        }

        public void LogDebug(Runtime.EventId eventId, string message, params object[] args)
        {
            // Do nothing for tests
        }
    }
}
