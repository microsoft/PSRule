// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// Search for localized files based on culture.
/// </summary>
/// <param name="cultures">An ordered list of culture names.</param>
internal sealed class LocalizedFileSearch(string[] cultures)
{
    private readonly string[] _Cultures = cultures ?? throw new ArgumentNullException(nameof(cultures));

    public string? GetLocalizedPath(string helpPath, string fileName, out string? culture)
    {
        if (helpPath == null) throw new ArgumentNullException(nameof(helpPath));
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));

        culture = null;
        for (var i = 0; i < _Cultures.Length; i++)
        {
            var path = Path.Combine(helpPath, _Cultures[i], fileName);
            if (File.Exists(path))
            {
                culture = _Cultures[i];
                return path;
            }
        }
        return null;
    }
}
