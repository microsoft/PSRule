using Newtonsoft.Json;
using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// Custom serializer to correctly convert PSObject properties to JSON instead of CLIXML.
    /// </summary>
    internal sealed class PSObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PSObject).IsAssignableFrom(objectType);
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
                // Ignore properties that are not readable
                if (!property.IsGettable)
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
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}
