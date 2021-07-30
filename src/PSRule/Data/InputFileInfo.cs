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
            Name = System.IO.Path.GetFileName(path);
            Extension = System.IO.Path.GetExtension(path);
            DirectoryName = System.IO.Path.GetDirectoryName(path);
            DisplayName = ExpressionHelpers.NormalizePath(basePath, FullName);
            Path = ExpressionHelpers.NormalizePath(basePath, FullName);
            _TargetType = string.IsNullOrEmpty(Extension) ? System.IO.Path.GetFileNameWithoutExtension(path) : Extension;
        }

        public string FullName { get; }

        public string BasePath { get; }

        public string Name { get; }

        public string Extension { get; }

        public string DirectoryName { get; }

        public string Path { get; }

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
