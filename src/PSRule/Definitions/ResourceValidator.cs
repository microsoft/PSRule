// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Text.RegularExpressions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Definitions;

internal interface IResourceValidator : IResourceVisitor
{

}

/// <summary>
/// A helper class to help validate a resource object.
/// </summary>
internal sealed class ResourceValidator : IResourceValidator
{
    private const string ERRORID_INVALIDRESOURCENAME = "PSRule.Parse.InvalidResourceName";

    private static readonly Regex ValidName = new("^[^<>:/\\\\|?*\"'`+@._\\-\x00-\x1F][^<>:/\\\\|?*\"'`+@\x00-\x1F]{1,126}[^<>:/\\\\|?*\"'`+@._\\-\x00-\x1F]$", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private readonly IPipelineWriter _Writer;

    public ResourceValidator(IPipelineWriter writer)
    {
        _Writer = writer;
    }

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

        ReportError(ERRORID_INVALIDRESOURCENAME, PSRuleResources.InvalidResourceName, name, ReportExtent(resource.Extent));
        return false;
    }

    private bool VisitName(IResource resource, ResourceId? name)
    {
        return !name.HasValue || VisitName(resource, name.Value.Name);
    }

    private bool VisitName(IResource resource, ResourceId[] name)
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

    private void ReportError(string errorId, string message, params object[] args)
    {
        if (_Writer == null)
            return;

        ReportError(new Pipeline.ParseException(
            message: string.Format(Thread.CurrentThread.CurrentCulture, message, args),
            errorId: errorId
        ));
    }

    private void ReportError(Pipeline.ParseException exception)
    {
        if (_Writer == null)
            return;

        _Writer.WriteError(new ErrorRecord(
            exception: exception,
            errorId: exception.ErrorId,
            errorCategory: ErrorCategory.InvalidOperation,
            targetObject: null
        ));
    }
}
