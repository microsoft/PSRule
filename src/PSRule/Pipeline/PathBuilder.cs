// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    public interface IPathBuilder
    {
        void Add(string path);

        void Add(System.IO.FileInfo[] fileInfo);

        void Add(PathInfo[] pathInfo);

        InputFileInfo[] Build();
    }

    internal abstract class PathBuilder
    {
        // Path separators
        private const char Slash = '/';
        private const char BackSlash = '\\';

        private const char Dot = '.';
        private const string CurrentPath = ".";
        private const string RecursiveSearchOperator = "**";

        private static readonly char[] PathLiteralStopCharacters = new char[] { '*', '[', '?' };
        private static readonly char[] PathSeparatorCharacters = new char[] { '\\', '/' };

        private readonly IPipelineWriter _Logger;
        private readonly List<InputFileInfo> _Files;
        private readonly HashSet<string> _Paths;
        private readonly string _BasePath;
        private readonly string _DefaultSearchPattern;
        private readonly PathFilter _GlobalFilter;

        protected PathBuilder(IPipelineWriter logger, string basePath, string searchPattern, PathFilter filter)
        {
            _Logger = logger;
            _Files = new List<InputFileInfo>();
            _Paths = new HashSet<string>();
            _BasePath = NormalizePath(PSRuleOption.GetRootedBasePath(basePath));
            _DefaultSearchPattern = searchPattern;
            _GlobalFilter = filter;
        }

        public void Add(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            for (var i = 0; i < path.Length; i++)
                Add(path[i]);
        }

        public void Add(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            FindFiles(path);
        }

        public InputFileInfo[] Build()
        {
            try
            {
                return _Files.ToArray();
            }
            finally
            {
                _Files.Clear();
                _Paths.Clear();
            }
        }

        private void FindFiles(string path)
        {
            if (TryUrl(path) || TryPath(path, out path))
                return;

            var pathLiteral = GetSearchParameters(path, out string searchPattern, out SearchOption searchOption, out PathFilter filter);
            var files = Directory.EnumerateFiles(pathLiteral, searchPattern, searchOption);
            foreach (var file in files)
                if (ShouldInclude(file, filter))
                    AddFile(file);
        }

        private bool TryUrl(string path)
        {
            if (!path.IsUri())
                return false;

            AddFile(path);
            return true;
        }

        private bool TryPath(string path, out string normalPath)
        {
            normalPath = path;
            if (path.IndexOfAny(PathLiteralStopCharacters) > -1)
                return false;

            var rootedPath = GetRootedPath(path);
            if (Directory.Exists(rootedPath) || path == CurrentPath)
            {
                if (IsBasePath(rootedPath))
                    normalPath = CurrentPath;

                return false;
            }
            if (!File.Exists(rootedPath))
            {
                ErrorNotFound(path);
                return false;
            }
            AddFile(rootedPath);
            return true;
        }

        private bool IsBasePath(string path)
        {
            path = IsSeparator(path[path.Length - 1]) ? path : string.Concat(path, Path.DirectorySeparatorChar);
            return NormalizePath(path) == _BasePath;
        }

        private void ErrorNotFound(string path)
        {
            if (_Logger == null)
                return;

            _Logger.WriteError(new ErrorRecord(new FileNotFoundException(), "PSRule.PathBuilder.ErrorNotFound", ErrorCategory.ObjectNotFound, path));
        }

        private void AddFile(string path)
        {
            if (_Paths.Contains(path))
                return;

            _Files.Add(new InputFileInfo(_BasePath, path));
            _Paths.Add(path);
        }

        private string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(_BasePath, path));
        }

        /// <summary>
        /// Split a search path into components based on wildcards.
        /// </summary>
        private string GetSearchParameters(string path, out string searchPattern, out SearchOption searchOption, out PathFilter filter)
        {
            searchOption = SearchOption.AllDirectories;
            var pathLiteral = TrimPath(path, out bool relativeAnchor);

            if (TryFilter(pathLiteral, out searchPattern, out filter))
                return _BasePath;

            pathLiteral = SplitSearchPath(pathLiteral, out searchPattern);
            if ((relativeAnchor || !string.IsNullOrEmpty(pathLiteral)) && !string.IsNullOrEmpty(searchPattern))
                searchOption = SearchOption.TopDirectoryOnly;

            if (string.IsNullOrEmpty(searchPattern))
                searchPattern = _DefaultSearchPattern;

            return GetRootedPath(pathLiteral);
        }

        private static string SplitSearchPath(string path, out string searchPattern)
        {
            // Find the index of the first expression character i.e. out/modules/**/file
            var stopIndex = path.IndexOfAny(PathLiteralStopCharacters);

            // Track back to the separator before any expression characters
            var literalSeparator = stopIndex > -1 ? path.LastIndexOfAny(PathSeparatorCharacters, stopIndex) + 1 : path.LastIndexOfAny(PathSeparatorCharacters) + 1;
            searchPattern = path.Substring(literalSeparator, path.Length - literalSeparator);
            return path.Substring(0, literalSeparator);
        }

        private bool TryFilter(string path, out string searchPattern, out PathFilter filter)
        {
            searchPattern = null;
            filter = null;
            if (UseSimpleSearch(path))
                return false;

            filter = PathFilter.Create(_BasePath, path);
            var patternSeparator = path.LastIndexOfAny(PathSeparatorCharacters) + 1;
            searchPattern = path.Substring(patternSeparator, path.Length - patternSeparator);
            return true;
        }

        /// <summary>
        /// Remove leading ./ or .\ characters indicating a relative path anchor.
        /// </summary>
        /// <param name="path">The path to trim.</param>
        /// <param name="relativeAnchor">Returns true when a relative path anchor was present.</param>
        /// <returns>Return a clean path without the relative path anchor.</returns>
        private static string TrimPath(string path, out bool relativeAnchor)
        {
            relativeAnchor = false;
            if (path.Length >= 2 && path[0] == Dot && IsSeparator(path[1]))
            {
                relativeAnchor = true;
                return path.Remove(0, 2);
            }
            return path;
        }

        private bool ShouldInclude(string file, PathFilter filter)
        {
            return (filter == null || filter.Match(file)) &&
                (_GlobalFilter == null || _GlobalFilter.Match(file));
        }

        [DebuggerStepThrough]
        private static bool IsSeparator(char c)
        {
            return c == Slash || c == BackSlash;
        }

        /// <summary>
        /// Determines if a simple search can be used.
        /// </summary>
        [DebuggerStepThrough]
        private static bool UseSimpleSearch(string s)
        {
            return s.IndexOf(RecursiveSearchOperator, System.StringComparison.OrdinalIgnoreCase) == -1;
        }

        [DebuggerStepThrough]
        private static string NormalizePath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
