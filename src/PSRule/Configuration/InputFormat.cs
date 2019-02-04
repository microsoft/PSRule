using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The format to convert input strings to.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InputFormat : byte
    {
        None = 0,

        Yaml = 1,

        Json = 2
    }
}
