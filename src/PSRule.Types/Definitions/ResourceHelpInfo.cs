// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace PSRule.Definitions;

internal sealed class ResourceHelpInfo : IResourceHelpInfo
{
    internal ResourceHelpInfo(string name, string displayName, InfoString synopsis, InfoString description)
    {
        Name = name;
        DisplayName = displayName;
        Synopsis = synopsis;
        Description = description;
    }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "name")]
    public string Name { get; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "displayName")]
    public string DisplayName { get; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "synopsis")]
    public InfoString Synopsis { get; }

    /// <inheritdoc/>
    [JsonProperty(PropertyName = "description")]
    public InfoString Description { get; }
}
