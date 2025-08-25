// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using PSRule.CommandLine;
using PSRule.Configuration;
using Xunit;

namespace PSRule.EditorServices.Tests;

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
    
    private static ClientContext CreateClientContext(string optionPath, string workingPath)
    {
        var p = new Parser();
        var result = p.Parse(string.Empty);
        var invocationContext = new InvocationContext(result);
        
        return new ClientContext(
            invocation: invocationContext,
            option: optionPath,
            verbose: false,
            debug: false,
            workingPath: workingPath
        );
    }
}