// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Host
{
    internal interface IDependencyTarget
    {
        string RuleId { get; }

        string[] DependsOn { get; }

        bool Dependency { get; }
    }
}