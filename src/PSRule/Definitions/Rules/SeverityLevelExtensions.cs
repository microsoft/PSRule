// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Rules;

/// <summary>
/// Extensions for <see cref="SeverityLevel"/>.
/// </summary>
internal static class SeverityLevelExtensions
{
    /// <summary>
    /// Get the worst case <see cref="SeverityLevel"/>, such that Error > Warning > Information > None.
    /// </summary>
    public static SeverityLevel GetWorstCase(this SeverityLevel o1, SeverityLevel o2)
    {
        if (o2 == SeverityLevel.Error || o1 == SeverityLevel.Error)
            return SeverityLevel.Error;
        else if (o2 == SeverityLevel.Warning || o1 == SeverityLevel.Warning)
            return SeverityLevel.Warning;

        return o2 == SeverityLevel.Information || o1 == SeverityLevel.Information ? SeverityLevel.Information : SeverityLevel.None;
    }
}
