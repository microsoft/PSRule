// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Data;

namespace PSRule.Pipeline
{
    /// <summary>
    /// Sort paths by versions in decending order. Use orginal order when versions are compariable.
    /// </summary>
    internal sealed class ModulePathComparer : IComparer<string>
    {
        /// <inheritdoc/>
        public int Compare(string x, string y)
        {
            var x_name = Path.GetFileName(x);
            var y_name = Path.GetFileName(y);
            if (!SemanticVersion.TryParseVersion(x_name, out var x_version))
                return SemanticVersion.TryParseVersion(y_name, out _) ? 1 : 0;

            return !SemanticVersion.TryParseVersion(y_name, out var y_version) ? -1 : y_version.CompareTo(x_version);
        }
    }
}
