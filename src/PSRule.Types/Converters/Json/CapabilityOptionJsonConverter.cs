// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Options;

namespace PSRule.Converters.Json;

/// <summary>
/// A JSON converter for <see cref="CapabilityOption"/>.
/// </summary>
public sealed class CapabilityOptionJsonConverter : JsonConverter<CapabilityOption>
{
    /// <inheritdoc/>
    public override CapabilityOption? ReadJson(JsonReader reader, Type objectType, CapabilityOption? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TryConsume(JsonToken.StartArray))
        {
            var items = new List<string>();
            while (reader.TryConsume(JsonToken.String, out var value) && value is string s)
            {
                items.Add(s);
            }

            return new CapabilityOption
            {
                Items = [.. items]
            };
        }
        return null;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, CapabilityOption? value, JsonSerializer serializer)
    {
        if (value == null || value.Items == null)
            return;

        writer.WriteStartArray();
        foreach (var item in value.Items)
        {
            writer.WriteValue(item);
        }
        writer.WriteEndArray();
    }
}
