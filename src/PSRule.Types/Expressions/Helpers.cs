// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Expressions;

internal static class Helpers
{
    private const char Backslash = '\\';
    private const char Slash = '/';

    internal static bool WithinPath(string actualPath, string expectedPath, bool caseSensitive)
    {
        var expected = Environment.GetRootedBasePath(expectedPath, normalize: true);
        var actual = Environment.GetRootedPath(actualPath, normalize: true);
        return actual.StartsWith(expected, ignoreCase: !caseSensitive, Thread.CurrentThread.CurrentCulture);
    }

    internal static string NormalizePath(string basePath, string? path, bool caseSensitive = true)
    {
        path = Environment.GetRootedPath(path, normalize: true, basePath: basePath);
        basePath = Environment.GetRootedBasePath(basePath, normalize: true);
        return path.Length >= basePath.Length &&
            path.StartsWith(basePath, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) ? path.Substring(basePath.Length).Replace(Backslash, Slash) : path;
    }
}
