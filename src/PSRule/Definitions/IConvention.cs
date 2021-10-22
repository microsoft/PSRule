// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Runtime;

namespace PSRule.Definitions
{
    internal interface IConvention
    {
        string Name { get; }

        void Initialize(RunspaceContext context, IEnumerable input);

        void Begin(RunspaceContext context, IEnumerable input);

        void Process(RunspaceContext context, IEnumerable input);

        void End(RunspaceContext context, IEnumerable input);
    }
}
