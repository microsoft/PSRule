// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Data;

namespace PSRule.Converters.Json;

/// <summary>
///  A converter for converting <see cref="SemanticVersion.Version"/> to/ from JSON.
/// </summary>
internal sealed class SemanticVersionJsonConverter : JsonConverter<SemanticVersion.Version>
{
    public override SemanticVersion.Version? ReadJson(JsonReader reader, Type objectType, SemanticVersion.Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.TokenType == JsonToken.String && SemanticVersion.TryParseVersion(reader.Value as string, out var version) ? version : default;
    }

    public override void WriteJson(JsonWriter writer, SemanticVersion.Version? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }
}
