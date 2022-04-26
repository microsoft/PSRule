// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace PSRule.Definitions
{
    public interface IResourceHelpInfo
    {
        InfoString Synopsis { get; set; }

        InfoString Description { get; set; }
    }

    internal sealed class ResourceHelpInfo : IResourceHelpInfo
    {
        internal ResourceHelpInfo(string name)
        {
            //Synopsis = synopsis;
        }

        [JsonProperty(PropertyName = "synopsis")]
        public InfoString Synopsis { get; set; }

        [JsonProperty(PropertyName = "synopsis")]
        public InfoString Description { get; set; }
    }
}
