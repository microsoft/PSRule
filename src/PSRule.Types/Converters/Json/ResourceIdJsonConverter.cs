// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;

namespace PSRule.Converters.Json;

/// <summary>
/// A converter for converting <see cref="ResourceId"/> to/ from JSON.
/// </summary>
public sealed class ResourceIdJsonConverter : JsonConverter<ResourceId>
{
    /// <inheritdoc/>
    public override ResourceId ReadJson(JsonReader reader, Type objectType, ResourceId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = reader.ReadAsString();
        return value != null ? ResourceId.Parse(value) : default;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ResourceId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Value);
    }
}
