// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Definitions.Conventions;

internal interface IConventionV1 : IResource
{
    /// <summary>
    /// Call to perform any initialization, such as creating global objects.
    /// Occurs once globally at the beginning of the pipeline outside of a run.
    /// </summary>
    void Initialize(IConventionContext context, IEnumerable input);

    /// <summary>
    /// Call to perform expansion, set data, or alter the object before rules are processed.
    /// Occurs once per object per run before the any rules are executed.
    /// </summary>
    void Begin(IConventionContext context, IEnumerable input);

    /// <summary>
    /// Call to perform per object tasks after rules have run such as generate badges.
    /// Occurs once per object per run after all rules are executed.
    /// </summary>
    void Process(IConventionContext context, IEnumerable input);

    /// <summary>
    /// Call to perform any finalization, such as upload results to an external service.
    /// Occurs once globally at the end of the pipeline outside of a run.
    /// </summary>
    void End(IConventionContext context, IEnumerable input);
}
