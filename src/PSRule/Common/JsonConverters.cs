using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule
{
    /// <summary>
    /// A custom serializer to correctly convert PSObject properties to JSON instead of CLIXML.
    /// </summary>
    internal sealed class PSObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PSObject);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is PSObject obj))
            {
                throw new ArgumentException();
            }

            writer.WriteStartObject();

            foreach (var property in obj.Properties)
            {
                // Ignore properties that are not readable or can cause race condition
                if (!property.IsGettable || property.Value is PSDriveInfo || property.Value is ProviderInfo)
                {
                    continue;
                }

                writer.WritePropertyName(property.Name);
                serializer.Serialize(writer, property.Value);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Create target object based on JObject
            var result = existingValue as PSObject ?? new PSObject();

            // Read tokens
            ReadObject(value: result, reader: reader);

            return result;
        }

        private void ReadObject(PSObject value, JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new Exception("Read json failed");
            }

            reader.Read();

            string name = null;

            // Read each token
            while (reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        name = reader.Value.ToString();
                        break;

                    case JsonToken.StartObject:
                        var child = new PSObject();
                        ReadObject(value: child, reader: reader);
                        value.Properties.Add(new PSNoteProperty(name: name, value: child));
                        break;

                    default:
                        value.Properties.Add(new PSNoteProperty(name: name, value: reader.Value));
                        break;
                }

                reader.Read();
            }
        }
    }

    /// <summary>
    /// A custom serializer to convert PSObjects that may or maynot be in a JSON array to an a PSObject array.
    /// </summary>
    internal sealed class PSObjectArrayJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PSObject[]);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject && reader.TokenType != JsonToken.StartArray)
            {
                throw new Exception("Read json failed");
            }

            var result = new List<PSObject>();

            var isArray = reader.TokenType == JsonToken.StartArray;

            if (isArray)
            {
                reader.Read();
            }

            while (!isArray || (isArray && reader.TokenType != JsonToken.EndArray))
            {
                var value = ReadObject(reader: reader);
                result.Add(value);

                // Consume the EndObject token
                if (isArray)
                {
                    reader.Read();
                }
            }

            return result.ToArray();
        }

        private PSObject ReadObject(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new Exception("Read json failed");
            }

            reader.Read();

            var result = new PSObject();

            string name = null;

            // Read each token
            while (reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        name = reader.Value.ToString();
                        break;

                    case JsonToken.StartObject:
                        var value = ReadObject(reader: reader);
                        result.Properties.Add(new PSNoteProperty(name: name, value: value));
                        break;

                    default:
                        result.Properties.Add(new PSNoteProperty(name: name, value: reader.Value));
                        break;
                }

                reader.Read();
            }

            return result;
        }
    }
}
