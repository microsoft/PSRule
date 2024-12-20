// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Runtime;

namespace PSRule.Definitions;

internal interface IConventionV1 : IResource
{
    void Initialize(RunspaceContext context, IEnumerable input);

    void Begin(RunspaceContext context, IEnumerable input);

    void Process(RunspaceContext context, IEnumerable input);

    void End(RunspaceContext context, IEnumerable input);
}
