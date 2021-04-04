// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace PSRule.Data
{
    public sealed class InputFileInfo : ITargetInfo
    {
        private readonly string _TargetType;

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
            DisplayName = ExpressionHelpers.NormalizePath(basePath, FullName);
            _TargetType = string.IsNullOrEmpty(Extension) ? Path.GetFileNameWithoutExtension(path) : Extension;
        }

        public string FullName { get; }

        public string BasePath { get; }

        public string Name { get; }

        public string Extension { get; }

        public string DirectoryName { get; }

        public string DisplayName { get; }

        string ITargetInfo.TargetName => DisplayName;

        string ITargetInfo.TargetType => _TargetType;

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Convert to FileInfo.
        /// </summary>
        public FileInfo AsFileInfo()
        {
            return new FileInfo(FullName);
        }
    }
}
