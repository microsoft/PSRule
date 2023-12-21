// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Rules;

namespace PSRule;

internal static class SeverityLevelExtensions
{
    public static SeverityLevel GetWorstCase(this SeverityLevel o1, SeverityLevel o2)
    {
        if (o2 == SeverityLevel.Error || o1 == SeverityLevel.Error)
            return SeverityLevel.Error;
        else if (o2 == SeverityLevel.Warning || o1 == SeverityLevel.Warning)
            return SeverityLevel.Warning;

        return o2 == SeverityLevel.Information || o1 == SeverityLevel.Information ? SeverityLevel.Information : SeverityLevel.None;
    }
}
