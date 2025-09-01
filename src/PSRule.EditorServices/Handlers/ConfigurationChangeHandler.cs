// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using PSRule.CommandLine;
using PSRule.Runtime;

namespace PSRule.EditorServices.Handlers;

/// <summary>
/// Handler for workspace configuration change notifications to update PSRule options path.
/// </summary>
public sealed class ConfigurationChangeHandler(ClientContext context, ILogger logger) : IDidChangeConfigurationHandler
{
    private readonly ClientContext _Context = context;
    private readonly ILogger _Logger = logger;

    /// <summary>
    /// Handle configuration change notifications and update options path if needed.
    /// </summary>
    public Task<Unit> Handle(DidChangeConfigurationParams request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the configuration contains PSRule settings
            if (request.Settings is { } settings)
            {
                // Extract PSRule configuration from the settings
                var psruleConfig = ExtractPSRuleConfiguration(settings);
                if (psruleConfig != null && psruleConfig.TryGetValue("options", out var optionsConfig))
                {
                    if (optionsConfig is Dictionary<string, object> options && options.TryGetValue("path", out var pathValue))
                    {
                        var newOptionsPath = pathValue?.ToString();
                        var currentOptionsPath = _Context.OptionsPath;

                        if (!string.Equals(newOptionsPath, currentOptionsPath, StringComparison.OrdinalIgnoreCase))
                        {
                            _Logger.LogInformation(EventId.None, "PSRule options path changed from '{0}' to '{1}'", currentOptionsPath, newOptionsPath);

                            // Update the options path in the context
                            if (!string.IsNullOrEmpty(newOptionsPath))
                            {
                                _Context.UpdateOptionsPath(newOptionsPath);
                            }
                            else
                            {
                                // Reset to default if path is cleared
                                _Context.UpdateOptionsPath("ps-rule.yaml");
                            }

                            // Reload options with the new path
                            _Context.ReloadOptions();
                            _Logger.LogInformation(EventId.None, "Successfully updated PSRule options path and reloaded options.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(EventId.None, ex, "Failed to handle configuration change: {0}", ex.Message);
        }

        return Task.FromResult(Unit.Value);
    }

    /// <summary>
    /// Set capability is required by ICapability interface.
    /// </summary>
    public void SetCapability(DidChangeConfigurationCapability capability, ClientCapabilities clientCapabilities)
    {
        // No additional capability configuration needed
    }

    private static Dictionary<string, object>? ExtractPSRuleConfiguration(object settings)
    {
        try
        {
            // Handle different JSON representations of the settings
            if (settings is Dictionary<string, object> dict)
            {
                return dict.TryGetValue("PSRule", out var psruleValue) && psruleValue is Dictionary<string, object> psruleDict
                    ? psruleDict
                    : null;
            }

            // Convert the settings to string and parse as JSON
            var settingsJson = settings.ToString();
            if (string.IsNullOrEmpty(settingsJson))
                return null;

            var jsonDoc = System.Text.Json.JsonDocument.Parse(settingsJson);
            if (jsonDoc.RootElement.TryGetProperty("PSRule", out var psruleElement))
            {
                var result = new Dictionary<string, object>();
                
                if (psruleElement.TryGetProperty("options", out var optionsElement))
                {
                    var optionsDict = new Dictionary<string, object>();
                    
                    if (optionsElement.TryGetProperty("path", out var pathElement))
                    {
                        optionsDict["path"] = pathElement.GetString() ?? string.Empty;
                    }
                    
                    result["options"] = optionsDict;
                }
                
                return result;
            }
        }
        catch (Exception)
        {
            // Ignore parsing errors and return null
        }

        return null;
    }
}