// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Rules;

/// <summary>
/// Extension methods for rule help information.
/// </summary>
public static class RuleHelpInfoExtensions
{
    private const string ONLINE_HELP_LINK_ANNOTATION = "online version";

    /// <summary>
    /// Get the URI for the online version of the documentation.
    /// </summary>
    /// <returns>Returns the URI when a valid link is set, otherwise null is returned.</returns>
    public static Uri? GetOnlineHelpUri(this IRuleHelpInfo info)
    {
        var link = GetOnlineHelpUrl(info);
        return link == null ||
            !Uri.TryCreate(link, UriKind.Absolute, out var result) ?
            null : result;
    }

    /// <summary>
    /// Get the URL for the online version of the documentation.
    /// </summary>
    /// <returns>Returns the URL when set, otherwise null is returned.</returns>
    public static string? GetOnlineHelpUrl(this IRuleHelpInfo info)
    {
        return info == null ||
            info.Annotations == null ||
            !info.Annotations.ContainsKey(ONLINE_HELP_LINK_ANNOTATION) ?
            null : info.Annotations[ONLINE_HELP_LINK_ANNOTATION].ToString();
    }

    /// <summary>
    /// Determines if the online help link is set.
    /// </summary>
    /// <returns>Returns <c>true</c> when the online help link is set. Otherwise this method returns <c>false</c>.</returns>
    internal static bool HasOnlineHelp(this IRuleHelpInfo info)
    {
        return info != null &&
            info.Annotations != null &&
            info.Annotations.ContainsKey(ONLINE_HELP_LINK_ANNOTATION);
    }

    /// <summary>
    /// Set the online help link from the <paramref name="url"/> parameter.
    /// </summary>
    /// <param name="info">The info object.</param>
    /// <param name="url">A URL to the online help location.</param>
    internal static void SetOnlineHelpUrl(this IRuleHelpInfo info, string url)
    {
        if (info == null || info.Annotations == null || string.IsNullOrEmpty(url)) return;
        info.Annotations[ONLINE_HELP_LINK_ANNOTATION] = url;
    }
}
