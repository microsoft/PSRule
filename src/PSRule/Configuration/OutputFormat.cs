using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputFormat : byte
    {
        None = 0,

        Yaml = 1,

        Json = 2,

        NUnit3 = 3,

        Csv = 4,

        Wide = 5
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputFormatGet : byte
    {
        None = 0,

        Wide = 5
    }
}
