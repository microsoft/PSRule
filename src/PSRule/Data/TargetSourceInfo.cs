// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.IO;

namespace PSRule.Data
{
    public sealed class TargetSourceInfo
    {
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

        [JsonProperty(PropertyName = "file")]
        public string File { get; internal set; }

        [JsonProperty(PropertyName = "line")]
        public int? Line { get; internal set; }

        [JsonProperty(PropertyName = "position")]
        public int? Position { get; internal set; }

        public bool Equals(TargetSourceInfo other)
        {
            return other != null &&
                File == other.File &&
                Line == other.Line &&
                Position == other.Position;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                int hash = 17;
                hash = hash * 23 + (File != null ? File.GetHashCode() : 0);
                hash = hash * 23 + (Line.HasValue ? Line.Value.GetHashCode() : 0);
                hash = hash * 23 + (Position.HasValue ? Position.Value.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
