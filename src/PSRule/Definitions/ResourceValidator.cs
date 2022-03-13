// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule.Definitions
{
    internal interface IResourceValidator : IResourceVisitor
    {

    }

    /// <summary>
    /// A helper class to help validate a resource object.
    /// </summary>
    internal sealed class ResourceValidator : IResourceValidator
    {
        private const string ERRORID_INVALIDRESOURCENAME = "PSRule.Parse.InvalidResourceName";

        private static readonly Regex ValidName = new Regex("^[a-zA-Z0-9][a-zA-Z0-9._-]{1,126}[a-zA-Z0-9]$", RegexOptions.Compiled);

        private readonly RunspaceContext _Context;

        public ResourceValidator(RunspaceContext context)
        {
            _Context = context;
        }

        internal static bool IsNameValid(string name)
        {
            return !string.IsNullOrEmpty(name) && ValidName.Match(name).Success;
        }

        public bool Visit(IResource resource)
        {
            return VisitName(resource);
        }

        private bool VisitName(IResource resource)
        {
            if (IsNameValid(resource.Name))
                return true;

            ReportError(ERRORID_INVALIDRESOURCENAME, PSRuleResources.InvalidResourceName, resource.Name, ReportExtent(resource.Extent));
            return false;
        }

        private static string ReportExtent(ISourceExtent extent)
        {
            return string.Concat(extent.File, " line ", extent.Line);
        }

        private void ReportError(string errorId, string message, params object[] args)
        {
            if (_Context == null)
                return;

            ReportError(new Pipeline.ParseException(
                message: string.Format(Thread.CurrentThread.CurrentCulture, message, args),
                errorId: errorId
            ));
        }

        private void ReportError(Pipeline.ParseException exception)
        {
            if (_Context == null)
                return;

            _Context.WriteError(new ErrorRecord(
                exception: exception,
                errorId: exception.ErrorId,
                errorCategory: ErrorCategory.InvalidOperation,
                targetObject: null
            ));
        }
    }
}
