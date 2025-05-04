// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using PSRule.Runtime;

namespace PSRule.Definitions;

/// <summary>
/// A helper class to help validate a resource object.
/// </summary>
internal sealed class ResourceValidator(ILogger? logger) : IResourceValidator
{
    private static readonly Regex ValidName = new("^[^<>:/\\\\|?*\"'`+@._\\-\x00-\x1F][^<>:/\\\\|?*\"'`+@\x00-\x1F]{1,126}[^<>:/\\\\|?*\"'`+@._\\-\x00-\x1F]$", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private readonly ILogger? _Logger = logger;

    internal static bool IsNameValid(string name)
    {
        return !string.IsNullOrEmpty(name) && ValidName.Match(name).Success;
    }

    public bool Visit(IResource resource)
    {
        return VisitName(resource, resource.Name) &&
            VisitName(resource, resource.Ref) &&
            VisitName(resource, resource.Alias);
    }

    private bool VisitName(IResource resource, string name)
    {
        if (IsNameValid(name))
            return true;

        _Logger?.LogInvalidResourceName(name, ReportExtent(resource.Extent));
        return false;
    }

    private bool VisitName(IResource resource, ResourceId? name)
    {
        return !name.HasValue || VisitName(resource, name.Value.Name);
    }

    private bool VisitName(IResource resource, ResourceId[]? name)
    {
        if (name == null || name.Length == 0)
            return true;

        for (var i = 0; i < name.Length; i++)
            if (!VisitName(resource, name[i].Name))
                return false;

        return true;
    }

    private static string ReportExtent(ISourceExtent extent)
    {
        return string.Concat(extent.File, " line ", extent.Line);
    }
}
