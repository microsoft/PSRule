using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PSRule.Configuration
{
    /// <summary>
    /// The formats to convert input from.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InputFormat : byte
    {
        None = 0,

        Yaml = 1,

        Json = 2,

        Markdown = 3,

        Detect = 255
    }
}
