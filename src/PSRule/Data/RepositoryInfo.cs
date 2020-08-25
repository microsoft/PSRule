// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data
{
    public sealed class RepositoryInfo : ITargetInfo
    {
        internal RepositoryInfo(string basePath, string headRef)
        {
            FullName = basePath;
            BasePath = basePath;
            DisplayName = headRef;
            Type = typeof(RepositoryInfo).FullName;
        }

        public string FullName { get; }

        public string BasePath { get; }

        public string DisplayName { get; }

        public string Type { get; }

        string ITargetInfo.TargetName => DisplayName;

        string ITargetInfo.TargetType => Type;
    }
}
