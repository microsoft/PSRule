// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;

namespace PSRule.Data
{
    public sealed class TargetIssueInfo
    {
        private const string PROPERTY_TYPE = "type";
        private const string PROPERTY_NAME = "name";
        private const string PROPERTY_PATH = "path";
        private const string PROPERTY_MESSAGE = "message";

        public TargetIssueInfo()
        {
            // Do nothing
        }

        [JsonProperty(PropertyName = PROPERTY_TYPE)]
        public string Type { get; internal set; }

        [JsonProperty(PropertyName = PROPERTY_NAME)]
        public string Name { get; internal set; }

        [JsonProperty(PropertyName = PROPERTY_PATH)]
        public string Path { get; internal set; }

        [JsonProperty(PropertyName = PROPERTY_MESSAGE)]
        public string Message { get; internal set; }

        public bool Equals(TargetIssueInfo other)
        {
            return other != null &&
                Type == other.Type &&
                Name == other.Name &&
                Path == other.Path &&
                Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            return obj is TargetIssueInfo info && Equals(info);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
                hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
                hash = hash * 23 + (Path != null ? Path.GetHashCode() : 0);
                hash = hash * 23 + (Message != null ? Message.GetHashCode() : 0);
                return hash;
            }
        }

        public static TargetIssueInfo Create(object o)
        {
            return o is PSObject pso ? Create(pso) : null;
        }

        public static TargetIssueInfo Create(PSObject o)
        {
            var result = new TargetIssueInfo();
            if (o.TryProperty(PROPERTY_TYPE, out string type))
                result.Type = type;

            if (o.TryProperty(PROPERTY_NAME, out string name))
                result.Name = name;

            if (o.TryProperty(PROPERTY_PATH, out string path))
                result.Path = path;

            if (o.TryProperty(PROPERTY_MESSAGE, out string message))
                result.Message = message;

            return result;
        }
    }
}
