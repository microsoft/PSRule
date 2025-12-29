// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Definitions;

namespace PSRule.Converters.Json;

/// <summary>
/// A converter for converting <see cref="ResourceIdReference"/> to/ from JSON.
/// </summary>
public sealed class ResourceIdReferenceJsonConverter : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ResourceIdReference) || objectType == typeof(ResourceIdReference?) || objectType == typeof(ResourceIdReference[]);
    }

    /// <inheritdoc/>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TryConsume(JsonToken.String, out var token) && token is string s)
        {
            if (!ConvertOne(s, out var value) || value == null)
                return null;

            return objectType == typeof(ResourceIdReference) ? value.Value : value;
        }

        if (objectType == typeof(ResourceIdReference[]) && reader.TryConsume(JsonToken.StartArray))
        {
            var list = new List<ResourceIdReference>();

            while (reader.TokenType != JsonToken.EndArray)
            {
                reader.SkipComments(out _);

                if (reader.TryConsume(JsonToken.String, out var item) && item is string s2 && ConvertOne(s2, out var value) && value != null)
                    list.Add(value.Value);

                reader.SkipComments(out _);
            }

            return list.ToArray();
        }

        return null;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is ResourceIdReference reference)
        {
            writer.WriteValue(reference.Raw);
        }
        else if (value is ResourceIdReference[] references)
        {
            writer.WriteStartArray();

            foreach (var r in references)
            {
                writer.WriteValue(r.Raw);
            }

            writer.WriteEndArray();
        }
    }

    private static bool ConvertOne(string value, out ResourceIdReference? result)
    {
        return ResourceIdReference.TryParse(value, out result);
    }
}
