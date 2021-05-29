// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;
using System;
using System.IO;
using System.Management.Automation;

namespace PSRule.Data
{
    public sealed class TargetSourceInfo
    {
        private const string PROPERTY_FILE = "file";
        private const string PROPERTY_LINE = "line";
        private const string PROPERTY_POSITION = "position";
        private const string PROPERTY_TYPE = "type";

        private const string COLON = ":";
        private const string COLONSPACE = ": ";

        public TargetSourceInfo()
        {
            // Do nothing
        }

        internal TargetSourceInfo(InputFileInfo info)
        {
            File = info.FullName;
        }

        internal TargetSourceInfo(FileInfo info)
        {
            File = info.FullName;
        }

        internal TargetSourceInfo(Uri uri)
        {
            File = uri.AbsoluteUri;
        }

        [JsonProperty(PropertyName = PROPERTY_FILE)]
        public string File { get; internal set; }

        [JsonProperty(PropertyName = PROPERTY_LINE)]
        public int? Line { get; internal set; }

        [JsonProperty(PropertyName = PROPERTY_POSITION)]
        public int? Position { get; internal set; }

        [JsonProperty(PropertyName = PROPERTY_TYPE)]
        public string Type { get; internal set; }

        public bool Equals(TargetSourceInfo other)
        {
            return other != null &&
                File == other.File &&
                Line == other.Line &&
                Position == other.Position &&
                Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (File != null ? File.GetHashCode() : 0);
                hash = hash * 23 + (Line.HasValue ? Line.Value.GetHashCode() : 0);
                hash = hash * 23 + (Position.HasValue ? Position.Value.GetHashCode() : 0);
                hash = hash * 23 + (Type != null ? Type.GetHashCode() : 0);
                return hash;
            }
        }

        public override string ToString()
        {
            return ToString(null, false);
        }

        public string ToString(string defaultType, bool useRelativePath)
        {
            var type = Type ?? defaultType;
            var file = useRelativePath ? ExpressionHelpers.NormalizePath(PSRuleOption.GetWorkingPath(), File) : File;
            return string.IsNullOrEmpty(type) ? string.Concat(file, COLON, Line, COLON, Position) : string.Concat(type, COLONSPACE, file, COLON, Line, COLON, Position);
        }

        public static TargetSourceInfo Create(object o)
        {
            if (o is PSObject pso)
                return Create(pso);

            return null;
        }

        public static TargetSourceInfo Create(PSObject o)
        {
            var result = new TargetSourceInfo();
            if (o.TryProperty(PROPERTY_FILE, out string file))
                result.File = file;

            if (o.TryProperty(PROPERTY_LINE, out int line))
                result.Line = line;

            if (o.TryProperty(PROPERTY_POSITION, out int position))
                result.Position = position;

            if (o.TryProperty(PROPERTY_TYPE, out string type))
                result.Type = type;

            return result;
        }
    }
}
