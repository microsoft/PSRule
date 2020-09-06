// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace PSRule.Data
{
    public sealed class InputFileInfo : ITargetInfo
    {
        private const char Backslash = '\\';
        private const char Slash = '/';

        internal readonly bool IsUrl;

        internal InputFileInfo(string basePath, string path)
        {
            FullName = path;
            if (path.IsUri())
            {
                IsUrl = true;
                return;
            }
            BasePath = basePath;
            Name = Path.GetFileName(path);
            Extension = Path.GetExtension(path);
            DirectoryName = Path.GetDirectoryName(path);
            DisplayName = FullName.Substring(basePath.Length).Replace(Backslash, Slash);
        }

        public string FullName { get; }

        public string BasePath { get; }

        public string Name { get; }

        public string Extension { get; }

        public string DirectoryName { get; }

        public string DisplayName { get; }

        string ITargetInfo.TargetName => DisplayName;

        string ITargetInfo.TargetType => Extension;
    }
}
