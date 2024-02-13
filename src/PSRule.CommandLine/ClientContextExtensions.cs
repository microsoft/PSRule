// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.IO;

namespace PSRule.CommandLine;

internal static class ClientContextExtensions
{
    private const string LOG_VERBOSE = "VERBOSE: ";
    private const string LOG_ERROR = "ERROR: ";

    public static void LogVerbose(this ClientContext context, string message, params object[] args)
    {
        if (context == null || string.IsNullOrEmpty(message) || !context.Verbose)
            return;

        var s = args != null && args.Length > 0 ? string.Format(Thread.CurrentThread.CurrentCulture, message, args) : message;
        context.Invocation.Console.WriteLine(string.Concat(LOG_VERBOSE, s));
    }

    public static void LogError(this ClientContext context, string message, params object[] args)
    {
        if (context == null || string.IsNullOrEmpty(message))
            return;

        var s = args != null && args.Length > 0 ? string.Format(Thread.CurrentThread.CurrentCulture, message, args) : message;
        context.Invocation.Console.Error.WriteLine(string.Concat(LOG_ERROR, s));
    }
}
