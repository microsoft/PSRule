// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline;

internal static class HostContextExtensions
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string InformationPreference = "InformationPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string DebugPreference = "DebugPreference";
    private const string AutoLoadingPreference = "PSModuleAutoLoadingPreference";

    public static ActionPreference GetErrorPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(ErrorPreference);
    }

    public static ActionPreference GetWarningPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(WarningPreference);
    }

    public static ActionPreference GetInformationPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(InformationPreference);
    }

    public static ActionPreference GetVerbosePreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(VerbosePreference);
    }

    public static ActionPreference GetDebugPreference(this IHostContext hostContext)
    {
        return hostContext.GetPreferenceVariable(DebugPreference);
    }

    public static PSModuleAutoLoadingPreference GetAutoLoadingPreference(this IHostContext hostContext)
    {
        return hostContext.GetVariable<PSModuleAutoLoadingPreference>(AutoLoadingPreference);
    }
}
