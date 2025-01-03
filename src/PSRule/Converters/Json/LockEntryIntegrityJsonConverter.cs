// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Pipeline.Dependencies;

namespace PSRule.Converters.Json;

#nullable enable

/// <summary>
/// A converter for converting <see cref="LockEntryIntegrity"/> to/ from JSON.
/// </summary>
internal sealed class LockEntryIntegrityJsonConverter : JsonConverter<LockEntryIntegrity>
{
    public override LockEntryIntegrity? ReadJson(JsonReader reader, Type objectType, LockEntryIntegrity? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String || reader.Value is not string s || string.IsNullOrEmpty(s) || s.IndexOf('-') == -1)
            return null;

        var parts = s.Split('-');
        return Enum.TryParse<IntegrityAlgorithm>(parts[0], ignoreCase: true, result: out var algorithm)
            ? new LockEntryIntegrity(algorithm, hash: parts[1])
            : null;
    }

    public override void WriteJson(JsonWriter writer, LockEntryIntegrity? value, JsonSerializer serializer)
    {
        if (value == null || value.Algorithm == IntegrityAlgorithm.Unknown || string.IsNullOrEmpty(value.Hash))
            return;

        var algorithm = Enum.GetName(typeof(IntegrityAlgorithm), value.Algorithm).ToLower();

        writer.WriteValue($"{algorithm}-{value.Hash}");
    }
}

#nullable restore
