// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Runtime;

namespace PSRule.Definitions;

internal interface IConventionV1 : IResource
{
    void Initialize(LegacyRunspaceContext context, IEnumerable input);

    void Begin(LegacyRunspaceContext context, IEnumerable input);

    void Process(LegacyRunspaceContext context, IEnumerable input);

    void End(LegacyRunspaceContext context, IEnumerable input);
}
