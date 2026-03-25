// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;

namespace PSRule.Converters.Json;

/// <summary>
/// A JSON converter for de/serializing <see cref="StringMap{TValue}"/>.
/// </summary>
public sealed class StringMapJsonConverter<TValue> : JsonConverter where TValue : class
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsSubclassOf(typeof(StringMap<TValue>));
    }

    /// <inheritdoc/>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is StringMap<TValue> map)
        {
            writer.WriteStartObject();
            foreach (var kv in map)
            {
                writer.WritePropertyName(kv.Key);
                serializer.Serialize(writer, kv.Value);
            }
            writer.WriteEndObject();
        }
    }
}
