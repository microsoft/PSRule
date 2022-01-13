// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions
{
    public interface IDependencyTarget
    {
        ResourceId Id { get; }

        ResourceId? Ref { get; }

        ResourceId[] Alias { get; }

        /// <summary>
        /// Resources this target depends on.
        /// </summary>
        ResourceId[] DependsOn { get; }

        /// <summary>
        /// Determines if the source was imported as a dependency.
        /// </summary>
        bool Dependency { get; }
    }
}
