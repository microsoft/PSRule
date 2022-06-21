// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace PSRule.Definitions
{
    public interface IResourceHelpInfo
    {
        string Name { get; }

        string DisplayName { get; }

        InfoString Synopsis { get; }

        InfoString Description { get; }
    }

    internal sealed class ResourceHelpInfo : IResourceHelpInfo
    {
        internal ResourceHelpInfo(string name, string displayName, InfoString synopsis, InfoString description)
        {
            Name = name;
            DisplayName = displayName;
            Synopsis = synopsis;
            Description = description;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; }

        [JsonProperty(PropertyName = "synopsis")]
        public InfoString Synopsis { get; }

        [JsonProperty(PropertyName = "synopsis")]
        public InfoString Description { get; }
    }
}
