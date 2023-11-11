// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;

namespace PSRule.Tool;

internal static class InvocationExtensions
{
    public static void Log(this InvocationContext context, string message, params object[] args)
    {
        if (context == null || string.IsNullOrEmpty(message))
            return;

        var s = args != null && args.Length > 0 ? string.Format(Thread.CurrentThread.CurrentCulture, message, args) : message;
        context.Console.WriteLine(s);
    }

    public static void LogError(this InvocationContext context, string message, params object[] args)
    {
        if (context == null || string.IsNullOrEmpty(message))
            return;

        var s = args != null && args.Length > 0 ? string.Format(Thread.CurrentThread.CurrentCulture, message, args) : message;
        context.Console.Error.Write(s);
    }
}
